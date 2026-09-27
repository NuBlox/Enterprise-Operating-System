using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Enterprise.Application;
using NuBlox.Kernel;

namespace NuBlox.Enterprise.Tests;

[TestClass]
public sealed class OrganisationTests
{
    [TestMethod]
    public void OrganisationCreationCanonicalisesNameAndPreservesPartyKind()
    {
        var tenantId = TenantId.New();
        var principalId = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 27, 9, 0, 0, TimeSpan.Zero);

        var organisation = Organisation.Create(tenantId, "  Acme Design Ltd  ", principalId, createdAt);

        Assert.AreNotEqual(Guid.Empty, organisation.Id.Value);
        Assert.AreEqual(PartyKind.Organisation, organisation.Kind);
        Assert.AreEqual("Acme Design Ltd", organisation.DisplayName);
        Assert.AreEqual(tenantId, organisation.TenantId);
        Assert.AreEqual(principalId, organisation.CreatedByPrincipalId);
        Assert.AreEqual(createdAt, organisation.CreatedAtUtc);
    }

    [TestMethod]
    public async Task ApplicationDeniesCreationWithoutConfiguredAccess()
    {
        var service = new OrganisationApplicationService(
            new InMemoryOrganisationRepository(),
            new FixedOrganisationAccessEvaluator(canCreate: false, canRead: false),
            new FixedEnterpriseClock());

        await Assert.ThrowsExactlyAsync<OrganisationAccessDeniedException>(() =>
            service.CreateAsync(new CreateOrganisationCommand(
                TenantId.New(),
                PrincipalId.New(),
                "Acme Design Ltd")));
    }

    [TestMethod]
    public async Task ApplicationCreatesAndReadsGovernedOrganisation()
    {
        var repository = new InMemoryOrganisationRepository();
        var actor = PrincipalId.New();
        var tenant = TenantId.New();
        var service = new OrganisationApplicationService(
            repository,
            new FixedOrganisationAccessEvaluator(canCreate: true, canRead: true),
            new FixedEnterpriseClock());

        var created = await service.CreateAsync(new CreateOrganisationCommand(
            tenant,
            actor,
            "Acme Design Ltd"));
        var found = await service.FindAsync(tenant, created.Organisation.Id, actor);

        Assert.IsNotNull(found);
        Assert.AreEqual(created.Organisation, found.Organisation);
    }

    private sealed class InMemoryOrganisationRepository : IOrganisationRepository
    {
        private readonly Dictionary<(TenantId TenantId, PartyId PartyId), Organisation> _organisations = [];

        public Task AddAsync(Organisation organisation, CancellationToken cancellationToken = default)
        {
            _organisations.Add((organisation.TenantId, organisation.Id), organisation);
            return Task.CompletedTask;
        }

        public Task<Organisation?> FindAsync(
            TenantId tenantId,
            PartyId partyId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_organisations.GetValueOrDefault((tenantId, partyId)));
    }

    private sealed class FixedOrganisationAccessEvaluator(bool canCreate, bool canRead) : IOrganisationAccessEvaluator
    {
        public ValueTask<bool> CanCreateAsync(
            TenantId tenantId,
            PrincipalId actorPrincipalId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(canCreate);

        public ValueTask<bool> CanReadAsync(
            Organisation organisation,
            PrincipalId actorPrincipalId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(canRead);
    }

    private sealed class FixedEnterpriseClock : IEnterpriseClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 9, 27, 9, 0, 0, TimeSpan.Zero);
    }
}
