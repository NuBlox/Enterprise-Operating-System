using System.Data.Common;
using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Subjects;

public sealed record SubjectNameVersion(
    string DisplayName,
    string ChangeReason,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset RecordedAt);

public sealed class SubjectNameHistory
{
    public async Task AddVersionAsync(
        ITransactionalSession session,
        CustomerId customerId,
        SubjectId subjectId,
        string displayName,
        string changeReason,
        DateTimeOffset effectiveFrom,
        DateTimeOffset? effectiveTo,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(changeReason);

        if (effectiveTo is not null && effectiveTo <= effectiveFrom)
        {
            throw new ArgumentOutOfRangeException(
                nameof(effectiveTo),
                effectiveTo,
                "effectiveTo must be later than effectiveFrom.");
        }

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO subjects.business_subject_names
                (customer_id, subject_id, effective_from, effective_to, display_name, change_reason)
            VALUES
                (@customer_id, @subject_id, @effective_from, @effective_to, @display_name, @change_reason);
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("subject_id", subjectId.Value);
        command.AddParameter("effective_from", effectiveFrom);
        command.AddParameter("effective_to", effectiveTo is null ? DBNull.Value : effectiveTo.Value);
        command.AddParameter("display_name", displayName.Trim());
        command.AddParameter("change_reason", changeReason.Trim());

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ChangeNameAsync(
        ITransactionalSession session,
        CustomerId customerId,
        SubjectId subjectId,
        string newDisplayName,
        string changeReason,
        DateTimeOffset effectiveFrom,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newDisplayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(changeReason);

        await using (var closeCommand = session.Connection.CreateCommand())
        {
            closeCommand.Transaction = session.Transaction;
            closeCommand.CommandText = """
                UPDATE subjects.business_subject_names
                SET effective_to = @effective_from
                WHERE customer_id = @customer_id
                  AND subject_id = @subject_id
                  AND effective_to IS NULL
                  AND effective_from < @effective_from;
                """;
            closeCommand.AddParameter("customer_id", customerId.Value);
            closeCommand.AddParameter("subject_id", subjectId.Value);
            closeCommand.AddParameter("effective_from", effectiveFrom);

            var updated = await closeCommand.ExecuteNonQueryAsync(cancellationToken);
            if (updated != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one open name version to close for subject {subjectId}; found {updated}.");
            }
        }

        await AddVersionAsync(
            session,
            customerId,
            subjectId,
            newDisplayName,
            changeReason,
            effectiveFrom,
            effectiveTo: null,
            cancellationToken);
    }

    public async Task<SubjectNameVersion?> GetAsOfAsync(
        ITransactionalSession session,
        CustomerId customerId,
        SubjectId subjectId,
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT display_name, change_reason, effective_from, effective_to, recorded_at
            FROM subjects.business_subject_names
            WHERE customer_id = @customer_id
              AND subject_id = @subject_id
              AND effective_from <= @as_of
              AND (effective_to IS NULL OR @as_of < effective_to)
            ORDER BY effective_from DESC
            LIMIT 1;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("subject_id", subjectId.Value);
        command.AddParameter("as_of", asOf);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadVersion(reader);
    }

    public async Task<IReadOnlyList<SubjectNameVersion>> GetAllVersionsAsync(
        ITransactionalSession session,
        CustomerId customerId,
        SubjectId subjectId,
        CancellationToken cancellationToken = default)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT display_name, change_reason, effective_from, effective_to, recorded_at
            FROM subjects.business_subject_names
            WHERE customer_id = @customer_id
              AND subject_id = @subject_id
            ORDER BY effective_from;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("subject_id", subjectId.Value);

        var versions = new List<SubjectNameVersion>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            versions.Add(ReadVersion(reader));
        }

        return versions;
    }

    private static SubjectNameVersion ReadVersion(DbDataReader reader)
    {
        var effectiveFrom = reader.GetFieldValue<DateTime>(2);
        DateTimeOffset? effectiveTo = reader.IsDBNull(3)
            ? null
            : new DateTimeOffset(reader.GetFieldValue<DateTime>(3));
        var recordedAt = reader.GetFieldValue<DateTime>(4);

        return new SubjectNameVersion(
            reader.GetString(0),
            reader.GetString(1),
            new DateTimeOffset(effectiveFrom),
            effectiveTo,
            new DateTimeOffset(recordedAt));
    }
}
