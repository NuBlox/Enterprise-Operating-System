using NuBlox.Kernel;

namespace NuBlox.WorkProducts.Application;

public enum WorkProductAttentionKind
{
    ContributorAction = 1,
    ReviewAction = 2
}

public sealed record WorkProductSourceReference(
    WorkProductId WorkProductId,
    WorkProductRevisionId WorkProductRevisionId,
    int RevisionNumber,
    ReviewRequestId? ReviewRequestId = null);

public sealed record WorkProductAttentionItem(
    TenantId TenantId,
    WorkProductAttentionKind Kind,
    string Title,
    string ProductType,
    WorkProductRevisionState RevisionState,
    DateTimeOffset AttentionSinceUtc,
    WorkProductSourceReference Source);

public sealed record WorkProductReviewEvidence(
    ReviewRequestId ReviewRequestId,
    ReviewRequestKind Kind,
    PrincipalId RequestedPrincipalId,
    DateTimeOffset RequestedAtUtc,
    ReviewRequestState State,
    DecisionEvidenceId? DecisionEvidenceId,
    ReviewDecisionOutcome? Outcome,
    PrincipalId? DecisionActorPrincipalId,
    DateTimeOffset? DecidedAtUtc);

public sealed record WorkProductIssueEvidenceSummary(
    IssueEvidenceId IssueEvidenceId,
    DecisionEvidenceId ApprovalDecisionEvidenceId,
    PrincipalId IssuedByPrincipalId,
    DateTimeOffset IssuedAtUtc);

public sealed record WorkProductSourceView(
    WorkProductRecord Record,
    IReadOnlyList<WorkProductReviewEvidence> ReviewEvidence,
    WorkProductIssueEvidenceSummary? IssueEvidence);

public interface IWorkProductAttentionReader
{
    Task<IReadOnlyList<WorkProductAttentionItem>> ListAsync(
        TenantId tenantId,
        PrincipalId principalId,
        CancellationToken cancellationToken = default);

    Task<WorkProductSourceView?> FindSourceAsync(
        TenantId tenantId,
        PrincipalId principalId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default);
}

public sealed class WorkProductAttentionService
{
    private readonly IWorkProductAttentionReader _reader;

    public WorkProductAttentionService(IWorkProductAttentionReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public Task<IReadOnlyList<WorkProductAttentionItem>> ListAsync(
        TenantId tenantId,
        PrincipalId principalId,
        CancellationToken cancellationToken = default) =>
        _reader.ListAsync(tenantId, principalId, cancellationToken);

    public Task<WorkProductSourceView?> FindSourceAsync(
        TenantId tenantId,
        PrincipalId principalId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default) =>
        _reader.FindSourceAsync(tenantId, principalId, workProductId, cancellationToken);
}
