using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Work;

public sealed class WorkModule
{
    public async Task<WorkId> CreateAsync(
        ITransactionalSession session,
        CustomerId customerId,
        SubjectId subjectId,
        string summary,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);

        var id = WorkId.New();

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO work.work_requests
                (customer_id, id, subject_id, summary, state)
            VALUES
                (@customer_id, @id, @subject_id, @summary, 'OPEN');
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", id.Value);
        command.AddParameter("subject_id", subjectId.Value);
        command.AddParameter("summary", summary.Trim());

        await command.ExecuteNonQueryAsync(cancellationToken);
        return id;
    }
}
