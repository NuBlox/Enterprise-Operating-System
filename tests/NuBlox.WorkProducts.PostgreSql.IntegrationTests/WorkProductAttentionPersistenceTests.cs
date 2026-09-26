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
    public async Task AttentionIsPrincipalScopedAndDrillsToGovernedSource()
    {
        await using var migrationDataSource = NpgsqlDataSource.Create(_connectionString);
        await ApplyMigrationsAsync(migrationDataSource);
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();
        await SeedTenantAsync(migrationDataSource, tenantA, "tenant-a");
        await SeedTenantAsync(migrationDataSource, tenantB, "tenant-b");
        await CreateRuntimeRoleAsync(migrationDataSource);

        await using var runtimeDataSource = NpgsqlDataSource.Create(RuntimeConnectionString());
        var repository = new PostgresGovernedWorkProductRepository(runtimeDataSource);
        var contributor = PrincipalId.New();
        var reviewer = PrincipalId.New();
        var stranger = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 21, 0, 0, TimeSpan.Zero);
        var access = new AllowAllWorkProductAccessEvaluator();
        var service = new WorkProductApplicationService(repository, access, new FixedClock(createdAt));

        var draft = await service.CreateAsync(new CreateWorkProductCommand(
            tenantA,
            contributor,
            "Draft Design Note",
            "governed-document"));
        var review = await service.CreateAsync(new CreateWorkProductCommand(
            tenantA,
            contributor,
            "Review Design Note",
            "governed-document"));

        service = new WorkProductApplicationService(repository, access, new FixedClock(createdAt.AddMinutes(10)));
        var submission = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenantA,
            review.WorkProduct.Id,
            contributor,
            reviewer));

        var attention = new PostgresWorkProductAttentionReader(runtimeDataSource);
        var contributorItems = await attention.ListAsync(tenantA, contributor);
        var reviewerItems = await attention.ListAsync(tenantA, reviewer);
        var tenantBItems = await attention.ListAsync(tenantB, reviewer);

        Assert.AreEqual(1, contributorItems.Count);
        Assert.AreEqual(draft.WorkProduct.Id, contributorItems[0].Source.WorkProductId);
        Assert.AreEqual(WorkProductAttentionKind.ContributorAction, contributorItems[0].Kind);
        Assert.AreEqual(WorkProductRevisionState.Draft, contributorItems[0].RevisionState);

        Assert.AreEqual(1, reviewerItems.Count);
        Assert.AreEqual(review.WorkProduct.Id, reviewerItems[0].Source.WorkProductId);
        Assert.AreEqual(submission.ReviewRequest.Id, reviewerItems[0].Source.ReviewRequestId);
        Assert.AreEqual(WorkProductAttentionKind.ReviewAction, reviewerItems[0].Kind);
        Assert.AreEqual(WorkProductRevisionState.InReview, reviewerItems[0].RevisionState);
        Assert.AreEqual(0, tenantBItems.Count);

        var reviewerSource = await attention.FindSourceAsync(tenantA, reviewer, review.WorkProduct.Id);
        var strangerSource = await attention.FindSourceAsync(tenantA, stranger, review.WorkProduct.Id);
        var crossTenantSource = await attention.FindSourceAsync(tenantB, reviewer, review.WorkProduct.Id);

        Assert.IsNotNull(reviewerSource);
        Assert.AreEqual(review.WorkProduct.Id, reviewerSource.Record.WorkProduct.Id);
        Assert.AreEqual(1, reviewerSource.ReviewEvidence.Count);
        Assert.AreEqual(submission.ReviewRequest.Id, reviewerSource.ReviewEvidence[0].ReviewRequestId);
        Assert.AreEqual(ReviewRequestState.Open, reviewerSource.ReviewEvidence[0].State);
        Assert.IsNull(reviewerSource.IssueEvidence);
        Assert.IsNull(strangerSource);
        Assert.IsNull(crossTenantSource);
    }

    [TestMethod]
    public async Task DrillThroughIncludesDecisionAndIssueEvidence()
    {
        await using var migrationDataSource = NpgsqlDataSource.Create(_connectionString);
        await ApplyMigrationsAsync(migrationDataSource);
        var tenant = TenantId.New();
        await SeedTenantAsync(migrationDataSource, tenant, "tenant-a");
        await CreateRuntimeRoleAsync(migrationDataSource);

        await using var runtimeDataSource = NpgsqlDataSource.Create(RuntimeConnectionString());
        var repository = new PostgresGovernedWorkProductRepository(runtimeDataSource);
        var contributor = PrincipalId.New();
        var approver = PrincipalId.New();
        var issuer = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 21, 30, 0, TimeSpan.Zero);
        var access = new AllowAllWorkProductAccessEvaluator();
        var authority = new GrantAuthorityEvaluator("delegation:issue-board");
        var service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt));
        var created = await service.CreateAsync(new CreateWorkProductCommand(
            tenant,
            contributor,
            "Issued Design Note",
            "governed-document"));

        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(10)));
        var submission = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenant,
            created.WorkProduct.Id,
            contributor,
            approver));
        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(20)));
        var decision = await service.DecideReviewAsync(new DecideWorkProductReviewCommand(
            tenant,
            created.WorkProduct.Id,
            submission.ReviewRequest.Id,
            approver,
            ReviewDecisionOutcome.Approved,
            "Approved for issue."));

        var issueService = new WorkProductIssueService(
            new PostgresWorkProductIssueRepository(runtimeDataSource),
            new AllowIssueAccessEvaluator(),
            new FixedClock(createdAt.AddMinutes(30)));
        var issue = await issueService.IssueAsync(new IssueWorkProductRevisionCommand(
            tenant,
            created.WorkProduct.Id,
            issuer));

        var attention = new PostgresWorkProductAttentionReader(runtimeDataSource);
        var source = await attention.FindSourceAsync(tenant, contributor, created.WorkProduct.Id);

        Assert.IsNotNull(source);
        Assert.AreEqual(WorkProductRevisionState.Issued, source.Record.CurrentRevision.State);
        Assert.AreEqual(1, source.ReviewEvidence.Count);
        Assert.AreEqual(decision.Decision.Id, source.ReviewEvidence[0].DecisionEvidenceId);
        Assert.AreEqual(ReviewDecisionOutcome.Approved, source.ReviewEvidence[0].Outcome);
        Assert.AreEqual(ReviewRequestState.Completed, source.ReviewEvidence[0].State);
        Assert.IsNotNull(source.IssueEvidence);
        Assert.AreEqual(issue.Evidence.Id, source.IssueEvidence.IssueEvidenceId);
        Assert.AreEqual(decision.Decision.Id, source.IssueEvidence.ApprovalDecisionEvidenceId);
        Assert.AreEqual(issuer, source.IssueEvidence.IssuedByPrincipalId);
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
