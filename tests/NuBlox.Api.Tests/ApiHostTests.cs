using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Identity;
using NuBlox.Kernel;

namespace NuBlox.Api.Tests;

[TestClass]
public sealed class ApiHostTests
{
    private static readonly string[] HealthResponseFields = ["status"];

    private WebApplicationFactory<Program> _factory = null!;

    [TestInitialize]
    public void Initialize()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [TestCleanup]
    public void Cleanup()
    {
        _factory.Dispose();
    }

    [TestMethod]
    public async Task OperationalHealthEndpointsAreMinimalAndSuccessful()
    {
        using var client = CreateHttpsClient();

        using var liveResponse = await client.GetAsync(NuBloxApiRoutes.Liveness);
        using var readyResponse = await client.GetAsync(NuBloxApiRoutes.Readiness);

        Assert.AreEqual(HttpStatusCode.OK, liveResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, readyResponse.StatusCode);
        await AssertHealthContractAsync(liveResponse, "live");
        await AssertHealthContractAsync(readyResponse, "ready");
    }

    [TestMethod]
    public async Task UnknownRouteReturnsSanitisedRfc9457ProblemDetails()
    {
        using var client = CreateHttpsClient();
        using var response = await client.GetAsync("/not-a-real-route");
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.AreEqual(NuBloxProblemTypes.NotFound, root.GetProperty("type").GetString());
        Assert.AreEqual(404, root.GetProperty("status").GetInt32());
        Assert.IsTrue(root.TryGetProperty("correlationId", out var correlationId));
        Assert.IsFalse(string.IsNullOrWhiteSpace(correlationId.GetString()));
        Assert.IsFalse(body.Contains("stack", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(body.Contains("exception", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task OpenApiDocumentIsPublishedWithoutOperationalHealthEndpoints()
    {
        using var client = CreateHttpsClient();
        using var response = await client.GetAsync(NuBloxApiRoutes.OpenApiV1);
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(root.GetProperty("openapi").GetString()?.StartsWith("3.", StringComparison.Ordinal) == true);

        var paths = root.GetProperty("paths");
        Assert.IsFalse(paths.TryGetProperty(NuBloxApiRoutes.Liveness, out _));
        Assert.IsFalse(paths.TryGetProperty(NuBloxApiRoutes.Readiness, out _));
    }

    [TestMethod]
    public async Task RequestTenantInputDoesNotManufactureVerifiedContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["tenantSlug"] = "tenant-a";
        httpContext.Request.Headers["X-Tenant"] = "tenant-a";

        Assert.IsFalse(httpContext.TryGetVerifiedNuBloxContext(out _));

        var principal = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "subject-1"));
        var tenant = new TenantDirectoryEntry(TenantId.New(), new TenantRouteSlug("tenant-a"), true);
        var resolver = new IdentityContextResolver(
            new SingleTenantDirectory(tenant),
            new SingleTenantAccess(principal.PrincipalId, tenant.TenantId));
        var verified = await resolver.ResolveAsync(principal, "tenant-a");

        httpContext.SetVerifiedNuBloxContext(verified);

        Assert.IsTrue(httpContext.TryGetVerifiedNuBloxContext(out var resolved));
        Assert.IsNotNull(resolved);
        Assert.AreEqual(tenant.TenantId, resolved.Tenant.TenantId);
    }

    [TestMethod]
    public void ProblemTypeMappingUsesStableNuBloxCategories()
    {
        var forbidden = new ProblemDetails();
        var unavailable = new ProblemDetails();
        var unexpected = new ProblemDetails();

        NuBloxProblemTypes.Apply(forbidden, StatusCodes.Status403Forbidden);
        NuBloxProblemTypes.Apply(unavailable, StatusCodes.Status503ServiceUnavailable);
        NuBloxProblemTypes.Apply(unexpected, StatusCodes.Status500InternalServerError);

        Assert.AreEqual(NuBloxProblemTypes.Forbidden, forbidden.Type);
        Assert.AreEqual(NuBloxProblemTypes.DependencyUnavailable, unavailable.Type);
        Assert.AreEqual(NuBloxProblemTypes.UnexpectedFailure, unexpected.Type);
        Assert.IsNull(unexpected.Detail);
        Assert.IsNull(unexpected.Instance);
    }

    [TestMethod]
    public void ApiHostDoesNotReferencePersistenceProviderOrSpikeAssemblies()
    {
        var references = typeof(NuBloxApiRoutes).Assembly.GetReferencedAssemblies();

        Assert.IsFalse(references.Any(reference =>
            reference.Name?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true
            || reference.Name?.Contains("Persistence.PostgreSql", StringComparison.OrdinalIgnoreCase) == true
            || reference.Name?.Contains("FoundationSpike", StringComparison.OrdinalIgnoreCase) == true));
    }

    private HttpClient CreateHttpsClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost", UriKind.Absolute),
            AllowAutoRedirect = false
        });

    private static async Task AssertHealthContractAsync(HttpResponseMessage response, string expectedStatus)
    {
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var propertyNames = root.EnumerateObject().Select(property => property.Name).ToArray();

        CollectionAssert.AreEquivalent(HealthResponseFields, propertyNames);
        Assert.AreEqual(expectedStatus, root.GetProperty("status").GetString());
    }

    private sealed class SingleTenantDirectory(TenantDirectoryEntry tenant) : ITenantDirectory
    {
        public ValueTask<TenantDirectoryEntry?> FindBySlugAsync(
            TenantRouteSlug routeSlug,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<TenantDirectoryEntry?>(
                routeSlug == tenant.CanonicalSlug ? tenant : null);
        }
    }

    private sealed class SingleTenantAccess(PrincipalId principalId, TenantId tenantId) : IPrincipalTenantAccessEvaluator
    {
        public ValueTask<bool> HasAccessAsync(
            PrincipalId requestedPrincipalId,
            TenantId requestedTenantId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(
                requestedPrincipalId == principalId && requestedTenantId == tenantId);
        }
    }
}
