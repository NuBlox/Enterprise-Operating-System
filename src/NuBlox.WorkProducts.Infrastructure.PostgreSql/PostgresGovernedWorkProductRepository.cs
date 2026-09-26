using Npgsql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Infrastructure.PostgreSql;

/// <summary>
/// Extends the base Work Product repository with governed review-decision persistence.
/// </summary>
public sealed class PostgresGovernedWorkProductRepository : IWorkProductRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly PostgresWorkProductRepository _inner;

    public PostgresGovernedWorkProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _inner = new PostgresWorkProductRepository(dataSource);
    }

    public Task AddAsync(WorkProductRecord record, CancellationToken cancellationToken = default) =>
        _inner.AddAsync(record, cancellationToken);

    public Task<WorkProductRecord?> FindAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default) =>
        _inner.FindAsync(tenantId, workProductId, cancellationToken);

    public Task SubmitForReviewAsync(
        ReviewSubmission submission,
        CancellationToken cancellationToken = default) =>
        _inner.SubmitForReviewAsync(submission, cancellationToken);

    public async Task<ReviewDecisionContext?> FindReviewDecisionContextAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        ReviewRequestId reviewRequestId,
        CancellationToken cancellationToken = default)
    {
        var record = await _inner.FindAsync(tenantId, workProductId, cancellationToken).ConfigureAwait(false);
        if (record is null)
        {
            return null;
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(
            """
            SELECT kind, requested_principal_id, state
            FROM work_products.review_requests
            WHERE tenant_id = @tenant_id
              AND review_request_id = @review_request_id
              AND work_product_revision_id = @revision_id;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("review_request_id", reviewRequestId.Value);
        command.Parameters.AddWithValue("revision_id", record.CurrentRevision.Id.Value);

        ReviewDecisionContext? result = null;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result = new ReviewDecisionContext(
                record,
                reviewRequestId,
                ParseKind(reader.GetString(0)),
                new PrincipalId(reader.GetGuid(1)),
                ParseState(reader.GetString(2)));
        }

        await reader.CloseAsync().ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task RecordDecisionAsync(
        ReviewDecisionResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Revision.TenantId != result.Decision.TenantId
            || result.Revision.Id != result.Decision.WorkProductRevisionId)
        {
            throw new InvalidOperationException("Decision evidence and revision identities must match.");
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, result.Decision.TenantId.Value, cancellationToken).ConfigureAwait(false);

        await using (var revisionUpdate = new NpgsqlCommand(
            """
            UPDATE work_products.revisions
               SET state = @state
             WHERE tenant_id = @tenant_id
               AND work_product_revision_id = @revision_id
               AND state = 'IN_REVIEW';
            """,
            connection,
            transaction))
        {
            revisionUpdate.Parameters.AddWithValue("state", FormatOutcomeState(result.Decision.Outcome));
            revisionUpdate.Parameters.AddWithValue("tenant_id", result.Decision.TenantId.Value);
            revisionUpdate.Parameters.AddWithValue("revision_id", result.Decision.WorkProductRevisionId.Value);
            if (await revisionUpdate.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new WorkProductStateConflictException("The revision is no longer InReview and cannot receive a decision.");
            }
        }

        await using (var requestUpdate = new NpgsqlCommand(
            """
            UPDATE work_products.review_requests
               SET state = 'COMPLETED'
             WHERE tenant_id = @tenant_id
               AND review_request_id = @review_request_id
               AND work_product_revision_id = @revision_id
               AND state = 'OPEN';
            """,
            connection,
            transaction))
        {
            requestUpdate.Parameters.AddWithValue("tenant_id", result.Decision.TenantId.Value);
            requestUpdate.Parameters.AddWithValue("review_request_id", result.Decision.ReviewRequestId.Value);
            requestUpdate.Parameters.AddWithValue("revision_id", result.Decision.WorkProductRevisionId.Value);
            if (await requestUpdate.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new WorkProductStateConflictException("The review request is no longer open for a decision.");
            }
        }

        await using (var insert = new NpgsqlCommand(
            """
            INSERT INTO work_products.review_decisions (
                tenant_id, decision_evidence_id, review_request_id, work_product_revision_id,
                actor_principal_id, outcome, rationale, authority_reference, decided_at, correlation_id)
            VALUES (
                @tenant_id, @decision_id, @review_request_id, @revision_id,
                @actor_principal_id, @outcome, @rationale, @authority_reference, @decided_at, @correlation_id);
            """,
            connection,
            transaction))
        {
            insert.Parameters.AddWithValue("tenant_id", result.Decision.TenantId.Value);
            insert.Parameters.AddWithValue("decision_id", result.Decision.Id.Value);
            insert.Parameters.AddWithValue("review_request_id", result.Decision.ReviewRequestId.Value);
            insert.Parameters.AddWithValue("revision_id", result.Decision.WorkProductRevisionId.Value);
            insert.Parameters.AddWithValue("actor_principal_id", result.Decision.ActorPrincipalId.Value);
            insert.Parameters.AddWithValue("outcome", FormatOutcome(result.Decision.Outcome));
            insert.Parameters.AddWithValue("rationale", result.Decision.Rationale);
            insert.Parameters.AddWithValue("authority_reference", result.Decision.AuthorityReference);
            insert.Parameters.AddWithValue("decided_at", result.Decision.DecidedAtUtc);
            insert.Parameters.AddWithValue("correlation_id", result.Decision.CorrelationId is { } correlationId ? correlationId : DBNull.Value);
            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static ReviewRequestKind ParseKind(string value) => value switch
    {
        "REVIEW" => ReviewRequestKind.Review,
        "APPROVAL" => ReviewRequestKind.Approval,
        _ => throw new InvalidOperationException($"Unknown review request kind '{value}'.")
    };

    private static ReviewRequestState ParseState(string value) => value switch
    {
        "OPEN" => ReviewRequestState.Open,
        "COMPLETED" => ReviewRequestState.Completed,
        "CANCELLED" => ReviewRequestState.Cancelled,
        _ => throw new InvalidOperationException($"Unknown review request state '{value}'.")
    };

    private static string FormatOutcome(ReviewDecisionOutcome outcome) => outcome switch
    {
        ReviewDecisionOutcome.Approved => "APPROVED",
        ReviewDecisionOutcome.Rejected => "REJECTED",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome))
    };

    private static string FormatOutcomeState(ReviewDecisionOutcome outcome) => outcome switch
    {
        ReviewDecisionOutcome.Approved => "APPROVED",
        ReviewDecisionOutcome.Rejected => "REJECTED",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome))
    };
}
