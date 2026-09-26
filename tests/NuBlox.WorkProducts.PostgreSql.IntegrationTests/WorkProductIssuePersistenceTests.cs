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
public sealed class WorkProductIssuePersistenceTests
{
    private const string ConnectionStringVariable = "NUBLOX_POSTGRES_CONNECTION_STRING";
    private const string RuntimeRole = "nublox_work_products_issue_runtime_test";
    private const string RuntimePassword = "nublox_work_products_issue_ci_only";
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
    public async Task ApprovedRevisionIssuesWithLinkedDecisionEvidenceAndTenantIsolation()
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

        var governedRepository = new PostgresGovernedWorkProductRepository(runtimeDataSource);
        var contributor = PrincipalId.New();
        var approver = PrincipalId.New();
        var issuer = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 20, 0, 0, TimeSpan.Zero);
        var access = new AllowAllWorkProductAccessEvaluator();
        var authority = new GrantAuthorityEvaluator("delegation:configuration-control-board");
        var workProducts = new WorkProductApplicationService(governedRepository, access, authority, new FixedClock(createdAt));

        var created = await workProducts.CreateAsync(new CreateWorkProductCommand(
            tenantA,
            contributor,
            "Configuration Management Plan",
            "governed-document"));

        workProducts = new WorkProductApplicationService(
            governedRepository,
            access,
            authority,
            new FixedClock(createdAt.AddMinutes(10)));
        var submission = await workProducts.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenantA,
            created.WorkProduct.Id,
            contributor,
            approver));

        workProducts = new WorkProductApplicationService(
            governedRepository,
            access,
            authority,
            new FixedClock(createdAt.AddMinutes(20)));
        var decision = await workProducts.DecideReviewAsync(new DecideWorkProductReviewCommand(
            tenantA,
            created.WorkProduct.Id,
            submission.ReviewRequest.Id,
            approver,
            ReviewDecisionOutcome.Approved,
            "Approved for controlled issue.",
            "corr-decision-issue"));

        var issueRepository = new PostgresWorkProductIssueRepository(runtimeDataSource);
        var issueService = new WorkProductIssueService(
            issueRepository,
            new AllowIssueAccessEvaluator(),
            new FixedClock(createdAt.AddMinutes(30)));
        var issue = await issueService.IssueAsync(new IssueWorkProductRevisionCommand(
            tenantA,
            created.WorkProduct.Id,
            issuer,
            "corr-issue-001"));

        Assert.AreEqual(WorkProductRevisionState.Issued, issue.Revision.State);
        Assert.AreEqual(issuer, issue.Revision.IssuedByPrincipalId);
        Assert.AreEqual(decision.Decision.Id, issue.Evidence.ApprovalDecisionEvidenceId);

        var tenantARecord = await governedRepository.FindAsync(tenantA, created.WorkProduct.Id);
        var tenantBRecord = await governedRepository.FindAsync(tenantB, created.WorkProduct.Id);
        Assert.AreEqual(WorkProductRevisionState.Issued, tenantARecord?.CurrentRevision.State);
        Assert.AreEqual(issuer, tenantARecord?.CurrentRevision.IssuedByPrincipalId);
        Assert.IsNull(tenantBRecord);

        await using var runtimeConnection = await runtimeDataSource.OpenConnectionAsync();
        await using var tenantATransaction = await runtimeConnection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(runtimeConnection, tenantATransaction, tenantA.Value);
        await using var linkedEvidence = new NpgsqlCommand(
            """
            SELECT approval_decision_evidence_id, approval_outcome, issued_by_principal_id, correlation_id
            FROM work_products.issue_evidence
            WHERE issue_evidence_id = @issue_evidence_id;
            """,
            runtimeConnection,
            tenantATransaction);
        linkedEvidence.Parameters.AddWithValue("issue_evidence_id", issue.Evidence.Id.Value);
        await using var reader = await linkedEvidence.ExecuteReaderAsync();
        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(decision.Decision.Id.Value, reader.GetGuid(0));
        Assert.AreEqual("APPROVED", reader.GetString(1));
        Assert.AreEqual(issuer.Value, reader.GetGuid(2));
        Assert.AreEqual("corr-issue-001", reader.GetString(3));
        await reader.CloseAsync();
        await tenantATransaction.RollbackAsync();

        await using var tenantBTransaction = await runtimeConnection.BeginTransactionAsync();
        await PostgresTenantSession.SetTenantAsync(runtimeConnection, tenantBTransaction, tenantB.Value);
        await using var hidden = new NpgsqlCommand(
            "SELECT count(*) FROM work_products.issue_evidence WHERE issue_evidence_id = @issue_evidence_id;",
            runtimeConnection,
            tenantBTransaction);
        hidden.Parameters.AddWithValue("issue_evidence_id", issue.Evidence.Id.Value);
        Assert.AreEqual(0L, Convert.ToInt64(await hidden.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture));
        await tenantBTransaction.RollbackAsync();
    }

    [TestMethod]
    public async Task DatabaseRejectsIssueEvidenceLinkedToRejectedDecision()
    {
        await using var migrationDataSource = NpgsqlDataSource.Create(_connectionString);
        await ApplyMigrationsAsync(migrationDataSource);
        var tenant = TenantId.New();
        await SeedTenantAsync(migrationDataSource, tenant, "tenant-a");
        await CreateRuntimeRoleAsync(migrationDataSource);

        var runtimeConnectionString = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Username = RuntimeRole,
            Password = RuntimePassword
        }.ConnectionString;
        await using var runtimeDataSource = NpgsqlDataSource.Create(runtimeConnectionString);
        var governedRepository = new PostgresGovernedWorkProductRepository(runtimeDataSource);
        var actor = PrincipalId.New();
        var reviewer = PrincipalId.New();
        var access = new AllowAllWorkProductAccessEvaluator();
        var authority = new GrantAuthorityEvaluator("delegation:review-chair");
        var createdAt = new DateTimeOffset(2026, 9, 26, 20, 0, 0, TimeSpan.Zero);
        var service = new WorkProductApplicationService(governedRepository, access, authority, new FixedClock(createdAt));
        var created = await service.CreateAsync(new CreateWorkProductCommand(tenant, actor, "Rejected Product", "governed-document"));
        service = new WorkProductApplicationService(governedRepository, access, authority, new FixedClock(createdAt.AddMinutes(10)));
        var submission = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(tenant, created.WorkProduct.Id, actor, reviewer));
        service = new WorkProductApplicationService(governedRepository, access, authority, new FixedClock(createdAt.AddMinutes(20)));
        await service.DecideReviewAsync(new DecideWorkProductReviewCommand(
            tenant,
            created.WorkProduct.Id,
            submission.ReviewRequest.Id,
            reviewer,
            ReviewDecisionOutcome.Rejected,
            "Rejected."));

        var issueRepository = new PostgresWorkProductIssueRepository(runtimeDataSource);
        var issueContext = await issueRepository.FindIssueContextAsync(tenant, created.WorkProduct.Id);
        Assert.IsNull(issueContext);
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
