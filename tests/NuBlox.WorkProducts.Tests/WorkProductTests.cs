using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Kernel;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Tests;

[TestClass]
public sealed class WorkProductTests
{
    [TestMethod]
    public async Task CreateProducesActiveProductAndInitialDraftRevision()
    {
        var repository = new InMemoryRepository();
        var timestamp = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);
        var actor = PrincipalId.New();
        var tenant = TenantId.New();
        var service = new WorkProductApplicationService(repository, new FixedClock(timestamp));

        var result = await service.CreateAsync(new CreateWorkProductCommand(
            tenant,
            actor,
            "  Configuration Plan  ",
            "  governed-document  "));

        Assert.AreEqual(tenant, result.WorkProduct.TenantId);
        Assert.AreEqual("Configuration Plan", result.WorkProduct.Title);
        Assert.AreEqual("governed-document", result.WorkProduct.ProductType);
        Assert.AreEqual(actor, result.WorkProduct.OwnerPrincipalId);
        Assert.AreEqual(WorkProductLifecycle.Active, result.WorkProduct.Lifecycle);
        Assert.AreEqual(1, result.WorkProduct.CurrentRevisionNumber);
        Assert.AreEqual(1, result.CurrentRevision.RevisionNumber);
        Assert.AreEqual(WorkProductRevisionState.Draft, result.CurrentRevision.State);
        Assert.AreEqual(result.WorkProduct.Id, result.CurrentRevision.WorkProductId);
        Assert.AreEqual(timestamp, result.CurrentRevision.CreatedAtUtc);
        Assert.AreSame(result, repository.Stored);
    }

    [TestMethod]
    public async Task CreateRejectsEmptyGovernedMetadataBeforePersistence()
    {
        var repository = new InMemoryRepository();
        var service = new WorkProductApplicationService(
            repository,
            new FixedClock(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateWorkProductCommand(
            TenantId.New(),
            PrincipalId.New(),
            " ",
            "governed-document")));

        Assert.IsNull(repository.Stored);
    }

    [TestMethod]
    public async Task FindUsesBothTenantAndStableWorkProductIdentity()
    {
        var tenant = TenantId.New();
        var actor = PrincipalId.New();
        var repository = new InMemoryRepository();
        var service = new WorkProductApplicationService(repository, new FixedClock(DateTimeOffset.UtcNow));
        var created = await service.CreateAsync(new CreateWorkProductCommand(
            tenant,
            actor,
            "Design Basis",
            "governed-document"));

        var found = await service.FindAsync(tenant, created.WorkProduct.Id);
        var wrongTenant = await service.FindAsync(TenantId.New(), created.WorkProduct.Id);

        Assert.IsNotNull(found);
        Assert.AreEqual(created.WorkProduct.Id, found.WorkProduct.Id);
        Assert.IsNull(wrongTenant);
    }

    private sealed class FixedClock(DateTimeOffset timestamp) : ISystemClock
    {
        public DateTimeOffset UtcNow => timestamp;
    }

    private sealed class InMemoryRepository : IWorkProductRepository
    {
        public WorkProductRecord? Stored { get; private set; }

        public Task AddAsync(WorkProductRecord record, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Stored = record;
            return Task.CompletedTask;
        }

        public Task<WorkProductRecord?> FindAsync(
            TenantId tenantId,
            WorkProductId workProductId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                Stored is not null
                && Stored.WorkProduct.TenantId == tenantId
                && Stored.WorkProduct.Id == workProductId
                    ? Stored
                    : null);
        }
    }
}
