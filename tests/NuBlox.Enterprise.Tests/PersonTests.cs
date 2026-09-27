using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Enterprise.Application;
using NuBlox.Kernel;

namespace NuBlox.Enterprise.Tests;

[TestClass]
public sealed class PersonTests
{
    [TestMethod]
    public void PersonCreationCanonicalisesNameAndPreservesPartyKind()
    {
        var tenantId = TenantId.New();
        var administratorPrincipalId = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 27, 10, 0, 0, TimeSpan.Zero);

        var person = Person.Create(tenantId, "  Alex Morgan  ", administratorPrincipalId, createdAt);

        Assert.AreNotEqual(Guid.Empty, person.Id.Value);
        Assert.AreEqual(PartyKind.Person, person.Kind);
        Assert.AreEqual("Alex Morgan", person.DisplayName);
        Assert.AreEqual(tenantId, person.TenantId);
        Assert.AreEqual(administratorPrincipalId, person.CreatedByPrincipalId);
        Assert.AreEqual(createdAt, person.CreatedAtUtc);
        Assert.AreNotEqual(administratorPrincipalId.Value, person.Id.Value);
    }

    [TestMethod]
    public async Task ApplicationDeniesCreationWithoutConfiguredAccess()
    {
        var service = new PersonApplicationService(
            new InMemoryPersonRepository(),
            new FixedPersonAccessEvaluator(canCreate: false, canRead: false),
            new FixedEnterpriseClock());

        await Assert.ThrowsExactlyAsync<PersonAccessDeniedException>(() =>
            service.CreateAsync(new CreatePersonCommand(
                TenantId.New(),
                PrincipalId.New(),
                "Alex Morgan")));
    }

    [TestMethod]
    public async Task PersonExistsIndependentlyOfActingPrincipal()
    {
        var repository = new InMemoryPersonRepository();
        var administrator = PrincipalId.New();
        var tenant = TenantId.New();
        var service = new PersonApplicationService(
            repository,
            new FixedPersonAccessEvaluator(canCreate: true, canRead: true),
            new FixedEnterpriseClock());

        var created = await service.CreateAsync(new CreatePersonCommand(
            tenant,
            administrator,
            "Alex Morgan"));
        var found = await service.FindAsync(tenant, created.Person.Id, administrator);

        Assert.IsNotNull(found);
        Assert.AreEqual(created.Person, found.Person);
        Assert.AreNotEqual(administrator.Value, found.Person.Id.Value);
    }

    private sealed class InMemoryPersonRepository : IPersonRepository
    {
        private readonly Dictionary<(TenantId TenantId, PartyId PartyId), Person> _people = [];

        public Task AddAsync(Person person, CancellationToken cancellationToken = default)
        {
            _people.Add((person.TenantId, person.Id), person);
            return Task.CompletedTask;
        }

        public Task<Person?> FindAsync(
            TenantId tenantId,
            PartyId partyId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_people.GetValueOrDefault((tenantId, partyId)));
    }

    private sealed class FixedPersonAccessEvaluator(bool canCreate, bool canRead) : IPersonAccessEvaluator
    {
        public ValueTask<bool> CanCreateAsync(
            TenantId tenantId,
            PrincipalId actorPrincipalId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(canCreate);

        public ValueTask<bool> CanReadAsync(
            Person person,
            PrincipalId actorPrincipalId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(canRead);
    }

    private sealed class FixedEnterpriseClock : IEnterpriseClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 9, 27, 10, 0, 0, TimeSpan.Zero);
    }
}
