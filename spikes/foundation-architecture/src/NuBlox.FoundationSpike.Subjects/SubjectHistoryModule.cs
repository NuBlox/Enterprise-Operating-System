using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Subjects;

public sealed record SubjectNameVersion(
    string DisplayName,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset RecordedAt,
    string ChangeReason);

public sealed class SubjectHistoryModule
{
    public async Task InitialiseAsync(
        ITransactionalSession session,
        CustomerId customerId,
        SubjectId subjectId,
        string displayName,
        DateTimeOffset effectiveFrom,
        string changeReason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(changeReason);

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO subjects.business_subject_names
                (customer_id, subject_id, effective_from, effective_to, display_name, change_reason)
            VALUES
                (@customer_id, @subject_id, @effective_from, NULL, @display_name, @change_reason);
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("subject_id", subjectId.Value);
        command.AddParameter("effective_from", effectiveFrom.UtcDateTime);
        command.AddParameter("display_name", displayName.Trim());
        command.AddParameter("change_reason", changeReason.Trim());

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ChangeNameAsync(
        ITransactionalSession session,
        CustomerId customerId,
        SubjectId subjectId,
        string displayName,
        DateTimeOffset effectiveFrom,
        string changeReason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(changeReason);

        await using (var closeCurrent = session.Connection.CreateCommand())
        {
            closeCurrent.Transaction = session.Transaction;
            closeCurrent.CommandText = """
                UPDATE subjects.business_subject_names
                SET effective_to = @effective_from
                WHERE customer_id = @customer_id
                  AND subject_id = @subject_id
                  AND effective_to IS NULL
                  AND effective_from < @effective_from;
                """;
            closeCurrent.AddParameter("customer_id", customerId.Value);
            closeCurrent.AddParameter("subject_id", subjectId.Value);
            closeCurrent.AddParameter("effective_from", effectiveFrom.UtcDateTime);

            var affected = await closeCurrent.ExecuteNonQueryAsync(cancellationToken);
            if (affected != 1)
            {
                throw new InvalidOperationException(
                    "Expected exactly one open-ended subject-name version to close before creating the next version.");
            }
        }

        await InitialiseAsync(
            session,
            customerId,
            subjectId,
            displayName,
            effectiveFrom,
            changeReason,
            cancellationToken);
    }

    public async Task<SubjectNameVersion?> GetAsOfAsync(
        ITransactionalSession session,
        CustomerId customerId,
        SubjectId subjectId,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT display_name,
                   effective_from,
                   effective_to,
                   recorded_at,
                   change_reason
            FROM subjects.business_subject_names
            WHERE customer_id = @customer_id
              AND subject_id = @subject_id
              AND effective_from <= @effective_at
              AND (effective_to IS NULL OR effective_to > @effective_at)
            ORDER BY effective_from DESC
            LIMIT 1;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("subject_id", subjectId.Value);
        command.AddParameter("effective_at", effectiveAt.UtcDateTime);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new SubjectNameVersion(
            reader.GetString(0),
            reader.GetFieldValue<DateTimeOffset>(1),
            reader.IsDBNull(2) ? null : reader.GetFieldValue<DateTimeOffset>(2),
            reader.GetFieldValue<DateTimeOffset>(3),
            reader.GetString(4));
    }
}
