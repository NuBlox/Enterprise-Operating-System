using Npgsql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Infrastructure.PostgreSql;

public sealed class PostgresWorkProductDeliveryStore : IWorkProductDeliveryStore
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresWorkProductDeliveryStore(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task<WorkProductDeliveryClaim?> ClaimNextAsync(
        TenantId tenantId,
        string workerId,
        DateTimeOffset nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        var canonicalWorker = CanonicalWorkerId(workerId);
        if (nowUtc == default) throw new ArgumentException("Claim timestamp is required.", nameof(nowUtc));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(leaseDuration, TimeSpan.Zero);

        var leaseExpiresAt = nowUtc.ToUniversalTime().Add(leaseDuration);
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(
            """
            WITH candidate AS (
                SELECT delivery_intent_id
                FROM work_products.delivery_intents
                WHERE tenant_id = @tenant_id
                  AND (
                        (state = 'PENDING' AND next_attempt_at <= @now)
                        OR
                        (state = 'PROCESSING' AND claim_expires_at <= @now)
                      )
                ORDER BY next_attempt_at ASC, created_at ASC, delivery_intent_id ASC
                FOR UPDATE SKIP LOCKED
                LIMIT 1
            )
            UPDATE work_products.delivery_intents intent
               SET state = 'PROCESSING',
                   attempt_count = intent.attempt_count + 1,
                   claimed_by = @worker_id,
                   claim_expires_at = @lease_expires_at
              FROM candidate
             WHERE intent.tenant_id = @tenant_id
               AND intent.delivery_intent_id = candidate.delivery_intent_id
            RETURNING intent.delivery_intent_id,
                      intent.work_product_id,
                      intent.work_product_revision_id,
                      intent.issue_evidence_id,
                      intent.consequence_type,
                      intent.idempotency_key,
                      intent.created_at,
                      intent.correlation_id,
                      intent.attempt_count;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("now", nowUtc.ToUniversalTime());
        command.Parameters.AddWithValue("worker_id", canonicalWorker);
        command.Parameters.AddWithValue("lease_expires_at", leaseExpiresAt);

        WorkProductDeliveryClaim? claim = null;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var intent = new WorkProductDeliveryIntent(
                    new WorkProductDeliveryIntentId(reader.GetGuid(0)),
                    tenantId,
                    new WorkProductId(reader.GetGuid(1)),
                    new WorkProductRevisionId(reader.GetGuid(2)),
                    new IssueEvidenceId(reader.GetGuid(3)),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetFieldValue<DateTimeOffset>(6),
                    reader.IsDBNull(7) ? null : reader.GetString(7));
                claim = new WorkProductDeliveryClaim(intent, reader.GetInt32(8), canonicalWorker, leaseExpiresAt);
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return claim;
    }

    public Task RecordCompletedAsync(
        TenantId tenantId,
        WorkProductDeliveryIntentId intentId,
        string workerId,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (completedAtUtc == default) throw new ArgumentException("Completion timestamp is required.", nameof(completedAtUtc));
        return UpdateClaimAsync(
            tenantId,
            intentId,
            workerId,
            """
            UPDATE work_products.delivery_intents
               SET state = 'COMPLETED',
                   claimed_by = NULL,
                   claim_expires_at = NULL,
                   last_failure_code = NULL,
                   completed_at = @completed_at
             WHERE tenant_id = @tenant_id
               AND delivery_intent_id = @intent_id
               AND state = 'PROCESSING'
               AND claimed_by = @worker_id;
            """,
            command => command.Parameters.AddWithValue("completed_at", completedAtUtc.ToUniversalTime()),
            cancellationToken);
    }

    public Task RecordRetryAsync(
        TenantId tenantId,
        WorkProductDeliveryIntentId intentId,
        string workerId,
        DateTimeOffset nextAttemptAtUtc,
        string failureCode,
        CancellationToken cancellationToken = default)
    {
        if (nextAttemptAtUtc == default) throw new ArgumentException("Next-attempt timestamp is required.", nameof(nextAttemptAtUtc));
        var canonicalFailure = CanonicalFailureCode(failureCode);
        return UpdateClaimAsync(
            tenantId,
            intentId,
            workerId,
            """
            UPDATE work_products.delivery_intents
               SET state = 'PENDING',
                   next_attempt_at = @next_attempt_at,
                   claimed_by = NULL,
                   claim_expires_at = NULL,
                   last_failure_code = @failure_code
             WHERE tenant_id = @tenant_id
               AND delivery_intent_id = @intent_id
               AND state = 'PROCESSING'
               AND claimed_by = @worker_id;
            """,
            command =>
            {
                command.Parameters.AddWithValue("next_attempt_at", nextAttemptAtUtc.ToUniversalTime());
                command.Parameters.AddWithValue("failure_code", canonicalFailure);
            },
            cancellationToken);
    }

    public Task RecordFailedAsync(
        TenantId tenantId,
        WorkProductDeliveryIntentId intentId,
        string workerId,
        string failureCode,
        CancellationToken cancellationToken = default)
    {
        var canonicalFailure = CanonicalFailureCode(failureCode);
        return UpdateClaimAsync(
            tenantId,
            intentId,
            workerId,
            """
            UPDATE work_products.delivery_intents
               SET state = 'FAILED',
                   claimed_by = NULL,
                   claim_expires_at = NULL,
                   last_failure_code = @failure_code
             WHERE tenant_id = @tenant_id
               AND delivery_intent_id = @intent_id
               AND state = 'PROCESSING'
               AND claimed_by = @worker_id;
            """,
            command => command.Parameters.AddWithValue("failure_code", canonicalFailure),
            cancellationToken);
    }

    public async Task<WorkProductDeliveryStatus?> FindAsync(
        TenantId tenantId,
        WorkProductDeliveryIntentId intentId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value, cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(
            """
            SELECT work_product_id, work_product_revision_id, issue_evidence_id,
                   consequence_type, idempotency_key, created_at, correlation_id,
                   state, attempt_count, next_attempt_at, claimed_by, claim_expires_at,
                   last_failure_code, completed_at
              FROM work_products.delivery_intents
             WHERE tenant_id = @tenant_id
               AND delivery_intent_id = @intent_id;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("intent_id", intentId.Value);

        WorkProductDeliveryStatus? status = null;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var intent = new WorkProductDeliveryIntent(
                    intentId,
                    tenantId,
                    new WorkProductId(reader.GetGuid(0)),
                    new WorkProductRevisionId(reader.GetGuid(1)),
                    new IssueEvidenceId(reader.GetGuid(2)),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetFieldValue<DateTimeOffset>(5),
                    reader.IsDBNull(6) ? null : reader.GetString(6));
                status = new WorkProductDeliveryStatus(
                    intent,
                    ParseState(reader.GetString(7)),
                    reader.GetInt32(8),
                    reader.IsDBNull(9) ? null : reader.GetFieldValue<DateTimeOffset>(9),
                    reader.IsDBNull(10) ? null : reader.GetString(10),
                    reader.IsDBNull(11) ? null : reader.GetFieldValue<DateTimeOffset>(11),
                    reader.IsDBNull(12) ? null : reader.GetString(12),
                    reader.IsDBNull(13) ? null : reader.GetFieldValue<DateTimeOffset>(13));
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return status;
    }

    private async Task UpdateClaimAsync(
        TenantId tenantId,
        WorkProductDeliveryIntentId intentId,
        string workerId,
        string sql,
        Action<NpgsqlCommand> addParameters,
        CancellationToken cancellationToken)
    {
        var canonicalWorker = CanonicalWorkerId(workerId);
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value, cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("intent_id", intentId.Value);
        command.Parameters.AddWithValue("worker_id", canonicalWorker);
        addParameters(command);

        if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new InvalidOperationException("The delivery intent is not held by the expected worker claim.");
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string CanonicalWorkerId(string workerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        var value = workerId.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 128, nameof(workerId));
        return value;
    }

    private static string CanonicalFailureCode(string failureCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failureCode);
        var value = failureCode.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 120, nameof(failureCode));
        return value;
    }

    private static WorkProductDeliveryState ParseState(string value) => value switch
    {
        "PENDING" => WorkProductDeliveryState.Pending,
        "PROCESSING" => WorkProductDeliveryState.Processing,
        "COMPLETED" => WorkProductDeliveryState.Completed,
        "FAILED" => WorkProductDeliveryState.Failed,
        _ => throw new InvalidOperationException($"Unknown delivery state '{value}'.")
    };
}
