using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Identity;
using NuBlox.Kernel;
using NuBlox.WorkProducts;

namespace NuBlox.Api.Tests;

[TestClass]
public sealed class WorkProductEndpointTests
{
    [TestMethod]
    public async Task MissingVerifiedContextFailsClosed()
    {
        var operations = new RecordingOperations();
        using var factory = CreateFactory(operations);
        using var client = CreateHttpsClient(factory);

        using var response = await client.PostAsJsonAsync(
            "/api/v1/tenant-a/work-products",
            new CreateWorkProductRequest("Test", "generic"));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual(0, operations.CallCount);
        await AssertProblemAsync(response, NuBloxProblemTypes.Unauthenticated, 401);
    }

    [TestMethod]
    public async Task RouteTenantCannotOverrideVerifiedTenantContext()
    {
        var operations = new RecordingOperations();
        var verified = await CreateVerifiedContextAsync("tenant-a");
        using var factory = CreateFactory(operations, verified);
        using var client = CreateHttpsClient(factory);

        using var response = await client.GetAsync($"/api/v1/tenant-b/work-products/{Guid.NewGuid():D}");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.AreEqual(0, operations.CallCount);
        await AssertProblemAsync(response, NuBloxProblemTypes.Forbidden, 403);
    }

    [TestMethod]
    public async Task BodyAndHeaderTenantOrActorValuesCannotManufactureTrustedContext()
    {
        var operations = new RecordingOperations();
        var verified = await CreateVerifiedContextAsync("tenant-a");
        using var factory = CreateFactory(operations, verified);
        using var client = CreateHttpsClient(factory);

        var spoofedTenant = Guid.NewGuid();
        var spoofedActor = Guid.NewGuid();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tenant-a/work-products")
        {
            Content = JsonContent.Create(new CreateWorkProductRequest(
                "Controlled output",
                "generic",
                TenantId: spoofedTenant,
                ActorPrincipalId: spoofedActor))
        };
        request.Headers.Add("X-Tenant", "tenant-b");
        request.Headers.Add("X-Principal-Id", spoofedActor.ToString("D"));

        using var response = await client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.AreEqual(1, operations.CallCount);
        Assert.IsNotNull(operations.LastContext);
        Assert.AreEqual(verified.Tenant.TenantId, operations.LastContext.Tenant.TenantId);
        Assert.AreEqual(verified.Principal.PrincipalId, operations.LastContext.Principal.PrincipalId);
        Assert.AreNotEqual(spoofedTenant, operations.LastContext.Tenant.TenantId.Value);
        Assert.AreNotEqual(spoofedActor, operations.LastContext.Principal.PrincipalId.Value);
    }

