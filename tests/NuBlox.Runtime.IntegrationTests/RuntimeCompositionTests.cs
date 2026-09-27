using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using NuBlox.Api;
using NuBlox.Audit.Infrastructure.PostgreSql;
using NuBlox.Identity;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Infrastructure.PostgreSql;

namespace NuBlox.Runtime.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class RuntimeCompositionTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private string _connectionString = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            Assert.Inconclusive($"Set {ConnectionStringVariable} to run runtime composition integration tests.");
        }

        await ResetDatabaseAsync();
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await new PostgresMigrationRunner(dataSource).ApplyAsync();
        await new PostgresMigrationRunner(dataSource, WorkProductsPostgresMigrations.Load()).ApplyAsync();
        await new PostgresMigrationRunner(dataSource, AuditPostgresMigrations.Load()).ApplyAsync();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (!string.IsNullOrWhiteSpace(_connectionString))
        {
            await ResetDatabaseAsync();
        }
    }

    [TestMethod]
    public async Task RealApiCreateFlowResolvesRuntimePersistsWorkProductAndAuditEvidence()
    {
        var tenantId = TenantId.New();
        var principal = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "runtime-composition-contributor"));
        var tenantEntry = new TenantDirectoryEntry(tenantId, new TenantRouteSlug("tenant-a"), true);
        var resolver = new IdentityContextResolver(
            new SingleTenantDirectory(tenantEntry),
            new SingleTenantAccess(principal.PrincipalId, tenantId));
        var verifiedContext = await resolver.ResolveAsync(principal, "tenant-a");

        await InsertTenantAsync(tenantId, "tenant-a");

        using var factory = new WebApplicationFactory<global::Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:NuBloxPostgres"] = _connectionString
                    }));
                builder.ConfigureTestServices(services =>
                    services.AddSingleton<IStartupFilter>(new VerifiedContextStartupFilter(verifiedContext)));
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost", UriKind.Absolute),
            AllowAutoRedirect = false
        });

        using var response = await client.PostAsJsonAsync(
            "/api/v1/tenant-a/work-products",
            new CreateWorkProductRequest("Runtime composition proof", "governed-document"));
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        var workProductId = document.RootElement.GetProperty("workProductId").GetGuid();

        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value);

        Assert.AreEqual(1L, await CountAsync(
            connection,
            transaction,
            "SELECT count(*) FROM work_products.work_products WHERE tenant_id = @tenant_id AND work_product_id = @subject_id;",
            tenantId.Value,
            workProductId));
        Assert.AreEqual(1L, await CountAsync(
            connection,
            transaction,
            "SELECT count(*) FROM audit.evidence WHERE tenant_id = @tenant_id AND subject_type = 'work_product' AND subject_id = @subject_id_text AND action_code = 'work_product.create';",
            tenantId.Value,
            workProductId));

        await transaction.CommitAsync();
    }

    private async Task InsertTenantAsync(TenantId tenantId, string tenantSlug)
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value);

        await using var command = new NpgsqlCommand(
            "INSERT INTO kernel.tenants (tenant_id, tenant_slug) VALUES (@tenant_id, @tenant_slug);",
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("tenant_slug", tenantSlug);
        await command.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
    }

    private static async Task<long> CountAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string sql,
        Guid tenantId,
        Guid subjectId)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        if (sql.Contains("@subject_id_text", StringComparison.Ordinal))
        {
            command.Parameters.AddWithValue("subject_id_text", subjectId.ToString("D"));
        }
        else
        {
            command.Parameters.AddWithValue("subject_id", subjectId);
        }

        return Convert.ToInt64(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "DROP SCHEMA IF EXISTS audit CASCADE; DROP SCHEMA IF EXISTS work_products CASCADE; DROP SCHEMA IF EXISTS kernel CASCADE; DROP SCHEMA IF EXISTS nublox_meta CASCADE;",
            connection);
        await command.ExecuteNonQueryAsync();
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
