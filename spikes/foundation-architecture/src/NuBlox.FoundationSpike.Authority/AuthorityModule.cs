using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Authority;

public sealed record AuthorityEvaluationResult(
    Guid EvaluationId,
    bool Allowed,
    string Reason);

public sealed class AuthorityModule
{
    private const string ApproveOperation = "DECISION.APPROVE";

    public async Task GrantApprovePermissionAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid principalId,
        DateTimeOffset grantedAt,
        CancellationToken cancellationToken = default)
    {
        EnsurePrincipal(principalId);

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO access.operation_permissions
                (customer_id, principal_id, operation, granted_at)
            VALUES
                (@customer_id, @principal_id, @operation, @granted_at);
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("principal_id", principalId);
        command.AddParameter("operation", ApproveOperation);
        command.AddParameter("granted_at", grantedAt.UtcDateTime);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Guid> GrantDecisionAuthorityAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid principalId,
        string decisionType,
        decimal? maximumAmount,
        DateTimeOffset effectiveFrom,
        DateTimeOffset? effectiveTo,
        string grantReason,
        Guid? delegatedBy = null,
        CancellationToken cancellationToken = default)
    {
        EnsurePrincipal(principalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionType);
        ArgumentException.ThrowIfNullOrWhiteSpace(grantReason);

        if (maximumAmount is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAmount));
        }

        if (effectiveTo is not null && effectiveTo <= effectiveFrom)
        {
            throw new ArgumentException("Authority effective-to must be after effective-from.", nameof(effectiveTo));
        }

        var id = Guid.NewGuid();

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO authority.decision_authorities
                (customer_id, id, principal_id, decision_type, maximum_amount,
                 effective_from, effective_to, delegated_by, grant_reason)
            VALUES
                (@customer_id, @id, @principal_id, @decision_type, @maximum_amount,
                 @effective_from, @effective_to, @delegated_by, @grant_reason);
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("id", id);
        command.AddParameter("principal_id", principalId);
        command.AddParameter("decision_type", decisionType.Trim().ToUpperInvariant());
        command.AddParameter("maximum_amount", maximumAmount is null ? DBNull.Value : maximumAmount.Value);
        command.AddParameter("effective_from", effectiveFrom.UtcDateTime);
        command.AddParameter("effective_to", effectiveTo is null ? DBNull.Value : effectiveTo.Value.UtcDateTime);
        command.AddParameter("delegated_by", delegatedBy is null ? DBNull.Value : delegatedBy.Value);
        command.AddParameter("grant_reason", grantReason.Trim());

        await command.ExecuteNonQueryAsync(cancellationToken);
        return id;
    }

    public async Task<AuthorityEvaluationResult> EvaluateApprovalAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid principalId,
        string decisionType,
        decimal requestedAmount,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default)
    {
        EnsurePrincipal(principalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionType);

        if (requestedAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requestedAmount));
        }

        var hasPermission = await HasApprovePermissionAsync(
            session,
            customerId,
            principalId,
            effectiveAt,
            cancellationToken);

        var hasAuthority = false;
        var reason = "OPERATION_PERMISSION_MISSING";

        if (hasPermission)
        {
            hasAuthority = await HasDecisionAuthorityAsync(
                session,
                customerId,
                principalId,
                decisionType,
                requestedAmount,
                effectiveAt,
                cancellationToken);

            reason = hasAuthority
                ? "ALLOWED"
                : "BUSINESS_AUTHORITY_MISSING_OR_THRESHOLD_EXCEEDED";
        }

        var allowed = hasPermission && hasAuthority;
        var evaluationId = Guid.NewGuid();

        await using var evidence = session.Connection.CreateCommand();
        evidence.Transaction = session.Transaction;
        evidence.CommandText = """
            INSERT INTO authority.evaluations
                (customer_id, id, principal_id, operation, decision_type,
                 requested_amount, allowed, reason)
            VALUES
                (@customer_id, @id, @principal_id, @operation, @decision_type,
                 @requested_amount, @allowed, @reason);
            """;
        evidence.AddParameter("customer_id", customerId.Value);
        evidence.AddParameter("id", evaluationId);
        evidence.AddParameter("principal_id", principalId);
        evidence.AddParameter("operation", ApproveOperation);
        evidence.AddParameter("decision_type", decisionType.Trim().ToUpperInvariant());
        evidence.AddParameter("requested_amount", requestedAmount);
        evidence.AddParameter("allowed", allowed);
        evidence.AddParameter("reason", reason);

        await evidence.ExecuteNonQueryAsync(cancellationToken);

        return new AuthorityEvaluationResult(evaluationId, allowed, reason);
    }

    private static async Task<bool> HasApprovePermissionAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid principalId,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM access.operation_permissions
                WHERE customer_id = @customer_id
                  AND principal_id = @principal_id
                  AND operation = @operation
                  AND granted_at <= @effective_at
                  AND (revoked_at IS NULL OR revoked_at > @effective_at)
            );
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("principal_id", principalId);
        command.AddParameter("operation", ApproveOperation);
        command.AddParameter("effective_at", effectiveAt.UtcDateTime);

        return await command.ExecuteScalarAsync(cancellationToken) is true;
    }

    private static async Task<bool> HasDecisionAuthorityAsync(
        ITransactionalSession session,
        CustomerId customerId,
        Guid principalId,
        string decisionType,
        decimal requestedAmount,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM authority.decision_authorities
                WHERE customer_id = @customer_id
                  AND principal_id = @principal_id
                  AND decision_type = @decision_type
                  AND effective_from <= @effective_at
                  AND (effective_to IS NULL OR effective_to > @effective_at)
                  AND (maximum_amount IS NULL OR maximum_amount >= @requested_amount)
            );
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("principal_id", principalId);
        command.AddParameter("decision_type", decisionType.Trim().ToUpperInvariant());
        command.AddParameter("effective_at", effectiveAt.UtcDateTime);
        command.AddParameter("requested_amount", requestedAmount);

        return await command.ExecuteScalarAsync(cancellationToken) is true;
    }

    private static void EnsurePrincipal(Guid principalId)
    {
        if (principalId == Guid.Empty)
        {
            throw new ArgumentException("Principal must be non-empty.", nameof(principalId));
        }
    }
}
