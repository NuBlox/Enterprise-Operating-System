using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Kernel.Identity;

namespace NuBlox.Kernel.Tests;

[TestClass]
public sealed class TenantContextResolverTests
{
    [TestMethod]
    public async Task AuthorisedTenantResolvesToStableInternalContext()
    {
        var principalId = PrincipalId.New();
        var tenantId = TenantId.New();
        var directory = new StubTenantDirectory(("baesystems", tenantId));
        var authorizer = new StubParticipationAuthorizer((principalId, tenantId));
        var resolver = new TenantContextResolver(directory, authorizer);

        var result = await resolver.ResolveAsync(CreatePrincipal(principalId), " baesystems ");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Context);
        Assert.AreEqual(tenantId, result.Context.TenantId);
        Assert.AreEqual("baesystems", result.Context.TenantSlug);
    }

    [TestMethod]
    public async Task RequestedSlugCannotOverridePrincipalTenantParticipation()
    {
        var principalId = PrincipalId.New();
        var allowedTenant = TenantId.New();
        var forbiddenTenant = TenantId.New();
        var directory = new StubTenantDirectory(
            ("allowed", allowedTenant),
            ("forbidden", forbiddenTenant));
        var authorizer = new StubParticipationAuthorizer((principalId, allowedTenant));
        var resolver = new TenantContextResolver(directory, authorizer);

        var result = await resolver.ResolveAsync(CreatePrincipal(principalId), "forbidden");

        Assert.AreEqual(TenantContextResolutionStatus.AccessDenied, result.Status);
        Assert.IsNull(result.Context);
    }

    [TestMethod]
    public async Task MissingTenantContextFailsClosed()
    {
        var principalId = PrincipalId.New();
        var resolver = new TenantContextResolver(
            new StubTenantDirectory(),
            new StubParticipationAuthorizer());

        var result = await resolver.ResolveAsync(CreatePrincipal(principalId), "   ");

        Assert.AreEqual(TenantContextResolutionStatus.MissingTenant, result.Status);
        Assert.IsNull(result.Context);
    }

    [TestMethod]
    public async Task UnknownTenantFailsClosed()
    {
        var principalId = PrincipalId.New();
        var resolver = new TenantContextResolver(
            new StubTenantDirectory(),
            new StubParticipationAuthorizer());

        var result = await resolver.ResolveAsync(CreatePrincipal(principalId), "unknown");

        Assert.AreEqual(TenantContextResolutionStatus.TenantNotFound, result.Status);
        Assert.IsNull(result.Context);
    }

    [TestMethod]
    public async Task DisabledPrincipalCannotEstablishTenantContext()
    {
        var principalId = PrincipalId.New();
        var tenantId = TenantId.New();
        var directory = new StubTenantDirectory(("tenant", tenantId));
        var authorizer = new StubParticipationAuthorizer((principalId, tenantId));
        var resolver = new TenantContextResolver(directory, authorizer);
        var principal = CreatePrincipal(principalId, isEnabled: false);

        var result = await resolver.ResolveAsync(principal, "tenant");

        Assert.AreEqual(TenantContextResolutionStatus.PrincipalDisabled, result.Status);
        Assert.IsNull(result.Context);
        Assert.AreEqual(0, directory.ResolveCount);
        Assert.AreEqual(0, authorizer.CheckCount);
    }

    [TestMethod]
    public async Task MultiTenantPrincipalMayEnterOnlyAuthorisedResolvedTenants()
    {
        var principalId = PrincipalId.New();
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();
        var directory = new StubTenantDirectory(("alpha", tenantA), ("beta", tenantB));
        var authorizer = new StubParticipationAuthorizer(
            (principalId, tenantA),
            (principalId, tenantB));
        var resolver = new TenantContextResolver(directory, authorizer);

        var alpha = await resolver.ResolveAsync(CreatePrincipal(principalId), "alpha");
        var beta = await resolver.ResolveAsync(CreatePrincipal(principalId), "beta");

        Assert.AreEqual(tenantA, alpha.Context?.TenantId);
        Assert.AreEqual(tenantB, beta.Context?.TenantId);
    }

    [TestMethod]
    public void ServicePrincipalRemainsAServiceIdentity()
    {
        var principal = new AuthenticatedPrincipal(
            PrincipalId.New(),
            PrincipalKind.Service,
            new ExternalIdentityBinding("https://issuer.example", "workload-42"));

        Assert.AreEqual(PrincipalKind.Service, principal.Kind);
        Assert.AreEqual("https://issuer.example", principal.ExternalIdentity.Issuer);
        Assert.AreEqual("workload-42", principal.ExternalIdentity.Subject);
    }

    private static AuthenticatedPrincipal CreatePrincipal(PrincipalId principalId, bool isEnabled = true) =>
        new(
            principalId,
            PrincipalKind.Human,
            new ExternalIdentityBinding("https://issuer.example", "subject-123"),
            isEnabled);

    private sealed class StubTenantDirectory : ITenantDirectory
    {
        private readonly Dictionary<string, TenantId> _tenants;

        public StubTenantDirectory(params (string Slug, TenantId TenantId)[] tenants)
        {
            _tenants = tenants.ToDictionary(x => x.Slug, x => x.TenantId, StringComparer.OrdinalIgnoreCase);
        }

        public int ResolveCount { get; private set; }

        public ValueTask<TenantId?> ResolveTenantIdAsync(
            string tenantSlug,
            CancellationToken cancellationToken = default)
        {
            ResolveCount++;
            return ValueTask.FromResult<TenantId?>(
                _tenants.TryGetValue(tenantSlug, out var tenantId) ? tenantId : null);
        }
    }

    private sealed class StubParticipationAuthorizer : ITenantParticipationAuthorizer
    {
        private readonly HashSet<(PrincipalId PrincipalId, TenantId TenantId)> _allowed;

        public StubParticipationAuthorizer(params (PrincipalId PrincipalId, TenantId TenantId)[] allowed)
        {
            _allowed = allowed.ToHashSet();
        }

        public int CheckCount { get; private set; }

        public ValueTask<bool> CanAccessTenantAsync(
            PrincipalId principalId,
            TenantId tenantId,
            CancellationToken cancellationToken = default)
        {
            CheckCount++;
            return ValueTask.FromResult(_allowed.Contains((principalId, tenantId)));
        }
    }
}
