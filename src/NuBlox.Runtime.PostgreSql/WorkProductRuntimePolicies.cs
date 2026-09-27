using NuBlox.Authority;
using NuBlox.Kernel;
using NuBlox.WorkProducts.Application;

namespace NuBlox.Runtime.PostgreSql;

public sealed class WorkProductOwnershipAccessEvaluator : IWorkProductAccessEvaluator, IWorkProductIssueAccessEvaluator
{
    public ValueTask<bool> CanSubmitForReviewAsync(
        WorkProductRecord record,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        return ValueTask.FromResult(record.WorkProduct.OwnerPrincipalId == actorPrincipalId);
    }

    public ValueTask<bool> CanDecideReviewAsync(
        ReviewDecisionContext context,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return ValueTask.FromResult(context.RequestedPrincipalId == actorPrincipalId);
    }

    public ValueTask<bool> CanIssueAsync(
        WorkProductIssueContext context,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return ValueTask.FromResult(context.Record.WorkProduct.OwnerPrincipalId == actorPrincipalId);
    }
}

public sealed class ConfiguredWorkProductAuthorityEvaluator : IBusinessAuthorityEvaluator
{
    private readonly HashSet<PrincipalId> _approvalPrincipals;

    public ConfiguredWorkProductAuthorityEvaluator(IEnumerable<PrincipalId> approvalPrincipals)
    {
        ArgumentNullException.ThrowIfNull(approvalPrincipals);
        _approvalPrincipals = [.. approvalPrincipals];
    }

    public ValueTask<AuthorityEvaluationResult> EvaluateAsync(
        AuthorityEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!_approvalPrincipals.Contains(context.PrincipalId))
        {
            return ValueTask.FromResult(AuthorityEvaluationResult.Denied("work-product-approval-not-configured"));
        }

        return ValueTask.FromResult(AuthorityEvaluationResult.Granted(
            $"runtime-config:{context.PrincipalId}:{context.Requirement.ActionCode}"));
    }
}
