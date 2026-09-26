using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Identity;
using NuBlox.Kernel;

namespace NuBlox.Identity.Tests;

[TestClass]
public sealed class IdentityContextResolverTests
{
    [TestMethod]
    public async Task MissingAuthenticationIsRejected()
    {
        var resolver = CreateResolver();

        await CaptureExceptionAsync<AuthenticationRequiredException>(
            async () => await resolver.ResolveAsync(null, "tenant-a"));
    }

    [TestMethod]
    public async Task DisabledPrincipalIsRejected()
    {
        var principal = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "subject-1"),
            isEnabled: false);
        var resolver = CreateResolver();

        await CaptureExceptionAsync<PrincipalDisabledException>(
            async () => await resolver.ResolveAsync(principal, "tenant-a"));
    }

    [TestMethod]
    public async Task UnknownAndUnauthorisedTenantsUseDeniedBoundary()
    {
        var principal = CreateHumanPrincipal();
        var tenant = CreateTenant("tenant-a");
        var resolver = CreateResolver([tenant]);

        await CaptureExceptionAsync<TenantContextDeniedException>(
            async () => await resolver.ResolveAsync(principal, "unknown-tenant"));

        await CaptureExceptionAsync<TenantContextDeniedException>(
            async () => await resolver.ResolveAsync(principal, "tenant-a"));
    }

    [TestMethod]
    public async Task MultiTenantPrincipalCanSelectAuthorisedWorkingTenant()
    {
        var principal = CreateHumanPrincipal();
        var tenantA = CreateTenant("tenant-a");
        var tenantB = CreateTenant("tenant-b");
        var resolver = CreateResolver(
            [tenantA, tenantB],
            [(principal.PrincipalId, tenantA.TenantId), (principal.PrincipalId, tenantB.TenantId)]);

        var contextA = await resolver.ResolveAsync(principal, "TENANT-A");
        var contextB = await resolver.ResolveAsync(principal, "tenant-b");

        Assert.AreEqual(principal.PrincipalId, contextA.Principal.PrincipalId);
        Assert.AreEqual(tenantA.TenantId, contextA.Tenant.TenantId);
        Assert.AreEqual("tenant-a", contextA.Tenant.CanonicalSlug.Value);
        Assert.AreEqual(tenantB.TenantId, contextB.Tenant.TenantId);
    }

    [TestMethod]
    public async Task UntrustedSlugCannotOverrideVerifiedTenantAccess()
    {
        var principal = CreateHumanPrincipal();
        var tenantA = CreateTenant("tenant-a");
        var tenantB = CreateTenant("tenant-b");
        var resolver = CreateResolver(
            [tenantA, tenantB],
            [(principal.PrincipalId, tenantA.TenantId)]);

        var allowed = await resolver.ResolveAsync(principal, "tenant-a");
        Assert.AreEqual(tenantA.TenantId, allowed.Tenant.TenantId);

        await CaptureExceptionAsync<TenantContextDeniedException>(
            async () => await resolver.ResolveAsync(principal, "tenant-b"));

        await CaptureExceptionAsync<TenantContextDeniedException>(
            async () => await resolver.ResolveAsync(principal, "tenant-a/../tenant-b"));
    }

    [TestMethod]
    public async Task ServicePrincipalRemainsAServicePrincipal()
    {
        var principal = AuthenticatedPrincipal.CreateService(
            PrincipalId.New(),
            new ExternalIdentity("https://workload.example.test", "worker-1"));
        var tenant = CreateTenant("tenant-a");
        var resolver = CreateResolver(
            [tenant],
            [(principal.PrincipalId, tenant.TenantId)]);

        var context = await resolver.ResolveAsync(principal, "tenant-a");

        Assert.AreEqual(PrincipalKind.Service, context.Principal.Kind);
        Assert.AreEqual(principal.PrincipalId, context.Principal.PrincipalId);
    }

    [TestMethod]
    public void ExternalIdentityRequiresIssuerAndSubject()
    {
        CaptureException<ArgumentException>(() => new ExternalIdentity(" ", "subject"));
        CaptureException<ArgumentException>(() => new ExternalIdentity("https://identity.example.test", " "));
    }

    [TestMethod]
    public void StableIdentifiersRejectEmptyValues()
    {
        CaptureException<ArgumentException>(() => new PrincipalId(Guid.Empty));
        CaptureException<ArgumentException>(() => new TenantId(Guid.Empty));
    }

    [TestMethod]
    public void IdentityBoundaryDoesNotReferenceProviderOrSpikeAssemblies()
    {
        var references = typeof(IdentityContextResolver).Assembly.GetReferencedAssemblies();

        Assert.IsFalse(
            references.Any(reference =>
                reference.Name?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true
                || reference.Name?.Contains("AspNetCore", StringComparison.OrdinalIgnoreCase) == true
                || reference.Name?.Contains("FoundationSpike", StringComparison.OrdinalIgnoreCase) == true),
            "Provider-neutral identity code must not depend on database, HTTP-host or spike implementation assemblies.");
    }

    private static AuthenticatedPrincipal CreateHumanPrincipal() =>
        AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "subject-1"));

    private static TenantDirectoryEntry CreateTenant(string slug) =>
        new(TenantId.New(), new TenantRouteSlug(slug), true);

    private static IdentityContextResolver CreateResolver(
        IReadOnlyCollection<TenantDirectoryEntry>? tenants = null,
        IReadOnlyCollection<(PrincipalId PrincipalId, TenantId TenantId)>? access = null) =>
        new(
            new InMemoryTenantDirectory(tenants ?? []),
            new InMemoryPrincipalTenantAccessEvaluator(access ?? []));

    private static async Task<TException> CaptureExceptionAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException exception)
        {
            return exception;
        }

        Assert.Fail($"Expected {typeof(TException).Name}.");
        throw new InvalidOperationException("Assert.Fail did not throw as expected.");
    }

    private static TException CaptureException<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        Assert.Fail($"Expected {typeof(TException).Name}.");
        throw new InvalidOperationException("Assert.Fail did not throw as expected.");
    }

    private sealed class InMemoryTenantDirectory : ITenantDirectory
    {
        private readonly IReadOnlyDictionary<string, TenantDirectoryEntry> _tenants;

        public InMemoryTenantDirectory(IEnumerable<TenantDirectoryEntry> tenants)
        {
            _tenants = tenants.ToDictionary(
                tenant => tenant.CanonicalSlug.Value,
                StringComparer.Ordinal);
        }

        public ValueTask<TenantDirectoryEntry?> FindBySlugAsync(
            TenantRouteSlug routeSlug,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _tenants.TryGetValue(routeSlug.Value, out var tenant);
            return ValueTask.FromResult<TenantDirectoryEntry?>(tenant);
        }
    }

    private sealed class InMemoryPrincipalTenantAccessEvaluator : IPrincipalTenantAccessEvaluator
    {
        private readonly HashSet<(PrincipalId PrincipalId, TenantId TenantId)> _access;

        public InMemoryPrincipalTenantAccessEvaluator(
            IEnumerable<(PrincipalId PrincipalId, TenantId TenantId)> access)
        {
            _access = [.. access];
        }

        public ValueTask<bool> HasAccessAsync(
            PrincipalId principalId,
            TenantId tenantId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(_access.Contains((principalId, tenantId)));
        }
    }
}
