using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;
using NuBlox.WorkProducts.Infrastructure.PostgreSql;

namespace NuBlox.WorkProducts.PostgreSql.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class ReviewDecisionPersistenceTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_decision_runtime_test";
    private const string RuntimePassword = "nublox_decision_runtime_ci_only";
    private string _connectionString = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            Assert.Inconclusive($"Set {ConnectionStringVariable} to run PostgreSQL integration tests.");
        }
        await ResetAsync();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (!string.IsNullOrWhiteSpace(_connectionString)) await ResetAsync();
    }

    [TestMethod]
    public async Task AuthorisedDecisionAtomicallyCompletesRequestChangesRevisionAndAppendsEvidence()
    {
        await using var migrationDataSource = NpgsqlDataSource.Create(_connectionString);
        await ApplyMigrationsAsync(migrationDataSource);
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();
        await SeedTenantAsync(migrationDataSource, tenantA, "tenant-a");
        await SeedTenantAsync(migrationDataSource, tenantB, "tenant-b");
        await CreateRuntimeRoleAsync(migrationDataSource);

        var runtimeConnectionString = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Username = RuntimeRole,
            Password = RuntimePassword
        }.ConnectionString;
        await using var runtimeDataSource = NpgsqlDataSource.Create(runtimeConnectionString);
        var recordRepository = new PostgresWorkProductRepository(runtimeDataSource);
        var decisionRepository = new PostgresWorkProductDecisionRepository(runtimeDataSource);
        var contributor = PrincipalId.New();
        var reviewer = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);
        var service = new WorkProductApplicationService(
            recordRepository,
            decisionRepository,
            new AllowSubmissionAccessEvaluator(),
            new AllowDecisionAuthorityEvaluator(),
            new FixedClock(createdAt));

        var created = await service.CreateAsync(new CreateWorkProductCommand(
            tenantA, contributor, "Configuration Management Plan", "governed-document"));
        service = CreateService(recordRepository, decisionRepository, createdAt.AddMinutes(10));
        var submission = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenantA, created.WorkProduct.Id, contributor, reviewer));
        service = CreateService(recordRepository, decisionRepository, createdAt.AddMinutes(20));
        var decision = await service.RecordReviewDecisionAsync(new RecordReviewDecisionCommand(
            tenantA,
            created.WorkProduct.Id,
            submission.ReviewRequest.Id,
            reviewer,
            ReviewDecisionOutcome.Approved));

        var found = await service.FindAsync(tenantA, created.WorkProduct.Id);
        Assert.IsNotNull(found);
        Assert.AreEqual(WorkProductRevisionState.Approved, found.CurrentRevision.State);
        Assert.AreEqual(ReviewDecisionOutcome.Approved, decision.Decision.Outcome);

        await using var connection = await runtimeDataSource.OpenConnectionAsync();
        await using var tenantATransaction = await connection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(connection, tenantATransaction, tenantA.Value);
        Assert.AreEqual(1L, await ScalarAsync(connection, tenantATransaction,
            "SELECT count(*) FROM work_products.review_decisions WHERE review_decision_id = @id;",
            "id", decision.Decision.Id.Value));
        Assert.AreEqual(1L, await ScalarAsync(connection, tenantATransaction,
            "SELECT count(*) FROM work_products.review_requests WHERE review_request_id = @id AND state = 'COMPLETED';",
            "id", submission.ReviewRequest.Id.Value));
        await tenantATransaction.RollbackAsync();

        await using var tenantBTransaction = await connection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(connection, tenantBTransaction, tenantB.Value);
        Assert.AreEqual(0L, await ScalarAsync(connection, tenantBTransaction,
            "SELECT count(*) FROM work_products.review_decisions WHERE review_decision_id = @id;",
            "id", decision.Decision.Id.Value));
        await tenantBTransaction.RollbackAsync();
    }

    private static WorkProductApplicationService CreateService(
        IWorkProductRepository recordRepository,
        IWorkProductDecisionRepository decisionRepository,
        DateTimeOffset timestamp) =>
        new(
            recordRepository,
            decisionRepository,
            new AllowSubmissionAccessEvaluator(),
            new AllowDecisionAuthorityEvaluator(),
            new FixedClock(timestamp));

    private static async Task ApplyMigrationsAsync(NpgsqlDataSource dataSource)
    {
        await new PostgresMigrationRunner(dataSource).ApplyAsync();
        await new PostgresMigrationRunner(dataSource, WorkProductsPostgresMigrations.Load()).ApplyAsync();
    }

    private static async Task SeedTenantAsync(NpgsqlDataSource dataSource, TenantId tenantId, string slug)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value);
        await using var command = new NpgsqlCommand(
            "INSERT INTO kernel.tenants (tenant_id, tenant_slug) VALUES (@tenant_id, @slug);",
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("slug", slug);
        await command.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
    }

    private static async Task CreateRuntimeRoleAsync(NpgsqlDataSource dataSource)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await ExecuteAsync(connection, $"CREATE ROLE {RuntimeRole} LOGIN PASSWORD '{RuntimePassword}' NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION;");
        await ExecuteAsync(connection, $"GRANT USAGE ON SCHEMA work_products TO {RuntimeRole};");
        await ExecuteAsync(connection, $"GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA work_products TO {RuntimeRole};");
    }

    private async Task ResetAsync()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await using var connection = await dataSource.OpenConnectionAsync();
        await ExecuteAsync(connection, "DROP SCHEMA IF EXISTS work_products CASCADE;");
        await ExecuteAsync(connection, "DROP SCHEMA IF EXISTS kernel CASCADE;");
        await ExecuteAsync(connection, "DROP SCHEMA IF EXISTS nublox_meta CASCADE;");
        await ExecuteAsync(connection, $"DROP ROLE IF EXISTS {RuntimeRole};");
    }

    private static async Task<long> ScalarAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string sql,
        string parameterName,
        Guid parameterValue)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue(parameterName, parameterValue);
        return Convert.ToInt64(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed class FixedClock(DateTimeOffset timestamp) : ISystemClock
    {
        public DateTimeOffset UtcNow => timestamp;
    }

    private sealed class AllowSubmissionAccessEvaluator : IWorkProductAccessEvaluator
    {
        public ValueTask<bool> CanSubmitForReviewAsync(WorkProductRecord record, PrincipalId actorPrincipalId, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);
    }

    private sealed class AllowDecisionAuthorityEvaluator : IWorkProductDecisionAuthorityEvaluator
    {
        public ValueTask<bool> HasDecisionAuthorityAsync(
            WorkProductRecord record,
            ReviewRequestAssignment assignment,
            PrincipalId actorPrincipalId,
            ReviewDecisionOutcome outcome,
            CancellationToken cancellationToken = default) => ValueTask.FromResult(true);
    }
}
