using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using NuBlox.Authority;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;
using NuBlox.WorkProducts.Infrastructure.PostgreSql;

namespace NuBlox.WorkProducts.PostgreSql.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class WorkProductDecisionPersistenceTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_work_products_decision_runtime_test";
    private const string RuntimePassword = "nublox_work_products_decision_ci_only";
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
    public async Task ApprovedDecisionIsAtomicTenantScopedAndAuthorityAttributed()
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
        var repository = new PostgresGovernedWorkProductRepository(runtimeDataSource);
        var access = new AllowAllWorkProductAccessEvaluator();
        var authority = new GrantAuthorityEvaluator("delegation:configuration-control-board");
        var contributor = PrincipalId.New();
        var approver = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);

        var service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt));
        var created = await service.CreateAsync(new CreateWorkProductCommand(
            tenantA, contributor, "Configuration Management Plan", "governed-document"));
        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(10)));
        var submission = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenantA, created.WorkProduct.Id, contributor, approver));
        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(20)));
        var decision = await service.DecideReviewAsync(new DecideWorkProductReviewCommand(
            tenantA,
            created.WorkProduct.Id,
            submission.ReviewRequest.Id,
            approver,
            ReviewDecisionOutcome.Approved,
            "Approved for controlled issue.",
            "corr-decision-001"));

        Assert.AreEqual(WorkProductRevisionState.Approved, decision.Revision.State);
        var tenantARecord = await service.FindAsync(tenantA, created.WorkProduct.Id);
        var tenantBRecord = await service.FindAsync(tenantB, created.WorkProduct.Id);
        Assert.AreEqual(WorkProductRevisionState.Approved, tenantARecord?.CurrentRevision.State);
        Assert.IsNull(tenantBRecord);

        await using var runtimeConnection = await runtimeDataSource.OpenConnectionAsync();
        await using var tenantATransaction = await runtimeConnection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(runtimeConnection, tenantATransaction, tenantA.Value);
        await using var evidence = new NpgsqlCommand(
            "SELECT outcome, authority_reference, rationale, correlation_id FROM work_products.review_decisions WHERE decision_evidence_id = @decision_id;",
            runtimeConnection,
            tenantATransaction);
        evidence.Parameters.AddWithValue("decision_id", decision.Decision.Id.Value);
        await using var reader = await evidence.ExecuteReaderAsync();
        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual("APPROVED", reader.GetString(0));
        Assert.AreEqual("delegation:configuration-control-board", reader.GetString(1));
        Assert.AreEqual("Approved for controlled issue.", reader.GetString(2));
        Assert.AreEqual("corr-decision-001", reader.GetString(3));
        await reader.CloseAsync();

        await using var requestState = new NpgsqlCommand(
            "SELECT state FROM work_products.review_requests WHERE review_request_id = @request_id;",
            runtimeConnection,
            tenantATransaction);
        requestState.Parameters.AddWithValue("request_id", submission.ReviewRequest.Id.Value);
        Assert.AreEqual("COMPLETED", Convert.ToString(await requestState.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture));
        await tenantATransaction.RollbackAsync();

        await using var tenantBTransaction = await runtimeConnection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(runtimeConnection, tenantBTransaction, tenantB.Value);
        await using var hidden = new NpgsqlCommand(
            "SELECT count(*) FROM work_products.review_decisions WHERE decision_evidence_id = @decision_id;",
            runtimeConnection,
            tenantBTransaction);
        hidden.Parameters.AddWithValue("decision_id", decision.Decision.Id.Value);
        Assert.AreEqual(0L, Convert.ToInt64(await hidden.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture));
        await tenantBTransaction.RollbackAsync();
    }

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

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed class FixedClock(DateTimeOffset timestamp) : ISystemClock
    {
        public DateTimeOffset UtcNow => timestamp;
    }

    private sealed class AllowAllWorkProductAccessEvaluator : IWorkProductAccessEvaluator
    {
        public ValueTask<bool> CanSubmitForReviewAsync(WorkProductRecord record, PrincipalId actorPrincipalId, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);

        public ValueTask<bool> CanDecideReviewAsync(ReviewDecisionContext context, PrincipalId actorPrincipalId, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);
    }

    private sealed class GrantAuthorityEvaluator(string authorityReference) : IBusinessAuthorityEvaluator
    {
        public ValueTask<AuthorityEvaluationResult> EvaluateAsync(AuthorityEvaluationContext context, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(AuthorityEvaluationResult.Granted(authorityReference));
    }
}
