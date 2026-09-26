using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using NuBlox.Kernel.Identity;
using NuBlox.Persistence.PostgreSql;

namespace NuBlox.Persistence.PostgreSql.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class PostgresPersistenceTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_runtime_test";
    private const string RuntimePassword = "nublox_runtime_ci_only";

    private string _connectionString = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            Assert.Inconclusive($"Set {ConnectionStringVariable} to run PostgreSQL integration tests.");
        }

        await ResetDatabaseAsync();
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
    public async Task MigrationsAreReplaySafeOwnedAndChecksumProtected()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var runner = new PostgresMigrationRunner(dataSource);

        await runner.ApplyAsync();
        await runner.ApplyAsync();

        await using var connection = await dataSource.OpenConnectionAsync();

        Assert.AreEqual(1L, await ScalarInt64Async(
            connection,
            "SELECT count(*) FROM nublox_meta.schema_migrations;"));

        Assert.AreEqual(1L, await ScalarInt64Async(
            connection,
            "SELECT count(*) FROM pg_namespace WHERE nspname = 'kernel' AND nspowner = (SELECT oid FROM pg_roles WHERE rolname = current_user);"));

        await using (var tamper = new NpgsqlCommand(
            "UPDATE nublox_meta.schema_migrations SET sha256 = repeat('0', 64);",
            connection))
        {
            await tamper.ExecuteNonQueryAsync();
        }

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => runner.ApplyAsync());
    }

    [TestMethod]
    public async Task RuntimeRoleIsTenantScopedAndCannotPerformSchemaDdl()
    {
        await using var migrationDataSource = NpgsqlDataSource.Create(_connectionString);
        var runner = new PostgresMigrationRunner(migrationDataSource);
        await runner.ApplyAsync();

        var tenantA = TenantId.New();
        var tenantB = TenantId.New();
        await InsertTenantAsync(migrationDataSource, tenantA, "tenant-a");
        await InsertTenantAsync(migrationDataSource, tenantB, "tenant-b");
        await CreateRestrictedRuntimeRoleAsync(migrationDataSource);

        var runtimeConnectionString = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Username = RuntimeRole,
            Password = RuntimePassword
        }.ConnectionString;

        await using var runtimeDataSource = NpgsqlDataSource.Create(runtimeConnectionString);
        await using var runtimeConnection = await runtimeDataSource.OpenConnectionAsync();

        Assert.AreEqual(0L, await ScalarInt64Async(
            runtimeConnection,
            "SELECT count(*) FROM kernel.tenants;"));

        await using (var transaction = await runtimeConnection.BeginTransactionAsync())
        {
            await PostgresTenantSession.SetTenantAsync(runtimeConnection, transaction, tenantA);

            await using var scopedCount = new NpgsqlCommand(
                "SELECT count(*) FROM kernel.tenants;",
                runtimeConnection,
                transaction);
            Assert.AreEqual(1L, Convert.ToInt64(await scopedCount.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture));

            await using var crossTenantWrite = new NpgsqlCommand(
                "INSERT INTO kernel.tenants (tenant_id, tenant_slug) VALUES (@tenant_id, @tenant_slug);",
                runtimeConnection,
                transaction);
            crossTenantWrite.Parameters.AddWithValue("tenant_id", TenantId.New().Value);
            crossTenantWrite.Parameters.AddWithValue("tenant_slug", "forbidden-tenant");

            var exception = await Assert.ThrowsExactlyAsync<PostgresException>(() => crossTenantWrite.ExecuteNonQueryAsync());
            Assert.AreEqual(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);

            await transaction.RollbackAsync();
        }

        await using var forbiddenDdl = new NpgsqlCommand(
            "CREATE TABLE kernel.runtime_forbidden (id integer);",
            runtimeConnection);
        var ddlException = await Assert.ThrowsExactlyAsync<PostgresException>(() => forbiddenDdl.ExecuteNonQueryAsync());
        Assert.AreEqual(PostgresErrorCodes.InsufficientPrivilege, ddlException.SqlState);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();

        await ExecuteAsync(connection, "DROP SCHEMA IF EXISTS kernel CASCADE; DROP SCHEMA IF EXISTS nublox_meta CASCADE;");
        await ExecuteAsync(connection, $"DROP OWNED BY {RuntimeRole}; DROP ROLE IF EXISTS {RuntimeRole};", ignoreUndefinedObject: true);
    }

    private static async Task InsertTenantAsync(NpgsqlDataSource dataSource, TenantId tenantId, string tenantSlug)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId);

        await using var command = new NpgsqlCommand(
            "INSERT INTO kernel.tenants (tenant_id, tenant_slug) VALUES (@tenant_id, @tenant_slug);",
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("tenant_slug", tenantSlug);
        await command.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
    }

    private static async Task CreateRestrictedRuntimeRoleAsync(NpgsqlDataSource dataSource)
    {
        await using var connection = await dataSource.OpenConnectionAsync();

        await ExecuteAsync(connection, $"CREATE ROLE {RuntimeRole} LOGIN PASSWORD '{RuntimePassword}' NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION;");
        await ExecuteAsync(connection, $"GRANT USAGE ON SCHEMA kernel TO {RuntimeRole};");
        await ExecuteAsync(connection, $"GRANT SELECT, INSERT, UPDATE, DELETE ON kernel.tenants TO {RuntimeRole};");
    }

    private static async Task<long> ScalarInt64Async(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        return Convert.ToInt64(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        string sql,
        bool ignoreUndefinedObject = false)
    {
        try
        {
            await using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }
        catch (PostgresException exception) when (ignoreUndefinedObject && exception.SqlState == PostgresErrorCodes.UndefinedObject)
        {
        }
    }
}
