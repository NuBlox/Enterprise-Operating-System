using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Decisions;

public sealed class DecisionModule
{
    private static readonly HashSet<string> AllowedOutcomes =
        new(StringComparer.OrdinalIgnoreCase) { "APPROVED", "REJECTED" };

    public async Task<DecisionId> CreateAsync(
        ITransactionalSession session,
        CustomerId customerId,
        WorkId workId,
        string outcome,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);
        var normalised = outcome.Trim().ToUpperInvariant();

        if (!AllowedOutcomes.Contains(normalised))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Outcome must be APPROVED or REJECTED for this spike.");
        }

        var id = DecisionId.New();

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO decisions.approval_decisions
                (customer_id, id, work_id, outcome)
            VALUES
                (@customer_id, @id, @work_id, @outcome);
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", id.Value);
        command.AddParameter("work_id", workId.Value);
        command.AddParameter("outcome", normalised);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return id;
    }
}