    [TestMethod]
    public async Task WorkProductConflictUsesSanitisedRfc9457Problem()
    {
        var operations = new RecordingOperations
        {
            Failure = new WorkProductStateConflictException("secret internal state detail")
        };
        var verified = await CreateVerifiedContextAsync("tenant-a");
        using var factory = CreateFactory(operations, verified);
        using var client = CreateHttpsClient(factory);

        using var response = await client.PostAsJsonAsync(
            $"/api/v1/tenant-a/work-products/{Guid.NewGuid():D}/submit",
            new SubmitWorkProductRequest(Guid.NewGuid()));
        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        Assert.IsFalse(body.Contains("secret internal state detail", StringComparison.Ordinal));
        Assert.IsFalse(body.Contains("stack", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(body.Contains("exception", StringComparison.OrdinalIgnoreCase));
        await AssertProblemAsync(response, NuBloxProblemTypes.Conflict, 409);
    }

    [TestMethod]
    public async Task WorkProductRoutesArePublishedInOpenApi()
    {
        var operations = new RecordingOperations();
        using var factory = CreateFactory(operations);
        using var client = CreateHttpsClient(factory);

        using var response = await client.GetAsync(NuBloxApiRoutes.OpenApiV1);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        Assert.IsTrue(paths.TryGetProperty("/api/v1/{tenantSlug}/work-products", out _));
        Assert.IsTrue(paths.TryGetProperty("/api/v1/{tenantSlug}/work-products/{workProductId}", out _));
        Assert.IsTrue(paths.TryGetProperty("/api/v1/{tenantSlug}/work-products/{workProductId}/submit", out _));
        Assert.IsTrue(paths.TryGetProperty("/api/v1/{tenantSlug}/work-products/{workProductId}/decisions", out _));
        Assert.IsTrue(paths.TryGetProperty("/api/v1/{tenantSlug}/work-products/{workProductId}/issue", out _));
    }

    private static WebApplicationFactory<Program> CreateFactory(
        IWorkProductHttpOperations operations,
        AuthenticatedRequestContext? verifiedContext = null) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IWorkProductHttpOperations>();
                    services.AddSingleton(operations);
                    if (verifiedContext is not null)
                    {
                        services.AddSingleton<IStartupFilter>(new VerifiedContextStartupFilter(verifiedContext));
                    }
                });
            });

    private static HttpClient CreateHttpsClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost", UriKind.Absolute),
            AllowAutoRedirect = false
        });

    private static async Task<AuthenticatedRequestContext> CreateVerifiedContextAsync(string slug)
    {
        var principal = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "subject-work-products"));
        var tenant = new TenantDirectoryEntry(TenantId.New(), new TenantRouteSlug(slug), true);
        var resolver = new IdentityContextResolver(
            new SingleTenantDirectory(tenant),
            new SingleTenantAccess(principal.PrincipalId, tenant.TenantId));
        return await resolver.ResolveAsync(principal, slug);
    }

    private static async Task AssertProblemAsync(
        HttpResponseMessage response,
        string expectedType,
        int expectedStatus)
    {
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.AreEqual(expectedType, root.GetProperty("type").GetString());
        Assert.AreEqual(expectedStatus, root.GetProperty("status").GetInt32());
    }

    private sealed class VerifiedContextStartupFilter(AuthenticatedRequestContext context) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (httpContext, middlewareNext) =>
            {
                httpContext.SetVerifiedNuBloxContext(context);
                await middlewareNext();
            });
            next(app);
        };
    }

    private sealed class RecordingOperations : IWorkProductHttpOperations
    {
        public int CallCount { get; private set; }
        public AuthenticatedRequestContext? LastContext { get; private set; }
        public Exception? Failure { get; init; }

        public Task<WorkProductContract> CreateAsync(
            AuthenticatedRequestContext context,
            CreateWorkProductRequest request,
            CancellationToken cancellationToken)
        {
            Record(context);
            ThrowIfConfigured();
            return Task.FromResult(new WorkProductContract(
                Guid.NewGuid(),
                request.Title,
                request.ProductType,
                context.Principal.PrincipalId.Value,
                1,
                WorkProductRevisionState.Draft.ToString()));
        }

        public Task<WorkProductContract?> FindAsync(
            AuthenticatedRequestContext context,
            WorkProductId workProductId,
            CancellationToken cancellationToken)
        {
            Record(context);
            ThrowIfConfigured();
            return Task.FromResult<WorkProductContract?>(new WorkProductContract(
                workProductId.Value,
                "Found",
                "generic",
                context.Principal.PrincipalId.Value,
                1,
                WorkProductRevisionState.Draft.ToString()));
        }

        public Task<ReviewSubmissionContract> SubmitAsync(
            AuthenticatedRequestContext context,
            WorkProductId workProductId,
            SubmitWorkProductRequest request,
            CancellationToken cancellationToken)
        {
            Record(context);
            ThrowIfConfigured();
            return Task.FromResult(new ReviewSubmissionContract(
                workProductId.Value,
                Guid.NewGuid(),
                1,
                WorkProductRevisionState.InReview.ToString(),
                Guid.NewGuid(),
                request.ReviewerPrincipalId));
        }

        public Task<ReviewDecisionContract> DecideAsync(
            AuthenticatedRequestContext context,
            WorkProductId workProductId,
            DecideWorkProductRequest request,
            string correlationId,
            CancellationToken cancellationToken)
        {
            Record(context);
            ThrowIfConfigured();
            return Task.FromResult(new ReviewDecisionContract(
                workProductId.Value,
                Guid.NewGuid(),
                WorkProductRevisionState.Approved.ToString(),
                Guid.NewGuid(),
                request.Outcome.ToString()));
        }

        public Task<IssueWorkProductContract> IssueAsync(
            AuthenticatedRequestContext context,
            WorkProductId workProductId,
            string correlationId,
            CancellationToken cancellationToken)
        {
            Record(context);
            ThrowIfConfigured();
            return Task.FromResult(new IssueWorkProductContract(
                workProductId.Value,
                Guid.NewGuid(),
                WorkProductRevisionState.Issued.ToString(),
                Guid.NewGuid()));
        }

        private void Record(AuthenticatedRequestContext context)
        {
            CallCount++;
            LastContext = context;
        }

        private void ThrowIfConfigured()
        {
            if (Failure is not null) throw Failure;
        }
    }

    private sealed class SingleTenantDirectory(TenantDirectoryEntry tenant) : ITenantDirectory
    {
        public ValueTask<TenantDirectoryEntry?> FindBySlugAsync(
            TenantRouteSlug routeSlug,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<TenantDirectoryEntry?>(
                routeSlug == tenant.CanonicalSlug ? tenant : null);
    }

    private sealed class SingleTenantAccess(PrincipalId principalId, TenantId tenantId) : IPrincipalTenantAccessEvaluator
    {
        public ValueTask<bool> HasAccessAsync(
            PrincipalId requestedPrincipalId,
            TenantId requestedTenantId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(
                requestedPrincipalId == principalId && requestedTenantId == tenantId);
    }
}
