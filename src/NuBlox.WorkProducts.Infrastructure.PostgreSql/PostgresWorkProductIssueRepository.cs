using Npgsql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Infrastructure.PostgreSql;

public sealed class PostgresWorkProductIssueRepository : IWorkProductIssueRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly PostgresWorkProductRepository _workProducts;

    public PostgresWorkProductIssueRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _workProducts = new PostgresWorkProductRepository(dataSource);
    }

    public async Task<WorkProductIssueContext?> FindIssueContextAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default)
    {
        var record = await _workProducts.FindAsync(tenantId, workProductId, cancellationToken).ConfigureAwait(false);
        if (record is null || record.CurrentRevision.State != WorkProductRevisionState.Approved)
        {
            return null;
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(
            """
            SELECT decision_evidence_id
            FROM work_products.review_decisions
            WHERE tenant_id = @tenant_id
              AND work_product_revision_id = @revision_id
              AND outcome = 'APPROVED'
            ORDER BY decided_at DESC, decision_evidence_id DESC
            LIMIT 1;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("revision_id", record.CurrentRevision.Id.Value);

        var decisionId = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return decisionId is Guid value
            ? new WorkProductIssueContext(record, new DecisionEvidenceId(value))
            : null;
    }

    public async Task RecordIssueAsync(
        WorkProductIssueResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var revision = result.Revision;
        var evidence = result.Evidence;
        var deliveryIntent = result.DeliveryIntent
            ?? throw new InvalidOperationException("A durable delivery intent is required when issuing a Work Product revision.");

        if (revision.State != WorkProductRevisionState.Issued
            || revision.TenantId != evidence.TenantId
            || revision.WorkProductId != evidence.WorkProductId
            || revision.Id != evidence.WorkProductRevisionId
            || revision.IssuedByPrincipalId != evidence.IssuedByPrincipalId
            || revision.IssuedAtUtc != evidence.IssuedAtUtc
            || deliveryIntent.TenantId != evidence.TenantId
            || deliveryIntent.WorkProductId != evidence.WorkProductId
            || deliveryIntent.WorkProductRevisionId != evidence.WorkProductRevisionId
            || deliveryIntent.IssueEvidenceId != evidence.Id)
        {
            throw new InvalidOperationException("Issued revision, issue evidence and delivery intent are inconsistent.");
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, evidence.TenantId.Value, cancellationToken).ConfigureAwait(false);

        await using (var supersede = new NpgsqlCommand(
            """
            UPDATE work_products.revisions
               SET state = 'SUPERSEDED'
             WHERE tenant_id = @tenant_id
               AND work_product_id = @work_product_id
               AND work_product_revision_id <> @revision_id
               AND state = 'ISSUED';
            """,
            connection,
            transaction))
        {
            supersede.Parameters.AddWithValue("tenant_id", evidence.TenantId.Value);
            supersede.Parameters.AddWithValue("work_product_id", evidence.WorkProductId.Value);
            supersede.Parameters.AddWithValue("revision_id", evidence.WorkProductRevisionId.Value);
            await supersede.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var issueRevision = new NpgsqlCommand(
            """
            UPDATE work_products.revisions
               SET state = 'ISSUED',
                   issued_by_principal_id = @issued_by,
                   issued_at = @issued_at
             WHERE tenant_id = @tenant_id
               AND work_product_revision_id = @revision_id
               AND state = 'APPROVED';
            """,
            connection,
            transaction))
        {
            issueRevision.Parameters.AddWithValue("issued_by", evidence.IssuedByPrincipalId.Value);
            issueRevision.Parameters.AddWithValue("issued_at", evidence.IssuedAtUtc);
            issueRevision.Parameters.AddWithValue("tenant_id", evidence.TenantId.Value);
            issueRevision.Parameters.AddWithValue("revision_id", evidence.WorkProductRevisionId.Value);

            if (await issueRevision.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new WorkProductStateConflictException("The revision is no longer Approved and cannot be issued.");
            }
        }

        await using (var insert = new NpgsqlCommand(
            """
            INSERT INTO work_products.issue_evidence (
                tenant_id, issue_evidence_id, work_product_id, work_product_revision_id,
                approval_decision_evidence_id, approval_outcome,
                issued_by_principal_id, issued_at, correlation_id)
            VALUES (
                @tenant_id, @issue_evidence_id, @work_product_id, @revision_id,
                @approval_decision_id, 'APPROVED',
                @issued_by, @issued_at, @correlation_id);
            """,
            connection,
            transaction))
        {
            insert.Parameters.AddWithValue("tenant_id", evidence.TenantId.Value);
            insert.Parameters.AddWithValue("issue_evidence_id", evidence.Id.Value);
            insert.Parameters.AddWithValue("work_product_id", evidence.WorkProductId.Value);
            insert.Parameters.AddWithValue("revision_id", evidence.WorkProductRevisionId.Value);
            insert.Parameters.AddWithValue("approval_decision_id", evidence.ApprovalDecisionEvidenceId.Value);
            insert.Parameters.AddWithValue("issued_by", evidence.IssuedByPrincipalId.Value);
            insert.Parameters.AddWithValue("issued_at", evidence.IssuedAtUtc);
            insert.Parameters.AddWithValue("correlation_id", evidence.CorrelationId is { } correlation ? correlation : DBNull.Value);
            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var outbox = new NpgsqlCommand(
            """
            INSERT INTO work_products.delivery_intents (
                tenant_id, delivery_intent_id, work_product_id, work_product_revision_id,
                issue_evidence_id, consequence_type, idempotency_key, created_at,
                correlation_id, state, attempt_count, next_attempt_at)
            VALUES (
                @tenant_id, @intent_id, @work_product_id, @revision_id,
                @issue_evidence_id, @consequence_type, @idempotency_key, @created_at,
                @correlation_id, 'PENDING', 0, @created_at);
            """,
            connection,
            transaction))
        {
            outbox.Parameters.AddWithValue("tenant_id", deliveryIntent.TenantId.Value);
            outbox.Parameters.AddWithValue("intent_id", deliveryIntent.Id.Value);
            outbox.Parameters.AddWithValue("work_product_id", deliveryIntent.WorkProductId.Value);
            outbox.Parameters.AddWithValue("revision_id", deliveryIntent.WorkProductRevisionId.Value);
            outbox.Parameters.AddWithValue("issue_evidence_id", deliveryIntent.IssueEvidenceId.Value);
            outbox.Parameters.AddWithValue("consequence_type", deliveryIntent.ConsequenceType);
            outbox.Parameters.AddWithValue("idempotency_key", deliveryIntent.IdempotencyKey);
            outbox.Parameters.AddWithValue("created_at", deliveryIntent.CreatedAtUtc);
            outbox.Parameters.AddWithValue("correlation_id", deliveryIntent.CorrelationId is { } correlation ? correlation : DBNull.Value);
            await outbox.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
