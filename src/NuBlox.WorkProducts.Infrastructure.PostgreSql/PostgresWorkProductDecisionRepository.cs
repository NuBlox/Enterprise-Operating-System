using Npgsql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Infrastructure.PostgreSql;

public sealed class PostgresWorkProductDecisionRepository : IWorkProductDecisionRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresWorkProductDecisionRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task<ReviewRequestAssignment?> FindOpenReviewRequestAsync(
        TenantId tenantId,
        ReviewRequestId reviewRequestId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(
            """
            SELECT work_product_revision_id, requested_principal_id
              FROM work_products.review_requests
             WHERE tenant_id = @tenant_id
               AND review_request_id = @request_id
               AND state = 'OPEN';
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("request_id", reviewRequestId.Value);

        ReviewRequestAssignment? result = null;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                result = new ReviewRequestAssignment(
                    reviewRequestId,
                    tenantId,
                    new WorkProductRevisionId(reader.GetGuid(0)),
                    new PrincipalId(reader.GetGuid(1)));
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task ApplyReviewDecisionAsync(
        ReviewDecisionResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Decision.TenantId != result.Revision.TenantId
            || result.Decision.ReviewRequestId != result.Assignment.ReviewRequestId
            || result.Decision.WorkProductRevisionId != result.Revision.Id)
        {
            throw new InvalidOperationException("Decision, assignment and revision identities must match.");
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, result.Decision.TenantId.Value, cancellationToken).ConfigureAwait(false);

        await using (var updateRevision = new NpgsqlCommand(
            """
            UPDATE work_products.revisions
               SET state = @target_state
             WHERE tenant_id = @tenant_id
               AND work_product_revision_id = @revision_id
               AND state = 'IN_REVIEW';
            """,
            connection,
            transaction))
        {
            updateRevision.Parameters.AddWithValue("target_state", FormatRevisionState(result.Revision.State));
            updateRevision.Parameters.AddWithValue("tenant_id", result.Decision.TenantId.Value);
            updateRevision.Parameters.AddWithValue("revision_id", result.Revision.Id.Value);
            if (await updateRevision.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new WorkProductStateConflictException("The revision is no longer InReview and cannot accept this decision.");
            }
        }

        await using (var completeRequest = new NpgsqlCommand(
            """
            UPDATE work_products.review_requests
               SET state = 'COMPLETED'
             WHERE tenant_id = @tenant_id
               AND review_request_id = @request_id
               AND state = 'OPEN';
            """,
            connection,
            transaction))
        {
            completeRequest.Parameters.AddWithValue("tenant_id", result.Decision.TenantId.Value);
            completeRequest.Parameters.AddWithValue("request_id", result.Decision.ReviewRequestId.Value);
            if (await completeRequest.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new WorkProductStateConflictException("The Review Request is no longer open.");
            }
        }

        await using (var insertDecision = new NpgsqlCommand(
            """
            INSERT INTO work_products.review_decisions (
                tenant_id, review_decision_id, review_request_id, work_product_revision_id,
                outcome, decided_by_principal_id, decided_at, rationale)
            VALUES (
                @tenant_id, @decision_id, @request_id, @revision_id,
                @outcome, @decided_by, @decided_at, @rationale);
            """,
            connection,
            transaction))
        {
            insertDecision.Parameters.AddWithValue("tenant_id", result.Decision.TenantId.Value);
            insertDecision.Parameters.AddWithValue("decision_id", result.Decision.Id.Value);
            insertDecision.Parameters.AddWithValue("request_id", result.Decision.ReviewRequestId.Value);
            insertDecision.Parameters.AddWithValue("revision_id", result.Decision.WorkProductRevisionId.Value);
            insertDecision.Parameters.AddWithValue("outcome", FormatOutcome(result.Decision.Outcome));
            insertDecision.Parameters.AddWithValue("decided_by", result.Decision.DecidedByPrincipalId.Value);
            insertDecision.Parameters.AddWithValue("decided_at", result.Decision.DecidedAtUtc);
            insertDecision.Parameters.AddWithValue("rationale", (object?)result.Decision.Rationale ?? DBNull.Value);
            await insertDecision.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string FormatRevisionState(WorkProductRevisionState state) => state switch
    {
        WorkProductRevisionState.ChangesRequired => "CHANGES_REQUIRED",
        WorkProductRevisionState.Rejected => "REJECTED",
        WorkProductRevisionState.Approved => "APPROVED",
        _ => throw new ArgumentOutOfRangeException(nameof(state), "Review decisions may only produce ChangesRequired, Rejected or Approved states.")
    };

    private static string FormatOutcome(ReviewDecisionOutcome outcome) => outcome switch
    {
        ReviewDecisionOutcome.ChangesRequired => "CHANGES_REQUIRED",
        ReviewDecisionOutcome.Rejected => "REJECTED",
        ReviewDecisionOutcome.Approved => "APPROVED",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome))
    };
}
