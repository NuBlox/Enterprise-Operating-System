using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Subjects;

public sealed class SubjectModule(SubjectNameHistory? nameHistory = null)
{
    private readonly SubjectNameHistory _nameHistory = nameHistory ?? new SubjectNameHistory();

    public async Task<SubjectId> CreateAsync(
        ITransactionalSession session,
        CustomerId customerId,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var id = SubjectId.New();
        var effectiveFrom = DateTimeOffset.UtcNow;

        await using (var command = session.Connection.CreateCommand())
        {
            command.Transaction = session.Transaction;
            command.CommandText = """
                INSERT INTO subjects.business_subjects
                    (customer_id, id)
                VALUES
                    (@customer_id, @id);
                """;
            command.AddParameter("customer_id", customerId.Value);
            command.AddParameter("id", id.Value);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await _nameHistory.AddVersionAsync(
            session,
            customerId,
            id,
            displayName,
            changeReason: "initial-name",
            effectiveFrom,
            effectiveTo: null,
            cancellationToken);

        return id;
    }
}
