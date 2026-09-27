using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using NuBlox.Identity;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.Runtime.PostgreSql;
using NuBlox.WorkProducts;

namespace NuBlox.Api.Tests;

[TestClass]
[DoNotParallelize]
public sealed class WorkProductRuntimeCompositionTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_runtime_api_test";
    private const string RuntimePassword = "nublox_runtime_api_test_only";

    private string _migrationConnectionString = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _migrationConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_migrationConnectionString))
        {
            Assert.Inconclusive($"Set {ConnectionStringVariable} to run the real runtime-composition smoke test.");
        }

        await ResetSchemasAsync();
        await using var migrationDataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        var runner = new PostgresMigrationRunner(migrationDataSource, PostgresRuntimeMigrations.LoadAll());
        await runner.ApplyAsync();
        await EnsureRuntimeRoleAsync(migrationDataSource);
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (!string.IsNullOrWhiteSpace(_migrationConnectionString))
        {
            await ResetSchemasAsync();
        }
    }

    [TestMethod]
    public async Task RealHostExecutesGovernedWorkProductFlowAgainstPostgreSql()
    {
        var tenantA = new TenantDirectoryEntry(TenantId.New(), new TenantRouteSlug("tenant-a"), true);
        var tenantB = new TenantDirectoryEntry(TenantId.New(), new TenantRouteSlug("tenant-b"), true);
        var contributor = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "contributor"));
        var reviewer = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "reviewer"));
        var outsider = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "outsider"));

        await SeedTenantAsync(tenantA);
        await SeedTenantAsync(tenantB);

        var runtimeConnectionString = new NpgsqlConnectionStringBuilder(_migrationConnectionString)
        {
            Username = RuntimeRole,
            Password = RuntimePassword,
            Pooling = false
        }.ConnectionString;

        var contributorContext = await ResolveAsync(contributor, tenantA);
        var reviewerContext = await ResolveAsync(reviewer, tenantA);
        var outsiderContext = await ResolveAsync(outsider, tenantB);

        using var contributorFactory = CreateRuntimeFactory(runtimeConnectionString, reviewer.PrincipalId, contributorContext);
        using var contributorClient = CreateHttpsClient(contributorFactory);

        const string sensitiveTitle = "Controlled output — no telemetry payload leakage";
        using var createResponse = await contributorClient.PostAsJsonAsync(
            "/api/v1/tenant-a/work-products",
            new CreateWorkProductRequest(sensitiveTitle, "generic"));
        Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<WorkProductContract>();
        Assert.IsNotNull(created);

        using var submitResponse = await contributorClient.PostAsJsonAsync(
            $"/api/v1/tenant-a/work-products/{created.WorkProductId:D}/submit",
            new SubmitWorkProductRequest(reviewer.PrincipalId.Value));
        Assert.AreEqual(HttpStatusCode.OK, submitResponse.StatusCode);
        var submitted = await submitResponse.Content.ReadFromJsonAsync<ReviewSubmissionContract>();
        Assert.IsNotNull(submitted);

        using var reviewerFactory = CreateRuntimeFactory(runtimeConnectionString, reviewer.PrincipalId, reviewerContext);
        using var reviewerClient = CreateHttpsClient(reviewerFactory);

        const string sensitiveRationale = "Approved because controlled evidence is complete — do not copy into telemetry";
        using var decisionResponse = await reviewerClient.PostAsJsonAsync(
            $"/api/v1/tenant-a/work-products/{created.WorkProductId:D}/decisions",
            new DecideWorkProductRequest(
                submitted.ReviewRequestId,
                ReviewDecisionOutcome.Approved,
                sensitiveRationale));
        Assert.AreEqual(HttpStatusCode.OK, decisionResponse.StatusCode);

        using var issueResponse = await contributorClient.PostAsync(
            $"/api/v1/tenant-a/work-products/{created.WorkProductId:D}/issue",
            content: null);
        Assert.AreEqual(HttpStatusCode.OK, issueResponse.StatusCode);

        using var outsiderFactory = CreateRuntimeFactory(runtimeConnectionString, reviewer.PrincipalId, outsiderContext);
        using var outsiderClient = CreateHttpsClient(outsiderFactory);
        using var crossTenantResponse = await outsiderClient.GetAsync(
            $"/api/v1/tenant-b/work-products/{created.WorkProductId:D}");
        Assert.AreEqual(HttpStatusCode.NotFound, crossTenantResponse.StatusCode);

        await using var migrationDataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        await using var connection = await migrationDataSource.OpenConnectionAsync();

        Assert.AreEqual(4L, await ScalarInt64Async(
            connection,
            "SELECT count(*) FROM audit.events WHERE tenant_id = @tenant_id;",
            tenantA.TenantId.Value));
        Assert.AreEqual(0L, await ScalarInt64Async(
            connection,
            "SELECT count(*) FROM audit.events WHERE tenant_id = @tenant_id;",
            tenantB.TenantId.Value));

        await using var payloadLeakCommand = new NpgsqlCommand(
            """
            SELECT count(*)
            FROM audit.events
            WHERE tenant_id = @tenant_id
              AND (subject_id = @title OR reason_reference = @title
                   OR subject_id = @rationale OR reason_reference = @rationale);
            """,
            connection);
        payloadLeakCommand.Parameters.AddWithValue("tenant_id", tenantA.TenantId.Value);
        payloadLeakCommand.Parameters.AddWithValue("title", sensitiveTitle);
        payloadLeakCommand.Parameters.AddWithValue("rationale", sensitiveRationale);
        Assert.AreEqual(0L, Convert.ToInt64(await payloadLeakCommand.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture));
    }

    private static WebApplicationFactory<Program> CreateRuntimeFactory(
        string runtimeConnectionString,
        PrincipalId approvalPrincipal,
        AuthenticatedRequestContext context) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureTestServices(services =>
                {
                    services.AddNuBloxPostgresRuntime(runtimeConnectionString, [approvalPrincipal]);
                    services.AddSingleton<IStartupFilter>(new VerifiedContextStartupFilter(context));
                });
            });

    private static HttpClient CreateHttpsClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost", UriKind.Absolute),
            AllowAutoRedirect = false
        });

    private static async Task<AuthenticatedRequestContext> ResolveAsync(
        AuthenticatedPrincipal principal,
        TenantDirectoryEntry tenant)
    {
        var resolver = new IdentityContextResolver(
            new SingleTenantDirectory(tenant),
            new SingleTenantAccess(principal.PrincipalId, tenant.TenantId));
        return await resolver.ResolveAsync(principal, tenant.CanonicalSlug.Value);
    }

    private async Task SeedTenantAsync(TenantDirectoryEntry tenant)
    {
        await using var dataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "INSERT INTO kernel.tenants (tenant_id, tenant_slug) VALUES (@tenant_id, @tenant_slug);",
            connection);
        command.Parameters.AddWithValue("tenant_id", tenant.TenantId.Value);
        command.Parameters.AddWithValue("tenant_slug", tenant.CanonicalSlug.Value);
        await command.ExecuteNonQueryAsync();
    }

    private async Task ResetSchemasAsync()
    {
        await using var dataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "DROP SCHEMA IF EXISTS audit CASCADE; DROP SCHEMA IF EXISTS work_products CASCADE; DROP SCHEMA IF EXISTS kernel CASCADE; DROP SCHEMA IF EXISTS nublox_meta CASCADE;",
            connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task EnsureRuntimeRoleAsync(NpgsqlDataSource migrationDataSource)
    {
        await using var connection = await migrationDataSource.OpenConnectionAsync();
        await using (var createRole = new NpgsqlCommand(
            $"""
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '{RuntimeRole}') THEN
                    CREATE ROLE {RuntimeRole} LOGIN PASSWORD '{RuntimePassword}' NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION;
                ELSE
                    ALTER ROLE {RuntimeRole} WITH LOGIN PASSWORD '{RuntimePassword}' NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION;
                END IF;
            END
            $$;
            """,
            connection))
        {
            await createRole.ExecuteNonQueryAsync();
        }

        await using var grants = new NpgsqlCommand(
            $"""
            GRANT USAGE ON SCHEMA work_products, audit TO {RuntimeRole};
            GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA work_products TO {RuntimeRole};
            GRANT SELECT, INSERT ON audit.events, audit.evidence_references TO {RuntimeRole};
            """,
            connection);
        await grants.ExecuteNonQueryAsync();
    }

    private static async Task<long> ScalarInt64Async(
        NpgsqlConnection connection,
        string sql,
        Guid tenantId)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        return Convert.ToInt64(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
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

    private sealed class SingleTenantDirectory(TenantDirectoryEntry tenant) : ITenantDirectory
    {
        public ValueTask<TenantDirectoryEntry?> FindBySlugAsync(
            TenantRouteSlug routeSlug,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<TenantDirectoryEntry?>(routeSlug == tenant.CanonicalSlug ? tenant : null);
    }

    private sealed class SingleTenantAccess(PrincipalId principalId, TenantId tenantId) : IPrincipalTenantAccessEvaluator
    {
        public ValueTask<bool> HasAccessAsync(
            PrincipalId requestedPrincipalId,
            TenantId requestedTenantId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(requestedPrincipalId == principalId && requestedTenantId == tenantId);
    }
}
