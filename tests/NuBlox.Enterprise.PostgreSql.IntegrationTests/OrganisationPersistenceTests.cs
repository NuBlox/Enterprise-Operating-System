using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using NuBlox.Enterprise;
using NuBlox.Enterprise.Infrastructure.PostgreSql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;

namespace NuBlox.Enterprise.PostgreSql.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class OrganisationPersistenceTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_enterprise_runtime_test";
    private const string RuntimePassword = "nublox_enterprise_runtime_test_only";

    private string _migrationConnectionString = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _migrationConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_migrationConnectionString))
        {
            Assert.Inconclusive($"Set {ConnectionStringVariable} to run the Enterprise PostgreSQL integration tests.");
        }

        await ResetSchemasAsync();
        await using var migrationDataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        await new PostgresMigrationRunner(migrationDataSource).ApplyAsync();
        await new PostgresMigrationRunner(migrationDataSource, EnterprisePostgresMigrations.Load()).ApplyAsync();
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
    public async Task RepositoryPersistsOrganisationKindAndEnforcesTenantReadIsolation()
    {
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();
        var actor = PrincipalId.New();
        await SeedTenantAsync(tenantA, "tenant-a");
        await SeedTenantAsync(tenantB, "tenant-b");

        var runtimeConnectionString = CreateRuntimeConnectionString();
        await using var runtimeDataSource = NpgsqlDataSource.Create(runtimeConnectionString);
        var repository = new PostgresOrganisationRepository(runtimeDataSource);
        var organisation = Organisation.Create(
            tenantA,
            "Acme Design Ltd",
            actor,
            new DateTimeOffset(2026, 9, 27, 9, 0, 0, TimeSpan.Zero));

        await repository.AddAsync(organisation);

        var sameTenant = await repository.FindAsync(tenantA, organisation.Id);
        var otherTenant = await repository.FindAsync(tenantB, organisation.Id);

        Assert.IsNotNull(sameTenant);
        Assert.AreEqual(PartyKind.Organisation, sameTenant.Kind);
        Assert.AreEqual("Acme Design Ltd", sameTenant.DisplayName);
        Assert.IsNull(otherTenant);

        await using var migrationDataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        await using var connection = await migrationDataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT party_kind FROM enterprise.parties WHERE tenant_id = @tenant_id AND party_id = @party_id;",
            connection);
        command.Parameters.AddWithValue("tenant_id", tenantA.Value);
        command.Parameters.AddWithValue("party_id", organisation.Id.Value);
        Assert.AreEqual("ORGANISATION", Convert.ToString(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public async Task RuntimeRoleCannotWriteOrganisationForDifferentTenantContext()
    {
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();
        await SeedTenantAsync(tenantA, "tenant-a");
        await SeedTenantAsync(tenantB, "tenant-b");

        await using var runtimeDataSource = NpgsqlDataSource.Create(CreateRuntimeConnectionString());
        await using var connection = await runtimeDataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantA.Value);

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO enterprise.parties (
                tenant_id, party_id, party_kind, created_by_principal_id, created_at)
            VALUES (@tenant_id, @party_id, 'ORGANISATION', @principal_id, now());
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantB.Value);
        command.Parameters.AddWithValue("party_id", Guid.NewGuid());
        command.Parameters.AddWithValue("principal_id", Guid.NewGuid());

        var exception = await Assert.ThrowsExactlyAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
        Assert.AreEqual(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
        await transaction.RollbackAsync();
    }

    private string CreateRuntimeConnectionString() =>
        new NpgsqlConnectionStringBuilder(_migrationConnectionString)
        {
            Username = RuntimeRole,
            Password = RuntimePassword,
            Pooling = false
        }.ConnectionString;

    private async Task SeedTenantAsync(TenantId tenantId, string slug)
    {
        await using var dataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "INSERT INTO kernel.tenants (tenant_id, tenant_slug) VALUES (@tenant_id, @tenant_slug);",
            connection);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("tenant_slug", slug);
        await command.ExecuteNonQueryAsync();
    }

    private async Task ResetSchemasAsync()
    {
        await using var dataSource = NpgsqlDataSource.Create(_migrationConnectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "DROP SCHEMA IF EXISTS enterprise CASCADE; DROP SCHEMA IF EXISTS kernel CASCADE; DROP SCHEMA IF EXISTS nublox_meta CASCADE;",
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
            GRANT USAGE ON SCHEMA enterprise TO {RuntimeRole};
            GRANT SELECT, INSERT ON enterprise.parties, enterprise.organisations TO {RuntimeRole};
            """,
            connection);
        await grants.ExecuteNonQueryAsync();
    }
}
