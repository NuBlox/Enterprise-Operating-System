using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Subjects;

public sealed class SubjectModule(SubjectHistoryModule? history = null)
{
    private readonly SubjectHistoryModule _history = history ?? new SubjectHistoryModule();

    public async Task<SubjectId> CreateAsync(
        ITransactionalSession session,
        CustomerId customerId,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var id = await CreateIdentityAsync(session, customerId, cancellationToken);

        await _history.InitialiseAsync(
            session,
            customerId,
            id,
            displayName,
            DateTimeOffset.UtcNow,
            "initial-name",
            cancellationToken);

        return id;
    }

    public async Task<SubjectId> CreateIdentityAsync(
        ITransactionalSession session,
        CustomerId customerId,
        CancellationToken cancellationToken = default)
    {
        var id = SubjectId.New();

        await using var command = session.Connection.CreateCommand();
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
        return id;
    }
}
