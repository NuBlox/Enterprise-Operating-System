using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Subjects;

public sealed class SubjectModule
{
    public async Task<SubjectId> CreateAsync(
        ITransactionalSession session,
        CustomerId customerId,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var id = SubjectId.New();

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO subjects.business_subjects
                (customer_id, id, display_name)
            VALUES
                (@customer_id, @id, @display_name);
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", id.Value);
        command.AddParameter("display_name", displayName.Trim());

        await command.ExecuteNonQueryAsync(cancellationToken);
        return id;
    }
}
