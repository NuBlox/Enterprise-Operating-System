using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using NuBlox.Kernel.Audit;
using NuBlox.Kernel.Identity;
using NuBlox.Persistence.Postgres;

namespace NuBlox.Persistence.Postgres.Tests;

[TestClass]
public sealed class PostgresFoundationTests
{
    private const string RuntimeRole = "nublox_runtime_test";

    [TestMethod]
    public async Task MigrationsAuditAndTenantIsolationAreEnforced()
    {
        string connectionString = Environment.GetEnvironmentVariable("NUBLOX_DATABASE_CONNECTION_STRING")
            ?? throw new AssertFailedException("NUBLOX_DATABASE_CONNECTION_STRING is required for PostgreSQL integration tests.");

        var runner = new PostgresMigrationRunner(connectionString);

        IReadOnlyList<MigrationResult> firstRun = await runner.ApplyAsync();
        IReadOnlyList<MigrationResult> secondRun = await runner.ApplyAsync();

        Assert.IsTrue(firstRun.Any(result => result.State == MigrationState.Applied));
        Assert.IsTrue(secondRun.All(result => result.State == MigrationState.AlreadyApplied));

        TenantId tenantA = new(Guid.NewGuid());
        TenantId tenantB = new(Guid.NewGuid());
        PrincipalId principalId = new(Guid.NewGuid());
        Guid auditEventId = Guid.NewGuid();

        var auditAppender = new PostgresAuditEventAppender(connectionString);
        await auditAppender.AppendAsync(new AuditEvent(
            auditEventId,
            tenantA,
            principalId,
            "PLATFORM_FOUNDATION_VERIFIED",
            "PlatformFoundation",
            "production-platform-foundation",
            DateTimeOffset.UtcNow,
            "postgres-foundation-test"));

        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(connectionString);
        await using NpgsqlConnection adminConnection = await dataSource.OpenConnectionAsync();

        await ConfigureRuntimeRoleAsync(adminConnection);

        long journalCount = await ExecuteScalarInt64Async(
            adminConnection,
            "SELECT count(*) FROM nublox_platform.schema_migrations;");
        Assert.AreEqual(1L, journalCount);

        long tenantAVisible = await QueryAsTenantAsync(adminConnection, tenantA);
        long tenantBVisible = await QueryAsTenantAsync(adminConnection, tenantB);

        Assert.AreEqual(1L, tenantAVisible);
        Assert.AreEqual(0L, tenantBVisible);

        await AssertCrossTenantInsertDeniedAsync(adminConnection, tenantA, tenantB, principalId);
        await AssertAuditMutationDeniedAsync(adminConnection, tenantA, auditEventId);
    }

    private static async Task ConfigureRuntimeRoleAsync(NpgsqlConnection connection)
    {
        const string sql = """
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'nublox_runtime_test') THEN
                    CREATE ROLE nublox_runtime_test NOLOGIN;
                END IF;
            END
            $$;

            GRANT USAGE ON SCHEMA nublox_platform TO nublox_runtime_test;
            GRANT SELECT, INSERT ON TABLE nublox_platform.audit_events TO nublox_runtime_test;
            REVOKE UPDATE, DELETE ON TABLE nublox_platform.audit_events FROM nublox_runtime_test;
            """;

        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> QueryAsTenantAsync(
        NpgsqlConnection connection,
        TenantId tenantId)
    {
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
        await SetRuntimeTenantAsync(connection, transaction, tenantId);

        await using NpgsqlCommand command = new(
            "SELECT count(*) FROM nublox_platform.audit_events;",
            connection,
            transaction);

        object? value = await command.ExecuteScalarAsync();
        await transaction.CommitAsync();
        return Convert.ToInt64(value);
    }

    private static async Task AssertCrossTenantInsertDeniedAsync(
        NpgsqlConnection connection,
        TenantId workingTenant,
        TenantId attemptedTenant,
        PrincipalId principalId)
    {
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
        await SetRuntimeTenantAsync(connection, transaction, workingTenant);

        await using NpgsqlCommand command = new(
            """
            INSERT INTO nublox_platform.audit_events
                (audit_event_id, tenant_id, principal_id, event_type, subject_type,
                 subject_id, recorded_at_utc, correlation_id)
            VALUES
                (@audit_event_id, @tenant_id, @principal_id, 'CROSS_TENANT_ATTEMPT',
                 'PlatformFoundation', 'denied', now(), NULL);
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("audit_event_id", Guid.NewGuid());
        command.Parameters.AddWithValue("tenant_id", attemptedTenant.Value);
        command.Parameters.AddWithValue("principal_id", principalId.Value);

        try
        {
            await command.ExecuteNonQueryAsync();
            Assert.Fail("Cross-tenant insert should have been rejected by PostgreSQL row-level security.");
        }
        catch (PostgresException exception)
        {
            Assert.AreEqual("42501", exception.SqlState);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    private static async Task AssertAuditMutationDeniedAsync(
        NpgsqlConnection connection,
        TenantId tenantId,
        Guid auditEventId)
    {
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
        await SetRuntimeTenantAsync(connection, transaction, tenantId);

        await using NpgsqlCommand command = new(
            """
            UPDATE nublox_platform.audit_events
            SET event_type = 'MUTATED'
            WHERE audit_event_id = @audit_event_id;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("audit_event_id", auditEventId);

        try
        {
            await command.ExecuteNonQueryAsync();
            Assert.Fail("Runtime role should not have UPDATE permission on authoritative audit evidence.");
        }
        catch (PostgresException exception)
        {
            Assert.AreEqual("42501", exception.SqlState);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    private static async Task SetRuntimeTenantAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        TenantId tenantId)
    {
        await using NpgsqlCommand roleCommand = new(
            $"SET LOCAL ROLE {RuntimeRole};",
            connection,
            transaction);
        await roleCommand.ExecuteNonQueryAsync();

        await using NpgsqlCommand tenantCommand = new(
            "SELECT set_config('nublox.tenant_id', @tenant_id, true);",
            connection,
            transaction);
        tenantCommand.Parameters.AddWithValue("tenant_id", tenantId.ToString());
        await tenantCommand.ExecuteNonQueryAsync();
    }

    private static async Task<long> ExecuteScalarInt64Async(
        NpgsqlConnection connection,
        string sql)
    {
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync();
        return Convert.ToInt64(value);
    }
}
