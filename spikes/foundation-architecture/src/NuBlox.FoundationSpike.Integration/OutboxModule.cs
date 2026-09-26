using System.Data.Common;
using System.Text.Json;
using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Integration;

public readonly record struct OutboxMessageId(Guid Value)
{
    public static OutboxMessageId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public sealed record OutboxMessage(
    CustomerId CustomerId,
    OutboxMessageId Id,
    string IdempotencyKey,
    string Operation,
    string SubjectType,
    Guid SubjectId,
    string PayloadJson,
    string State,
    int AttemptCount,
    DateTimeOffset AvailableAt,
    DateTimeOffset? LockedAt,
    string? LockedBy,
    string? LastError,
    string? ProviderRequestId);

public sealed class OutboxModule
{
    public async Task<OutboxMessageId> EnqueueAsync(
        ITransactionalSession session,
        CustomerId customerId,
        string idempotencyKey,
        string operation,
        string subjectType,
        Guid subjectId,
        object payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectType);

        var proposedId = OutboxMessageId.New();

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO integration.outbox
                (customer_id, id, idempotency_key, operation, subject_type, subject_id, payload, state)
            VALUES
                (@customer_id, @id, @idempotency_key, @operation, @subject_type, @subject_id, CAST(@payload AS jsonb), 'PENDING')
            ON CONFLICT (customer_id, idempotency_key)
            DO UPDATE SET idempotency_key = EXCLUDED.idempotency_key
            RETURNING id;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", proposedId.Value);
        command.AddParameter("idempotency_key", idempotencyKey.Trim());
        command.AddParameter("operation", operation.Trim());
        command.AddParameter("subject_type", subjectType.Trim());
        command.AddParameter("subject_id", subjectId);
        command.AddParameter("payload", JsonSerializer.Serialize(payload));

        var returned = await command.ExecuteScalarAsync(cancellationToken)
            ?? throw new InvalidOperationException("Outbox enqueue did not return an identifier.");

        return new OutboxMessageId((Guid)returned);
    }

    public async Task<OutboxMessage?> ClaimNextAsync(
        ITransactionalSession session,
        CustomerId customerId,
        string workerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            WITH candidate AS (
                SELECT id
                FROM integration.outbox
                WHERE customer_id = @customer_id
                  AND state IN ('PENDING', 'RETRY_WAIT')
                  AND available_at <= clock_timestamp()
                ORDER BY created_at, id
                FOR UPDATE SKIP LOCKED
                LIMIT 1
            )
            UPDATE integration.outbox AS o
            SET state = 'PROCESSING',
                attempt_count = o.attempt_count + 1,
                locked_at = clock_timestamp(),
                locked_by = @worker_id,
                last_error = NULL
            FROM candidate
            WHERE o.customer_id = @customer_id
              AND o.id = candidate.id
            RETURNING
                o.customer_id,
                o.id,
                o.idempotency_key,
                o.operation,
                o.subject_type,
                o.subject_id,
                o.payload::text,
                o.state,
                o.attempt_count,
                o.available_at,
                o.locked_at,
                o.locked_by,
                o.last_error,
                o.provider_request_id;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("worker_id", workerId.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? ReadMessage(reader)
            : null;
    }

    public Task MarkSucceededAsync(
        ITransactionalSession session,
        CustomerId customerId,
        OutboxMessageId messageId,
        string providerRequestId,
        CancellationToken cancellationToken = default)
        => UpdateTerminalAsync(
            session,
            customerId,
            messageId,
            state: "SUCCEEDED",
            providerRequestId,
            lastError: null,
            completed: true,
            cancellationToken);

    public async Task MarkRetryAsync(
        ITransactionalSession session,
        CustomerId customerId,
        OutboxMessageId messageId,
        string error,
        DateTimeOffset availableAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            UPDATE integration.outbox
            SET state = 'RETRY_WAIT',
                available_at = @available_at,
                locked_at = NULL,
                locked_by = NULL,
                last_error = @last_error
            WHERE customer_id = @customer_id
              AND id = @id
              AND state = 'PROCESSING';
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", messageId.Value);
        command.AddParameter("available_at", availableAt);
        command.AddParameter("last_error", error.Trim());

        EnsureSingleRow(await command.ExecuteNonQueryAsync(cancellationToken), messageId, "mark retry");
    }

    public Task MarkFailedActionRequiredAsync(
        ITransactionalSession session,
        CustomerId customerId,
        OutboxMessageId messageId,
        string error,
        string? providerRequestId,
        CancellationToken cancellationToken = default)
        => UpdateTerminalAsync(
            session,
            customerId,
            messageId,
            state: "FAILED_ACTION_REQUIRED",
            providerRequestId,
            error,
            completed: false,
            cancellationToken);

    public async Task<int> RecoverAbandonedAsync(
        ITransactionalSession session,
        CustomerId customerId,
        DateTimeOffset lockedBefore,
        DateTimeOffset retryAt,
        CancellationToken cancellationToken = default)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            UPDATE integration.outbox
            SET state = 'RETRY_WAIT',
                available_at = @retry_at,
                locked_at = NULL,
                locked_by = NULL,
                last_error = 'Recovered abandoned PROCESSING message'
            WHERE customer_id = @customer_id
              AND state = 'PROCESSING'
              AND locked_at <= @locked_before;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("locked_before", lockedBefore);
        command.AddParameter("retry_at", retryAt);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<OutboxMessage?> GetAsync(
        ITransactionalSession session,
        CustomerId customerId,
        OutboxMessageId messageId,
        CancellationToken cancellationToken = default)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT
                customer_id,
                id,
                idempotency_key,
                operation,
                subject_type,
                subject_id,
                payload::text,
                state,
                attempt_count,
                available_at,
                locked_at,
                locked_by,
                last_error,
                provider_request_id
            FROM integration.outbox
            WHERE customer_id = @customer_id
              AND id = @id;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", messageId.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? ReadMessage(reader)
            : null;
    }

    private static async Task UpdateTerminalAsync(
        ITransactionalSession session,
        CustomerId customerId,
        OutboxMessageId messageId,
        string state,
        string? providerRequestId,
        string? lastError,
        bool completed,
        CancellationToken cancellationToken)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            UPDATE integration.outbox
            SET state = @state,
                provider_request_id = @provider_request_id,
                last_error = @last_error,
                locked_at = NULL,
                locked_by = NULL,
                completed_at = CASE WHEN @completed THEN clock_timestamp() ELSE NULL END
            WHERE customer_id = @customer_id
              AND id = @id
              AND state = 'PROCESSING';
            """;
        command.AddParameter("state", state);
        command.AddParameter("provider_request_id", providerRequestId is null ? DBNull.Value : providerRequestId);
        command.AddParameter("last_error", lastError is null ? DBNull.Value : lastError);
        command.AddParameter("completed", completed);
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", messageId.Value);

        EnsureSingleRow(await command.ExecuteNonQueryAsync(cancellationToken), messageId, $"set {state}");
    }

    private static OutboxMessage ReadMessage(DbDataReader reader)
        => new(
            new CustomerId(reader.GetGuid(0)),
            new OutboxMessageId(reader.GetGuid(1)),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetGuid(5),
            reader.GetString(6),
            reader.GetString(7),
            reader.GetInt32(8),
            new DateTimeOffset(reader.GetFieldValue<DateTime>(9)),
            reader.IsDBNull(10) ? null : new DateTimeOffset(reader.GetFieldValue<DateTime>(10)),
            reader.IsDBNull(11) ? null : reader.GetString(11),
            reader.IsDBNull(12) ? null : reader.GetString(12),
            reader.IsDBNull(13) ? null : reader.GetString(13));

    private static void EnsureSingleRow(int affected, OutboxMessageId id, string operation)
    {
        if (affected != 1)
        {
            throw new InvalidOperationException(
                $"Expected one outbox row to {operation} for {id}; affected {affected}.");
        }
    }
}
