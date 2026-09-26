using System.Text.Json;
using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Audit;

public sealed class AuditModule
{
    public async Task<AuditEventId> AppendAsync(
        ITransactionalSession session,
        CustomerId customerId,
        string subjectType,
        Guid subjectId,
        string eventType,
        object payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);

        var id = AuditEventId.New();

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO audit.events
                (customer_id, id, subject_type, subject_id, event_type, payload)
            VALUES
                (@customer_id, @id, @subject_type, @subject_id, @event_type, CAST(@payload AS jsonb));
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", id.Value);
        command.AddParameter("subject_type", subjectType.Trim());
        command.AddParameter("subject_id", subjectId);
        command.AddParameter("event_type", eventType.Trim());
        command.AddParameter("payload", JsonSerializer.Serialize(payload));

        await command.ExecuteNonQueryAsync(cancellationToken);
        return id;
    }
}
