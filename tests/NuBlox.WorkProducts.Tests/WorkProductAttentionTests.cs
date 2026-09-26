using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Kernel;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Tests;

[TestClass]
public sealed class WorkProductAttentionTests
{
    [TestMethod]
    public async Task ServiceReturnsTypedSourceReferencesFromGovernedQuery()
    {
        var tenant = TenantId.New();
        var principal = PrincipalId.New();
        var source = new WorkProductSourceReference(
            WorkProductId.New(),
            WorkProductRevisionId.New(),
            3,
            ReviewRequestId.New());
        var item = new WorkProductAttentionItem(
            tenant,
            WorkProductAttentionKind.ReviewRequest,
            WorkProductAttentionReason.ReviewRequested,
            "Design Basis",
            WorkProductRevisionState.InReview,
            source,
            new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero));
        var query = new FixedAttentionQuery(new WorkProductAttentionView(tenant, principal, [item]));
        var service = new WorkProductAttentionService(query);

        var result = await service.GetMyAttentionAsync(tenant, principal);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(tenant, result.TenantId);
        Assert.AreEqual(principal, result.PrincipalId);
        Assert.AreEqual(source.WorkProductId, result.Items[0].Source.WorkProductId);
        Assert.AreEqual(source.WorkProductRevisionId, result.Items[0].Source.WorkProductRevisionId);
        Assert.AreEqual(source.ReviewRequestId, result.Items[0].Source.ReviewRequestId);
        Assert.AreEqual(3, result.Items[0].Source.RevisionNumber);
    }

    private sealed class FixedAttentionQuery(WorkProductAttentionView view) : IWorkProductAttentionQuery
    {
        public Task<WorkProductAttentionView> QueryAsync(
            TenantId tenantId,
            PrincipalId principalId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.AreEqual(view.TenantId, tenantId);
            Assert.AreEqual(view.PrincipalId, principalId);
            return Task.FromResult(view);
        }
    }
}
