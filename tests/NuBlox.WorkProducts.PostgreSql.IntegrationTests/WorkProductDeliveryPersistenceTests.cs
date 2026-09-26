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
public sealed class WorkProductDeliveryPersistenceTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_work_products_delivery_runtime_test";
    private const string RuntimePassword = "nublox_work_products_delivery_ci_only";
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
    public async Task IssueCommitsTenantScopedDurableIntent()
    {
        await using var migrationDataSource = NpgsqlDataSource.Create(_connectionString);
        await ApplyMigrationsAsync(migrationDataSource);
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();
        await SeedTenantAsync(migrationDataSource, tenantA, "tenant-a");
        await SeedTenantAsync(migrationDataSource, tenantB, "tenant-b");
        await CreateRuntimeRoleAsync(migrationDataSource);

        await using var runtimeDataSource = NpgsqlDataSource.Create(RuntimeConnectionString());
        var issued = await CreateIssuedWorkProductAsync(runtimeDataSource, tenantA);
        var store = new PostgresWorkProductDeliveryStore(runtimeDataSource);

        Assert.IsNotNull(issued.DeliveryIntent);
        var tenantAStatus = await store.FindAsync(tenantA, issued.DeliveryIntent.Id);
        var tenantBStatus = await store.FindAsync(tenantB, issued.DeliveryIntent.Id);

        Assert.IsNotNull(tenantAStatus);
        Assert.AreEqual(WorkProductDeliveryState.Pending, tenantAStatus.State);
        Assert.AreEqual(0, tenantAStatus.AttemptCount);
        Assert.AreEqual(issued.Evidence.Id, tenantAStatus.Intent.IssueEvidenceId);
        Assert.AreEqual(issued.Evidence.CorrelationId, tenantAStatus.Intent.CorrelationId);
        Assert.IsNull(tenantBStatus);
    }

    [TestMethod]
    public async Task ExpiredClaimRecoversRetryPreservesIdentityAndCompletionReconciles()
    {
        await using var migrationDataSource = NpgsqlDataSource.Create(_connectionString);
        await ApplyMigrationsAsync(migrationDataSource);
        var tenant = TenantId.New();
        await SeedTenantAsync(migrationDataSource, tenant, "tenant-a");
        await CreateRuntimeRoleAsync(migrationDataSource);

        await using var runtimeDataSource = NpgsqlDataSource.Create(RuntimeConnectionString());
        var issued = await CreateIssuedWorkProductAsync(runtimeDataSource, tenant);
        var intent = issued.DeliveryIntent ?? throw new InvalidOperationException("Expected issue delivery intent.");
        var store = new PostgresWorkProductDeliveryStore(runtimeDataSource);
        var t0 = intent.CreatedAtUtc.AddMinutes(1);

        var first = await store.ClaimNextAsync(tenant, "worker-a", t0, TimeSpan.FromMinutes(5));
        Assert.IsNotNull(first);
        Assert.AreEqual(1, first.AttemptNumber);
        Assert.AreEqual(intent.Id, first.Intent.Id);
        Assert.AreEqual(intent.IdempotencyKey, first.Intent.IdempotencyKey);

        var beforeExpiry = await store.ClaimNextAsync(tenant, "worker-b", t0.AddMinutes(1), TimeSpan.FromMinutes(5));
        Assert.IsNull(beforeExpiry);

        var recovered = await store.ClaimNextAsync(tenant, "worker-b", t0.AddMinutes(6), TimeSpan.FromMinutes(5));
        Assert.IsNotNull(recovered);
        Assert.AreEqual(intent.Id, recovered.Intent.Id);
        Assert.AreEqual(intent.IdempotencyKey, recovered.Intent.IdempotencyKey);
        Assert.AreEqual(2, recovered.AttemptNumber);

        var retryAt = t0.AddMinutes(10);
        await store.RecordRetryAsync(tenant, intent.Id, "worker-b", retryAt, "provider-timeout");
        var retryState = await store.FindAsync(tenant, intent.Id);
        Assert.IsNotNull(retryState);
        Assert.AreEqual(WorkProductDeliveryState.Pending, retryState.State);
        Assert.AreEqual(2, retryState.AttemptCount);
        Assert.AreEqual("provider-timeout", retryState.LastFailureCode);
        Assert.AreEqual(retryAt, retryState.NextAttemptAtUtc);

        Assert.IsNull(await store.ClaimNextAsync(tenant, "worker-c", retryAt.AddSeconds(-1), TimeSpan.FromMinutes(5)));
        var retryClaim = await store.ClaimNextAsync(tenant, "worker-c", retryAt, TimeSpan.FromMinutes(5));
        Assert.IsNotNull(retryClaim);
        Assert.AreEqual(3, retryClaim.AttemptNumber);
        Assert.AreEqual(intent.IdempotencyKey, retryClaim.Intent.IdempotencyKey);

        var completedAt = retryAt.AddSeconds(20);
        await store.RecordCompletedAsync(tenant, intent.Id, "worker-c", completedAt);
        var completed = await store.FindAsync(tenant, intent.Id);
        Assert.IsNotNull(completed);
        Assert.AreEqual(WorkProductDeliveryState.Completed, completed.State);
        Assert.AreEqual(3, completed.AttemptCount);
        Assert.AreEqual(completedAt, completed.CompletedAtUtc);
        Assert.IsNull(completed.ClaimedBy);
        Assert.IsNull(completed.ClaimExpiresAtUtc);
        Assert.IsNull(completed.LastFailureCode);
    }

    [TestMethod]
    public async Task ConcurrentWorkersClaimIntentOnce()
    {
        await using var migrationDataSource = NpgsqlDataSource.Create(_connectionString);
        await ApplyMigrationsAsync(migrationDataSource);
        var tenant = TenantId.New();
        await SeedTenantAsync(migrationDataSource, tenant, "tenant-a");
        await CreateRuntimeRoleAsync(migrationDataSource);

        await using var runtimeDataSource = NpgsqlDataSource.Create(RuntimeConnectionString());
        var issued = await CreateIssuedWorkProductAsync(runtimeDataSource, tenant);
        var intent = issued.DeliveryIntent ?? throw new InvalidOperationException("Expected issue delivery intent.");
        var store = new PostgresWorkProductDeliveryStore(runtimeDataSource);
        var now = intent.CreatedAtUtc.AddMinutes(1);

        var claims = await Task.WhenAll(
            store.ClaimNextAsync(tenant, "worker-a", now, TimeSpan.FromMinutes(5)),
            store.ClaimNextAsync(tenant, "worker-b", now, TimeSpan.FromMinutes(5)));

        Assert.AreEqual(1, claims.Count(claim => claim is not null));
        Assert.AreEqual(intent.Id, claims.Single(claim => claim is not null)!.Intent.Id);
    }

    private async Task<WorkProductIssueResult> CreateIssuedWorkProductAsync(NpgsqlDataSource dataSource, TenantId tenant)
    {
        var repository = new PostgresGovernedWorkProductRepository(dataSource);
        var contributor = PrincipalId.New();
        var approver = PrincipalId.New();
        var issuer = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);
        var access = new AllowAllWorkProductAccessEvaluator();
        var authority = new GrantAuthorityEvaluator("delegation:configuration-control-board");
        var service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt));

        var created = await service.CreateAsync(new CreateWorkProductCommand(
            tenant,
            contributor,
            "Durable Delivery Product",
            "governed-document"));
        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(10)));
        var submission = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenant,
            created.WorkProduct.Id,
            contributor,
            approver));
        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(20)));
        await service.DecideReviewAsync(new DecideWorkProductReviewCommand(
            tenant,
            created.WorkProduct.Id,
            submission.ReviewRequest.Id,
            approver,
            ReviewDecisionOutcome.Approved,
            "Approved for durable consequence verification."));

        var issueService = new WorkProductIssueService(
            new PostgresWorkProductIssueRepository(dataSource),
            new AllowIssueAccessEvaluator(),
            new FixedClock(createdAt.AddMinutes(30)));
        return await issueService.IssueAsync(new IssueWorkProductRevisionCommand(
            tenant,
            created.WorkProduct.Id,
            issuer,
            "corr-durable-issue"));
    }

    private string RuntimeConnectionString() => new NpgsqlConnectionStringBuilder(_connectionString)
    {
        Username = RuntimeRole,
        Password = RuntimePassword
    }.ConnectionString;

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

    private sealed class AllowIssueAccessEvaluator : IWorkProductIssueAccessEvaluator
    {
        public ValueTask<bool> CanIssueAsync(WorkProductIssueContext context, PrincipalId actorPrincipalId, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);
    }

    private sealed class GrantAuthorityEvaluator(string authorityReference) : IBusinessAuthorityEvaluator
    {
        public ValueTask<AuthorityEvaluationResult> EvaluateAsync(AuthorityEvaluationContext context, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(AuthorityEvaluationResult.Granted(authorityReference));
    }
}
