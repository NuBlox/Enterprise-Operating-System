using Npgsql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Infrastructure.PostgreSql;

public sealed class PostgresWorkProductAttentionReader : IWorkProductAttentionReader
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly PostgresWorkProductRepository _repository;

    public PostgresWorkProductAttentionReader(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _repository = new PostgresWorkProductRepository(dataSource);
    }

    public async Task<IReadOnlyList<WorkProductAttentionItem>> ListAsync(
        TenantId tenantId,
        PrincipalId principalId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(
            """
            SELECT
                wp.work_product_id,
                r.work_product_revision_id,
                r.revision_number,
                wp.title,
                wp.product_type,
                r.state,
                rr.review_request_id,
                rr.requested_at,
                CASE WHEN rr.review_request_id IS NOT NULL THEN 'REVIEW' ELSE 'CONTRIBUTOR' END AS attention_kind,
                CASE WHEN rr.review_request_id IS NOT NULL THEN rr.requested_at ELSE r.created_at END AS attention_since
            FROM work_products.work_products wp
            JOIN work_products.revisions r
              ON r.tenant_id = wp.tenant_id
             AND r.work_product_id = wp.work_product_id
             AND r.revision_number = wp.current_revision_number
            LEFT JOIN work_products.review_requests rr
              ON rr.tenant_id = r.tenant_id
             AND rr.work_product_revision_id = r.work_product_revision_id
             AND rr.requested_principal_id = @principal_id
             AND rr.state = 'OPEN'
            WHERE wp.tenant_id = @tenant_id
              AND (
                    rr.review_request_id IS NOT NULL
                    OR (
                        wp.owner_principal_id = @principal_id
                        AND r.state IN ('DRAFT', 'CHANGES_REQUIRED', 'REJECTED', 'APPROVED')
                    )
                  )
            ORDER BY attention_since ASC, wp.work_product_id ASC;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("principal_id", principalId.Value);

        var results = new List<WorkProductAttentionItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var reviewRequestId = reader.IsDBNull(6)
                ? (ReviewRequestId?)null
                : new ReviewRequestId(reader.GetGuid(6));
            var kind = reader.GetString(8) == "REVIEW"
                ? WorkProductAttentionKind.ReviewAction
                : WorkProductAttentionKind.ContributorAction;

            results.Add(new WorkProductAttentionItem(
                tenantId,
                kind,
                reader.GetString(3),
                reader.GetString(4),
                ParseRevisionState(reader.GetString(5)),
                reader.GetFieldValue<DateTimeOffset>(9),
                new WorkProductSourceReference(
                    new WorkProductId(reader.GetGuid(0)),
                    new WorkProductRevisionId(reader.GetGuid(1)),
                    reader.GetInt32(2),
                    reviewRequestId)));
        }

        await reader.CloseAsync().ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return results;
    }

    public async Task<WorkProductSourceView?> FindSourceAsync(
        TenantId tenantId,
        PrincipalId principalId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default)
    {
        var record = await _repository.FindAsync(tenantId, workProductId, cancellationToken).ConfigureAwait(false);
        if (record is null)
        {
            return null;
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value, cancellationToken).ConfigureAwait(false);

        if (!await PrincipalMayViewAsync(
            connection,
            transaction,
            tenantId,
            principalId,
            record,
            cancellationToken).ConfigureAwait(false))
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }

        var reviews = await ReadReviewEvidenceAsync(
            connection,
            transaction,
            tenantId,
            record.CurrentRevision.Id,
            cancellationToken).ConfigureAwait(false);
        var issue = await ReadIssueEvidenceAsync(
            connection,
            transaction,
            tenantId,
            record.CurrentRevision.Id,
            cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new WorkProductSourceView(record, reviews, issue);
    }

    private static async Task<bool> PrincipalMayViewAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        TenantId tenantId,
        PrincipalId principalId,
        WorkProductRecord record,
        CancellationToken cancellationToken)
    {
        if (record.WorkProduct.OwnerPrincipalId == principalId)
        {
            return true;
        }

        await using var command = new NpgsqlCommand(
            """
            SELECT EXISTS (
                SELECT 1
                FROM work_products.review_requests
                WHERE tenant_id = @tenant_id
                  AND work_product_revision_id = @revision_id
                  AND requested_principal_id = @principal_id
            );
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("revision_id", record.CurrentRevision.Id.Value);
        command.Parameters.AddWithValue("principal_id", principalId.Value);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) ?? false);
    }

    private static async Task<IReadOnlyList<WorkProductReviewEvidence>> ReadReviewEvidenceAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        TenantId tenantId,
        WorkProductRevisionId revisionId,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            """
            SELECT
                rr.review_request_id,
                rr.kind,
                rr.requested_principal_id,
                rr.requested_at,
                rr.state,
                rd.decision_evidence_id,
                rd.outcome,
                rd.actor_principal_id,
                rd.decided_at
            FROM work_products.review_requests rr
            LEFT JOIN work_products.review_decisions rd
              ON rd.tenant_id = rr.tenant_id
             AND rd.review_request_id = rr.review_request_id
            WHERE rr.tenant_id = @tenant_id
              AND rr.work_product_revision_id = @revision_id
            ORDER BY rr.requested_at ASC, rr.review_request_id ASC;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("revision_id", revisionId.Value);

        var results = new List<WorkProductReviewEvidence>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(new WorkProductReviewEvidence(
                new ReviewRequestId(reader.GetGuid(0)),
                ParseReviewKind(reader.GetString(1)),
                new PrincipalId(reader.GetGuid(2)),
                reader.GetFieldValue<DateTimeOffset>(3),
                ParseReviewState(reader.GetString(4)),
                reader.IsDBNull(5) ? null : new DecisionEvidenceId(reader.GetGuid(5)),
                reader.IsDBNull(6) ? null : ParseDecisionOutcome(reader.GetString(6)),
                reader.IsDBNull(7) ? null : new PrincipalId(reader.GetGuid(7)),
                reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8)));
        }

        await reader.CloseAsync().ConfigureAwait(false);
        return results;
    }

    private static async Task<WorkProductIssueEvidenceSummary?> ReadIssueEvidenceAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        TenantId tenantId,
        WorkProductRevisionId revisionId,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            """
            SELECT issue_evidence_id, approval_decision_evidence_id, issued_by_principal_id, issued_at
            FROM work_products.issue_evidence
            WHERE tenant_id = @tenant_id
              AND work_product_revision_id = @revision_id;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("revision_id", revisionId.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            await reader.CloseAsync().ConfigureAwait(false);
            return null;
        }

        var result = new WorkProductIssueEvidenceSummary(
            new IssueEvidenceId(reader.GetGuid(0)),
            new DecisionEvidenceId(reader.GetGuid(1)),
            new PrincipalId(reader.GetGuid(2)),
            reader.GetFieldValue<DateTimeOffset>(3));
        await reader.CloseAsync().ConfigureAwait(false);
        return result;
    }

    private static WorkProductRevisionState ParseRevisionState(string value) => value switch
    {
        "DRAFT" => WorkProductRevisionState.Draft,
        "IN_REVIEW" => WorkProductRevisionState.InReview,
        "CHANGES_REQUIRED" => WorkProductRevisionState.ChangesRequired,
        "REJECTED" => WorkProductRevisionState.Rejected,
        "APPROVED" => WorkProductRevisionState.Approved,
        "ISSUED" => WorkProductRevisionState.Issued,
        "SUPERSEDED" => WorkProductRevisionState.Superseded,
        _ => throw new InvalidOperationException($"Unknown Work Product revision state '{value}'.")
    };

    private static ReviewRequestKind ParseReviewKind(string value) => value switch
    {
        "REVIEW" => ReviewRequestKind.Review,
        "APPROVAL" => ReviewRequestKind.Approval,
        _ => throw new InvalidOperationException($"Unknown review request kind '{value}'.")
    };

    private static ReviewRequestState ParseReviewState(string value) => value switch
    {
        "OPEN" => ReviewRequestState.Open,
        "COMPLETED" => ReviewRequestState.Completed,
        "CANCELLED" => ReviewRequestState.Cancelled,
        _ => throw new InvalidOperationException($"Unknown review request state '{value}'.")
    };

    private static ReviewDecisionOutcome ParseDecisionOutcome(string value) => value switch
    {
        "APPROVED" => ReviewDecisionOutcome.Approved,
        "REJECTED" => ReviewDecisionOutcome.Rejected,
        _ => throw new InvalidOperationException($"Unknown review decision outcome '{value}'.")
    };
}
