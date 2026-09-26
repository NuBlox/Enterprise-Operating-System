using Npgsql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Infrastructure.PostgreSql;

/// <summary>
/// Read-only operational projection derived directly from governed WorkProducts records.
/// No secondary authoritative state is created.
/// </summary>
public sealed class PostgresWorkProductAttentionQuery : IWorkProductAttentionQuery
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresWorkProductAttentionQuery(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task<WorkProductAttentionView> QueryAsync(
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
                attention_kind,
                attention_reason,
                title,
                revision_state,
                work_product_id,
                work_product_revision_id,
                revision_number,
                review_request_id,
                attention_since
            FROM (
                SELECT
                    'CONTRIBUTOR_ACTION'::text AS attention_kind,
                    CASE r.state
                        WHEN 'DRAFT' THEN 'DRAFT_REQUIRES_ACTION'
                        WHEN 'CHANGES_REQUIRED' THEN 'CHANGES_REQUIRED'
                    END AS attention_reason,
                    p.title,
                    r.state AS revision_state,
                    p.work_product_id,
                    r.work_product_revision_id,
                    r.revision_number,
                    NULL::uuid AS review_request_id,
                    COALESCE(r.submitted_at, r.created_at) AS attention_since
                FROM work_products.work_products p
                JOIN work_products.revisions r
                  ON r.tenant_id = p.tenant_id
                 AND r.work_product_id = p.work_product_id
                 AND r.revision_number = p.current_revision_number
                WHERE p.tenant_id = @tenant_id
                  AND p.lifecycle = 'ACTIVE'
                  AND p.owner_principal_id = @principal_id
                  AND r.state IN ('DRAFT', 'CHANGES_REQUIRED')

                UNION ALL

                SELECT
                    'REVIEW_REQUEST'::text AS attention_kind,
                    'REVIEW_REQUESTED'::text AS attention_reason,
                    p.title,
                    r.state AS revision_state,
                    p.work_product_id,
                    r.work_product_revision_id,
                    r.revision_number,
                    rr.review_request_id,
                    rr.requested_at AS attention_since
                FROM work_products.review_requests rr
                JOIN work_products.revisions r
                  ON r.tenant_id = rr.tenant_id
                 AND r.work_product_revision_id = rr.work_product_revision_id
                JOIN work_products.work_products p
                  ON p.tenant_id = r.tenant_id
                 AND p.work_product_id = r.work_product_id
                WHERE rr.tenant_id = @tenant_id
                  AND rr.requested_principal_id = @principal_id
                  AND rr.state = 'OPEN'
                  AND r.state = 'IN_REVIEW'
                  AND p.lifecycle = 'ACTIVE'
            ) attention
            ORDER BY attention_since, work_product_id, work_product_revision_id, review_request_id NULLS FIRST;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("principal_id", principalId.Value);

        var items = new List<WorkProductAttentionItem>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var reviewRequestId = reader.IsDBNull(7)
                    ? (ReviewRequestId?)null
                    : new ReviewRequestId(reader.GetGuid(7));

                items.Add(new WorkProductAttentionItem(
                    tenantId,
                    ParseKind(reader.GetString(0)),
                    ParseReason(reader.GetString(1)),
                    reader.GetString(2),
                    ParseRevisionState(reader.GetString(3)),
                    new WorkProductSourceReference(
                        new WorkProductId(reader.GetGuid(4)),
                        new WorkProductRevisionId(reader.GetGuid(5)),
                        reader.GetInt32(6),
                        reviewRequestId),
                    reader.GetFieldValue<DateTimeOffset>(8)));
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new WorkProductAttentionView(tenantId, principalId, items);
    }

    private static WorkProductAttentionKind ParseKind(string value) => value switch
    {
        "CONTRIBUTOR_ACTION" => WorkProductAttentionKind.ContributorAction,
        "REVIEW_REQUEST" => WorkProductAttentionKind.ReviewRequest,
        _ => throw new InvalidOperationException($"Unknown attention kind '{value}'.")
    };

    private static WorkProductAttentionReason ParseReason(string value) => value switch
    {
        "DRAFT_REQUIRES_ACTION" => WorkProductAttentionReason.DraftRequiresAction,
        "CHANGES_REQUIRED" => WorkProductAttentionReason.ChangesRequired,
        "REVIEW_REQUESTED" => WorkProductAttentionReason.ReviewRequested,
        _ => throw new InvalidOperationException($"Unknown attention reason '{value}'.")
    };

    private static WorkProductRevisionState ParseRevisionState(string value) => value switch
    {
        "DRAFT" => WorkProductRevisionState.Draft,
        "CHANGES_REQUIRED" => WorkProductRevisionState.ChangesRequired,
        "IN_REVIEW" => WorkProductRevisionState.InReview,
        _ => throw new InvalidOperationException($"State '{value}' is not valid in the bounded attention view.")
    };
}
