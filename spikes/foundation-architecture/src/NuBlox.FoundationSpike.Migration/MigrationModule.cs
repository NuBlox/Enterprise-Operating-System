using System.Data.Common;
using System.Text.Json;
using NuBlox.FoundationSpike.Shared;
using NuBlox.FoundationSpike.Subjects;

namespace NuBlox.FoundationSpike.Migration;

public sealed record MigrationReconciliation(
    int StagedCount,
    int LoadedCount,
    int ExceptionCount,
    int UnresolvedCount);

public sealed class MigrationModule(
    SubjectModule subjects,
    SubjectHistoryModule subjectHistory)
{
    public async Task<Guid> CreateBatchAsync(
        ITransactionalSession session,
        CustomerId customerId,
        string sourceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        var batchId = Guid.NewGuid();

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO migration.batches
                (customer_id, id, source_name, state)
            VALUES
                (@customer_id, @id, @source_name, 'STAGING');
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", batchId);
        command.AddParameter("source_name", sourceName.Trim());
        await command.ExecuteNonQueryAsync(cancellationToken);
        return batchId;
    }

    public async Task StageSubjectAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid batchId,
        string sourceId,
        string? displayName,
        DateTimeOffset effectiveFrom,
        object sourcePayload,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO migration.staged_subjects
                (customer_id, batch_id, source_id, display_name, effective_from, source_payload)
            VALUES
                (@customer_id, @batch_id, @source_id, @display_name, @effective_from, CAST(@source_payload AS jsonb));
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("batch_id", batchId);
        command.AddParameter("source_id", sourceId.Trim());
        command.AddParameter("display_name", string.IsNullOrWhiteSpace(displayName) ? DBNull.Value : displayName.Trim());
        command.AddParameter("effective_from", effectiveFrom);
        command.AddParameter("source_payload", JsonSerializer.Serialize(sourcePayload));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<MigrationReconciliation> ProcessBatchAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var staged = await ReadStagedAsync(session, customerId, batchId, cancellationToken);

        foreach (var row in staged)
        {
            if (await IsLoadedAsync(session, customerId, batchId, row.SourceId, cancellationToken))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.DisplayName))
            {
                await RecordExceptionAsync(
                    session,
                    customerId,
                    batchId,
                    row.SourceId,
                    "DISPLAY_NAME_REQUIRED",
                    "Source record has no usable display name.",
                    cancellationToken);
                continue;
            }

            if (await HasUnresolvedExceptionAsync(session, customerId, batchId, row.SourceId, cancellationToken))
            {
                continue;
            }

            var subjectId = await subjects.CreateIdentityAsync(
                session,
                customerId,
                cancellationToken);

            await subjectHistory.InitialiseAsync(
                session,
                customerId,
                subjectId,
                row.DisplayName,
                row.EffectiveFrom,
                $"Migrated from source ID {row.SourceId}",
                cancellationToken);

            await RecordLoadedAsync(
                session,
                customerId,
                batchId,
                row.SourceId,
                subjectId,
                cancellationToken);
        }

        var reconciliation = await CalculateReconciliationAsync(
            session,
            customerId,
            batchId,
            cancellationToken);

        await using (var reconcileCommand = session.Connection.CreateCommand())
        {
            reconcileCommand.Transaction = session.Transaction;
            reconcileCommand.CommandText = """
                INSERT INTO migration.reconciliations
                    (customer_id, batch_id, staged_count, loaded_count, exception_count, unresolved_count)
                VALUES
                    (@customer_id, @batch_id, @staged_count, @loaded_count, @exception_count, @unresolved_count)
                ON CONFLICT (customer_id, batch_id)
                DO UPDATE SET
                    staged_count = EXCLUDED.staged_count,
                    loaded_count = EXCLUDED.loaded_count,
                    exception_count = EXCLUDED.exception_count,
                    unresolved_count = EXCLUDED.unresolved_count,
                    reconciled_at = clock_timestamp();
                """;
            reconcileCommand.AddParameter("customer_id", customerId.Value);
            reconcileCommand.AddParameter("batch_id", batchId);
            reconcileCommand.AddParameter("staged_count", reconciliation.StagedCount);
            reconcileCommand.AddParameter("loaded_count", reconciliation.LoadedCount);
            reconcileCommand.AddParameter("exception_count", reconciliation.ExceptionCount);
            reconcileCommand.AddParameter("unresolved_count", reconciliation.UnresolvedCount);
            await reconcileCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var stateCommand = session.Connection.CreateCommand())
        {
            stateCommand.Transaction = session.Transaction;
            stateCommand.CommandText = """
                UPDATE migration.batches
                SET state = 'PROCESSED'
                WHERE customer_id = @customer_id
                  AND id = @batch_id;
                """;
            stateCommand.AddParameter("customer_id", customerId.Value);
            stateCommand.AddParameter("batch_id", batchId);
            await stateCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        return reconciliation;
    }

    private static async Task<IReadOnlyList<StagedSubject>> ReadStagedAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid batchId,
        CancellationToken cancellationToken)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT source_id, display_name, effective_from
            FROM migration.staged_subjects
            WHERE customer_id = @customer_id
              AND batch_id = @batch_id
            ORDER BY source_id;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("batch_id", batchId);

        var rows = new List<StagedSubject>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new StagedSubject(
                reader.GetString(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                new DateTimeOffset(reader.GetFieldValue<DateTime>(2))));
        }

        return rows;
    }

    private static async Task<bool> IsLoadedAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid batchId,
        string sourceId,
        CancellationToken cancellationToken)
        => await ExistsAsync(
            session,
            "migration.loaded_subjects",
            customerId,
            batchId,
            sourceId,
            extraPredicate: null,
            cancellationToken);

    private static async Task<bool> HasUnresolvedExceptionAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid batchId,
        string sourceId,
        CancellationToken cancellationToken)
        => await ExistsAsync(
            session,
            "migration.exceptions",
            customerId,
            batchId,
            sourceId,
            "AND disposition = 'UNRESOLVED'",
            cancellationToken);

    private static async Task<bool> ExistsAsync(
        ITransactionalSession session,
        string trustedTable,
        CustomerId customerId,
        Guid batchId,
        string sourceId,
        string? extraPredicate,
        CancellationToken cancellationToken)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = $"""
            SELECT EXISTS (
                SELECT 1
                FROM {trustedTable}
                WHERE customer_id = @customer_id
                  AND batch_id = @batch_id
                  AND source_id = @source_id
                  {extraPredicate ?? string.Empty}
            );
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("batch_id", batchId);
        command.AddParameter("source_id", sourceId);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return value is true;
    }

    private static async Task RecordExceptionAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid batchId,
        string sourceId,
        string code,
        string message,
        CancellationToken cancellationToken)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO migration.exceptions
                (customer_id, batch_id, source_id, code, message, disposition)
            VALUES
                (@customer_id, @batch_id, @source_id, @code, @message, 'UNRESOLVED')
            ON CONFLICT (customer_id, batch_id, source_id, code)
            DO NOTHING;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("batch_id", batchId);
        command.AddParameter("source_id", sourceId);
        command.AddParameter("code", code);
        command.AddParameter("message", message);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task RecordLoadedAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid batchId,
        string sourceId,
        SubjectId subjectId,
        CancellationToken cancellationToken)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO migration.loaded_subjects
                (customer_id, batch_id, source_id, subject_id)
            VALUES
                (@customer_id, @batch_id, @source_id, @subject_id);
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("batch_id", batchId);
        command.AddParameter("source_id", sourceId);
        command.AddParameter("subject_id", subjectId.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<MigrationReconciliation> CalculateReconciliationAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid batchId,
        CancellationToken cancellationToken)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT
                (SELECT count(*) FROM migration.staged_subjects
                 WHERE customer_id = @customer_id AND batch_id = @batch_id),
                (SELECT count(*) FROM migration.loaded_subjects
                 WHERE customer_id = @customer_id AND batch_id = @batch_id),
                (SELECT count(DISTINCT source_id) FROM migration.exceptions
                 WHERE customer_id = @customer_id AND batch_id = @batch_id),
                (SELECT count(DISTINCT source_id) FROM migration.exceptions
                 WHERE customer_id = @customer_id AND batch_id = @batch_id
                   AND disposition = 'UNRESOLVED');
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("batch_id", batchId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Migration reconciliation query returned no row.");
        }

        return new MigrationReconciliation(
            Convert.ToInt32(reader.GetInt64(0)),
            Convert.ToInt32(reader.GetInt64(1)),
            Convert.ToInt32(reader.GetInt64(2)),
            Convert.ToInt32(reader.GetInt64(3)));
    }

    private sealed record StagedSubject(
        string SourceId,
        string? DisplayName,
        DateTimeOffset EffectiveFrom);
}
