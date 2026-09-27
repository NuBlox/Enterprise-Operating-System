using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using NuBlox.Enterprise;
using NuBlox.Identity;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.Runtime.PostgreSql;

namespace NuBlox.Api.Tests;

[TestClass]
[DoNotParallelize]
public sealed class PersonRuntimeCompositionTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_person_api_runtime_test";
    private const string RuntimePassword = "nublox_person_api_runtime_test_only";

    private string _migrationConnectionString = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _migrationConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_migrationConnectionString))
        {
            Assert.Inconclusive($"Set {ConnectionStringVariable} to run the real Person runtime-composition smoke test.");
        }

        await ResetSchemasAsync();
        await using var migrationDataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        await new PostgresMigrationRunner(migrationDataSource, PostgresRuntimeMigrations.LoadAll()).ApplyAsync();
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
    public async Task RealHostCreatesPersonIndependentOfPrincipalAgainstPostgreSql()
    {
        var tenantA = new TenantDirectoryEntry(TenantId.New(), new TenantRouteSlug("tenant-a"), true);
        var tenantB = new TenantDirectoryEntry(TenantId.New(), new TenantRouteSlug("tenant-b"), true);
        var administratorA = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "person-admin-a"));
        var administratorB = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "person-admin-b"));
        var deniedPrincipal = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "person-denied"));

        await SeedTenantAsync(tenantA);
        await SeedTenantAsync(tenantB);

        var runtimeConnectionString = new NpgsqlConnectionStringBuilder(_migrationConnectionString)
        {
            Username = RuntimeRole,
            Password = RuntimePassword,
            Pooling = false
        }.ConnectionString;

        var administratorAContext = await ResolveAsync(administratorA, tenantA);
        var administratorBContext = await ResolveAsync(administratorB, tenantB);
        var deniedContext = await ResolveAsync(deniedPrincipal, tenantA);
        var personAdministrators = new[] { administratorA.PrincipalId, administratorB.PrincipalId };

        using var administratorAFactory = CreateRuntimeFactory(
            runtimeConnectionString,
            administratorAContext,
            personAdministrators);
        using var administratorAClient = CreateHttpsClient(administratorAFactory);

        const string sensitiveDisplayName = "Alex Morgan — private display identity";
        using var createResponse = await administratorAClient.PostAsJsonAsync(
            "/api/v1/tenant-a/people",
            new CreatePersonRequest(
                sensitiveDisplayName,
                TenantId: tenantB.TenantId.Value,
                ActorPrincipalId: deniedPrincipal.PrincipalId.Value));
        Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<PersonContract>();
        Assert.IsNotNull(created);
        Assert.AreEqual(PartyKind.Person.ToString(), created.PartyKind);
        Assert.AreEqual(sensitiveDisplayName, created.DisplayName);
        Assert.AreNotEqual(administratorA.PrincipalId.Value, created.PartyId);
        Assert.AreNotEqual(deniedPrincipal.PrincipalId.Value, created.PartyId);

        using var sameTenantRead = await administratorAClient.GetAsync(
            $"/api/v1/tenant-a/people/{created.PartyId:D}");
        Assert.AreEqual(HttpStatusCode.OK, sameTenantRead.StatusCode);

        using var deniedFactory = CreateRuntimeFactory(
            runtimeConnectionString,
            deniedContext,
            personAdministrators);
        using var deniedClient = CreateHttpsClient(deniedFactory);
        using var deniedCreate = await deniedClient.PostAsJsonAsync(
            "/api/v1/tenant-a/people",
            new CreatePersonRequest("Denied Person"));
        Assert.AreEqual(HttpStatusCode.Forbidden, deniedCreate.StatusCode);

        using var administratorBFactory = CreateRuntimeFactory(
            runtimeConnectionString,
            administratorBContext,
            personAdministrators);
        using var administratorBClient = CreateHttpsClient(administratorBFactory);
        using var crossTenantRead = await administratorBClient.GetAsync(
            $"/api/v1/tenant-b/people/{created.PartyId:D}");
        Assert.AreEqual(HttpStatusCode.NotFound, crossTenantRead.StatusCode);

        await using var migrationDataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        await using var connection = await migrationDataSource.OpenConnectionAsync();

        await using (var partyCommand = new NpgsqlCommand(
            """
            SELECT party_kind, created_by_principal_id
            FROM enterprise.parties
            WHERE tenant_id = @tenant_id
              AND party_id = @party_id;
            """,
            connection))
        {
            partyCommand.Parameters.AddWithValue("tenant_id", tenantA.TenantId.Value);
            partyCommand.Parameters.AddWithValue("party_id", created.PartyId);
            await using var reader = await partyCommand.ExecuteReaderAsync();
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("PERSON", reader.GetString(0));
            Assert.AreEqual(administratorA.PrincipalId.Value, reader.GetGuid(1));
        }

        await using (var auditActorCommand = new NpgsqlCommand(
            """
            SELECT actor_principal_id
            FROM audit.events
            WHERE tenant_id = @tenant_id
              AND action_code = 'enterprise.person.create'
              AND subject_id = @party_id;
            """,
            connection))
        {
            auditActorCommand.Parameters.AddWithValue("tenant_id", tenantA.TenantId.Value);
            auditActorCommand.Parameters.AddWithValue("party_id", created.PartyId.ToString("D"));
            var auditActor = await auditActorCommand.ExecuteScalarAsync();
            Assert.AreEqual(administratorA.PrincipalId.Value, Assert.IsInstanceOfType<Guid>(auditActor));
        }

        Assert.AreEqual(1L, await ScalarInt64Async(
            connection,
            "SELECT count(*) FROM audit.events WHERE tenant_id = @tenant_id AND action_code = 'enterprise.person.create';",
            tenantA.TenantId.Value));
        Assert.AreEqual(0L, await ScalarInt64Async(
            connection,
            "SELECT count(*) FROM audit.events WHERE tenant_id = @tenant_id AND action_code = 'enterprise.person.create';",
            tenantB.TenantId.Value));

        await using var payloadLeakCommand = new NpgsqlCommand(
            """
            SELECT count(*)
            FROM audit.events
            WHERE tenant_id = @tenant_id
              AND (subject_id = @display_name OR reason_reference = @display_name);
            """,
            connection);
        payloadLeakCommand.Parameters.AddWithValue("tenant_id", tenantA.TenantId.Value);
        payloadLeakCommand.Parameters.AddWithValue("display_name", sensitiveDisplayName);
        Assert.AreEqual(0L, Convert.ToInt64(await payloadLeakCommand.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture));
    }

    private static WebApplicationFactory<Program> CreateRuntimeFactory(
        string runtimeConnectionString,
        AuthenticatedRequestContext context,
        IEnumerable<PrincipalId> personAdministrators) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureTestServices(services =>
                {
                    services.AddNuBloxPostgresRuntime(
                        runtimeConnectionString,
                        workProductApprovalPrincipals: null,
                        organisationAdministratorPrincipals: null,
                        personAdministratorPrincipals: personAdministrators);
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
            "DROP SCHEMA IF EXISTS audit CASCADE; DROP SCHEMA IF EXISTS work_products CASCADE; DROP SCHEMA IF EXISTS enterprise CASCADE; DROP SCHEMA IF EXISTS kernel CASCADE; DROP SCHEMA IF EXISTS nublox_meta CASCADE;",
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
            GRANT USAGE ON SCHEMA enterprise, audit TO {RuntimeRole};
            GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA enterprise TO {RuntimeRole};
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
