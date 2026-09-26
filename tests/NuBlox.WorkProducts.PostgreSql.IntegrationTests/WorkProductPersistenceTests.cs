using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;
using NuBlox.WorkProducts.Infrastructure.PostgreSql;

namespace NuBlox.WorkProducts.PostgreSql.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class WorkProductPersistenceTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_work_products_runtime_test";
    private const string RuntimePassword = "nublox_work_products_runtime_ci_only";

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
    public async Task ModuleMigrationIsOwnedJournalledAndReplaySafe()
    {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        await ApplyMigrationsAsync(dataSource);
        await ApplyMigrationsAsync(dataSource);
        await using var connection = await dataSource.OpenConnectionAsync();

        Assert.AreEqual(3L, await ScalarInt64Async(connection, "SELECT count(*) FROM nublox_meta.schema_migrations;"));
        Assert.AreEqual(1L, await ScalarInt64Async(connection, "SELECT count(*) FROM pg_namespace WHERE nspname = 'work_products';"));
        Assert.AreEqual(3L, await ScalarInt64Async(connection, "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'work_products';"));
    }

    [TestMethod]
    public async Task RestrictedRepositoryPersistsAndRoutesReviewInsideTenantBoundary()
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
        var repository = new PostgresWorkProductRepository(runtimeDataSource);
        var actor = PrincipalId.New();
        var reviewer = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);
        var submittedAt = createdAt.AddMinutes(30);
        var service = new WorkProductApplicationService(repository, new AllowSubmissionAccessEvaluator(), new FixedClock(createdAt));

        var created = await service.CreateAsync(new CreateWorkProductCommand(
            tenantA, actor, "Configuration Management Plan", "governed-document"));
        service = new WorkProductApplicationService(repository, new AllowSubmissionAccessEvaluator(), new FixedClock(submittedAt));
        var submission = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenantA, created.WorkProduct.Id, actor, reviewer));

        var foundForTenantA = await service.FindAsync(tenantA, created.WorkProduct.Id);
        var foundForTenantB = await service.FindAsync(tenantB, created.WorkProduct.Id);
        Assert.IsNotNull(foundForTenantA);
        Assert.AreEqual(WorkProductRevisionState.InReview, foundForTenantA.CurrentRevision.State);
        Assert.AreEqual(actor, foundForTenantA.CurrentRevision.SubmittedByPrincipalId);
        Assert.AreEqual(submittedAt, foundForTenantA.CurrentRevision.SubmittedAtUtc);
        Assert.IsNull(foundForTenantB);

        await using var runtimeConnection = await runtimeDataSource.OpenConnectionAsync();
        await using var tenantATransaction = await runtimeConnection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(runtimeConnection, tenantATransaction, tenantA.Value);
        await using var routed = new NpgsqlCommand(
            "SELECT count(*) FROM work_products.review_requests WHERE review_request_id = @request AND requested_principal_id = @reviewer AND state = 'OPEN';",
            runtimeConnection,
            tenantATransaction);
        routed.Parameters.AddWithValue("request", submission.ReviewRequest.Id.Value);
        routed.Parameters.AddWithValue("reviewer", reviewer.Value);
        Assert.AreEqual(1L, Convert.ToInt64(await routed.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture));
        await tenantATransaction.RollbackAsync();

        await using var tenantBTransaction = await runtimeConnection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(runtimeConnection, tenantBTransaction, tenantB.Value);
        await using var hidden = new NpgsqlCommand(
            "SELECT count(*) FROM work_products.review_requests WHERE review_request_id = @request;",
            runtimeConnection,
            tenantBTransaction);
        hidden.Parameters.AddWithValue("request", submission.ReviewRequest.Id.Value);
        Assert.AreEqual(0L, Convert.ToInt64(await hidden.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture));
        await tenantBTransaction.RollbackAsync();
    }

    [TestMethod]
    public async Task DatabaseRejectsDuplicateRevisionNumberForSameTenantProduct()
    {
        await using var migrationDataSource = NpgsqlDataSource.Create(_connectionString);
        await ApplyMigrationsAsync(migrationDataSource);
        var tenant = TenantId.New();
        await SeedTenantAsync(migrationDataSource, tenant, "tenant-a");
        var actor = PrincipalId.New();
        var workProduct = WorkProduct.Create(tenant, "Design Basis", "governed-document", actor, actor, DateTimeOffset.UtcNow);
        var revision = WorkProductRevision.CreateInitial(workProduct);

        await using var connection = await migrationDataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenant.Value);
        await using (var productInsert = new NpgsqlCommand(
            "INSERT INTO work_products.work_products (tenant_id, work_product_id, title, product_type, owner_principal_id, created_by_principal_id, created_at, lifecycle, current_revision_number) VALUES (@tenant, @product, @title, @type, @owner, @creator, @created, 'ACTIVE', 1);",
            connection,
            transaction))
        {
            productInsert.Parameters.AddWithValue("tenant", tenant.Value);
            productInsert.Parameters.AddWithValue("product", workProduct.Id.Value);
            productInsert.Parameters.AddWithValue("title", workProduct.Title);
            productInsert.Parameters.AddWithValue("type", workProduct.ProductType);
            productInsert.Parameters.AddWithValue("owner", actor.Value);
            productInsert.Parameters.AddWithValue("creator", actor.Value);
            productInsert.Parameters.AddWithValue("created", workProduct.CreatedAtUtc);
            await productInsert.ExecuteNonQueryAsync();
        }

        await InsertRevisionAsync(connection, transaction, revision.Id.Value, tenant.Value, workProduct.Id.Value, actor.Value, revision.CreatedAtUtc);
        var duplicateException = await Assert.ThrowsAsync<PostgresException>(() => InsertRevisionAsync(
            connection, transaction, Guid.NewGuid(), tenant.Value, workProduct.Id.Value, actor.Value, revision.CreatedAtUtc));
        Assert.AreEqual(PostgresErrorCodes.UniqueViolation, duplicateException.SqlState);
        await transaction.RollbackAsync();
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
        await using var command = new NpgsqlCommand("INSERT INTO kernel.tenants (tenant_id, tenant_slug) VALUES (@tenant_id, @slug);", connection, transaction);
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

    private static async Task InsertRevisionAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid revisionId, Guid tenantId, Guid workProductId, Guid actorId, DateTimeOffset createdAt)
    {
        await using var command = new NpgsqlCommand(
            "INSERT INTO work_products.revisions (tenant_id, work_product_revision_id, work_product_id, revision_number, title_snapshot, state, created_by_principal_id, created_at) VALUES (@tenant, @revision, @product, 1, 'Design Basis', 'DRAFT', @actor, @created);",
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant", tenantId);
        command.Parameters.AddWithValue("revision", revisionId);
        command.Parameters.AddWithValue("product", workProductId);
        command.Parameters.AddWithValue("actor", actorId);
        command.Parameters.AddWithValue("created", createdAt);
        await command.ExecuteNonQueryAsync();
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

    private static async Task<long> ScalarInt64Async(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
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
        public ValueTask<bool> CanSubmitForReviewAsync(WorkProductRecord record, PrincipalId actorPrincipalId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(true);
        }
    }
}
