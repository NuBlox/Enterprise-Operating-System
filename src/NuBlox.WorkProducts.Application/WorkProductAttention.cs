using NuBlox.Kernel;

namespace NuBlox.WorkProducts.Application;

public enum WorkProductAttentionKind
{
    ContributorAction = 1,
    ReviewRequest = 2
}

public enum WorkProductAttentionReason
{
    DraftRequiresAction = 1,
    ChangesRequired = 2,
    ReviewRequested = 3
}

/// <summary>
/// Typed drill-through reference back to the governed Work Product source context.
/// It deliberately carries internal identities rather than an untrusted caller-built URL.
/// </summary>
public sealed record WorkProductSourceReference(
    WorkProductId WorkProductId,
    WorkProductRevisionId WorkProductRevisionId,
    int RevisionNumber,
    ReviewRequestId? ReviewRequestId = null);

public sealed record WorkProductAttentionItem(
    TenantId TenantId,
    WorkProductAttentionKind Kind,
    WorkProductAttentionReason Reason,
    string Title,
    WorkProductRevisionState RevisionState,
    WorkProductSourceReference Source,
    DateTimeOffset AttentionSinceUtc);

public sealed record WorkProductAttentionView(
    TenantId TenantId,
    PrincipalId PrincipalId,
    IReadOnlyList<WorkProductAttentionItem> Items)
{
    public int Count => Items.Count;
}

public interface IWorkProductAttentionQuery
{
    Task<WorkProductAttentionView> QueryAsync(
        TenantId tenantId,
        PrincipalId principalId,
        CancellationToken cancellationToken = default);
}

public sealed class WorkProductAttentionService
{
    private readonly IWorkProductAttentionQuery _query;

    public WorkProductAttentionService(IWorkProductAttentionQuery query)
    {
        _query = query ?? throw new ArgumentNullException(nameof(query));
    }

    public Task<WorkProductAttentionView> GetMyAttentionAsync(
        TenantId tenantId,
        PrincipalId principalId,
        CancellationToken cancellationToken = default) =>
        _query.QueryAsync(tenantId, principalId, cancellationToken);
}
