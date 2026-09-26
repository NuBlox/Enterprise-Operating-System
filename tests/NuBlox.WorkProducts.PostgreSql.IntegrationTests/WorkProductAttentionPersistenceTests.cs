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
public sealed class WorkProductAttentionPersistenceTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_work_products_attention_runtime_test";
    private const string RuntimePassword = "nublox_work_products_attention_ci_only";
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
    public async Task AttentionFollowsAuthoritativeWorkflowAndDrillsThroughToGovernedSource()
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
        var query = new PostgresWorkProductAttentionQuery(runtimeDataSource);
        var attention = new WorkProductAttentionService(query);
        var contributor = PrincipalId.New();
        var reviewer = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 20, 0, 0, TimeSpan.Zero);
        var access = new AllowAllWorkProductAccessEvaluator();
        var authority = new GrantAuthorityEvaluator("delegation:review-board");

        var workflow = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt));
        var created = await workflow.CreateAsync(new CreateWorkProductCommand(
            tenantA,
            contributor,
            "Configuration Management Plan",
            "governed-document"));

        var contributorDraft = await attention.GetMyAttentionAsync(tenantA, contributor);
        Assert.AreEqual(1, contributorDraft.Count);
        Assert.AreEqual(WorkProductAttentionKind.ContributorAction, contributorDraft.Items[0].Kind);
        Assert.AreEqual(WorkProductAttentionReason.DraftRequiresAction, contributorDraft.Items[0].Reason);
        Assert.AreEqual(created.WorkProduct.Id, contributorDraft.Items[0].Source.WorkProductId);
        Assert.AreEqual(created.CurrentRevision.Id, contributorDraft.Items[0].Source.WorkProductRevisionId);
        Assert.IsNull(contributorDraft.Items[0].Source.ReviewRequestId);

        var tenantBContributor = await attention.GetMyAttentionAsync(tenantB, contributor);
        Assert.AreEqual(0, tenantBContributor.Count);

        workflow = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(10)));
        var submission = await workflow.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenantA,
            created.WorkProduct.Id,
            contributor,
            reviewer));

        var contributorAfterSubmit = await attention.GetMyAttentionAsync(tenantA, contributor);
        var reviewerAttention = await attention.GetMyAttentionAsync(tenantA, reviewer);
        Assert.AreEqual(0, contributorAfterSubmit.Count);
        Assert.AreEqual(1, reviewerAttention.Count);
        Assert.AreEqual(WorkProductAttentionKind.ReviewRequest, reviewerAttention.Items[0].Kind);
        Assert.AreEqual(WorkProductAttentionReason.ReviewRequested, reviewerAttention.Items[0].Reason);
        Assert.AreEqual(submission.ReviewRequest.Id, reviewerAttention.Items[0].Source.ReviewRequestId);
        Assert.AreEqual(submission.Revision.Id, reviewerAttention.Items[0].Source.WorkProductRevisionId);

        var drillThrough = await repository.FindAsync(
            tenantA,
            reviewerAttention.Items[0].Source.WorkProductId);
        Assert.IsNotNull(drillThrough);
        Assert.AreEqual(submission.Revision.Id, drillThrough.CurrentRevision.Id);
        Assert.AreEqual(WorkProductRevisionState.InReview, drillThrough.CurrentRevision.State);

        workflow = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(20)));
        await workflow.DecideReviewAsync(new DecideWorkProductReviewCommand(
            tenantA,
            created.WorkProduct.Id,
            submission.ReviewRequest.Id,
            reviewer,
            ReviewDecisionOutcome.Approved,
            "Approved."));

        var reviewerAfterDecision = await attention.GetMyAttentionAsync(tenantA, reviewer);
        Assert.AreEqual(0, reviewerAfterDecision.Count);
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
