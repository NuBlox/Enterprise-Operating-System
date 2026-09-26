using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Kernel;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Tests;

[TestClass]
public sealed class WorkProductIssueTests
{
    [TestMethod]
    public async Task ApprovedRevisionIssuesWithExactApprovalEvidenceLink()
    {
        var tenant = TenantId.New();
        var actor = PrincipalId.New();
        var revision = ApprovedRevision(tenant, actor);
        var approvalDecision = DecisionEvidenceId.New();
        var record = new WorkProductRecord(
            WorkProduct.Restore(
                revision.WorkProductId,
                tenant,
                "Design Basis",
                "governed-document",
                actor,
                actor,
                revision.CreatedAtUtc,
                WorkProductLifecycle.Active,
                revision.RevisionNumber),
            revision);
        var repository = new InMemoryIssueRepository(new WorkProductIssueContext(record, approvalDecision));
        var issuedAt = revision.SubmittedAtUtc!.Value.AddHours(1);
        var service = new WorkProductIssueService(repository, new FixedIssueAccessEvaluator(true), new FixedClock(issuedAt));

        var result = await service.IssueAsync(new IssueWorkProductRevisionCommand(
            tenant,
            revision.WorkProductId,
            actor,
            "corr-issue-001"));

        Assert.AreEqual(WorkProductRevisionState.Issued, result.Revision.State);
        Assert.AreEqual(actor, result.Revision.IssuedByPrincipalId);
        Assert.AreEqual(issuedAt, result.Revision.IssuedAtUtc);
        Assert.AreEqual(approvalDecision, result.Evidence.ApprovalDecisionEvidenceId);
        Assert.AreEqual(revision.Id, result.Evidence.WorkProductRevisionId);
        Assert.AreEqual("corr-issue-001", result.Evidence.CorrelationId);
        Assert.AreSame(result, repository.Recorded);
    }

    [TestMethod]
    public async Task IssueAccessDenialLeavesApprovedRevisionUnchanged()
    {
        var tenant = TenantId.New();
        var actor = PrincipalId.New();
        var revision = ApprovedRevision(tenant, actor);
        var record = CreateRecord(revision, actor);
        var repository = new InMemoryIssueRepository(new WorkProductIssueContext(record, DecisionEvidenceId.New()));
        var service = new WorkProductIssueService(repository, new FixedIssueAccessEvaluator(false), new FixedClock(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<WorkProductIssueAccessDeniedException>(() =>
            service.IssueAsync(new IssueWorkProductRevisionCommand(tenant, revision.WorkProductId, actor)));

        Assert.IsNull(repository.Recorded);
        Assert.AreEqual(WorkProductRevisionState.Approved, repository.Context.Record.CurrentRevision.State);
    }

    [TestMethod]
    public async Task RevisionWithoutApprovedIssueContextCannotBeIssued()
    {
        var repository = new InMemoryIssueRepository(null);
        var service = new WorkProductIssueService(
            repository,
            new FixedIssueAccessEvaluator(true),
            new FixedClock(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.IssueAsync(new IssueWorkProductRevisionCommand(TenantId.New(), WorkProductId.New(), PrincipalId.New())));

        Assert.IsNull(repository.Recorded);
    }

    [TestMethod]
    public void IssuedRevisionCanBeSupersededWithoutRewritingIssueEvidence()
    {
        var tenant = TenantId.New();
        var actor = PrincipalId.New();
        var approved = ApprovedRevision(tenant, actor);
        var issuedAt = approved.SubmittedAtUtc!.Value.AddHours(1);

        var issued = approved.Issue(actor, issuedAt);
        var superseded = issued.Supersede();

        Assert.AreEqual(WorkProductRevisionState.Superseded, superseded.State);
        Assert.AreEqual(actor, superseded.IssuedByPrincipalId);
        Assert.AreEqual(issuedAt, superseded.IssuedAtUtc);
    }

    private static WorkProductRevision ApprovedRevision(TenantId tenant, PrincipalId actor)
    {
        var createdAt = new DateTimeOffset(2026, 9, 26, 20, 0, 0, TimeSpan.Zero);
        var submittedAt = createdAt.AddMinutes(15);
        return WorkProductRevision.Restore(
            WorkProductRevisionId.New(),
            tenant,
            WorkProductId.New(),
            1,
            "Design Basis",
            WorkProductRevisionState.Approved,
            actor,
            createdAt,
            actor,
            submittedAt);
    }

    private static WorkProductRecord CreateRecord(WorkProductRevision revision, PrincipalId actor) =>
        new(
            WorkProduct.Restore(
                revision.WorkProductId,
                revision.TenantId,
                revision.TitleSnapshot,
                "governed-document",
                actor,
                actor,
                revision.CreatedAtUtc,
                WorkProductLifecycle.Active,
                revision.RevisionNumber),
            revision);

    private sealed class FixedClock(DateTimeOffset timestamp) : ISystemClock
    {
        public DateTimeOffset UtcNow => timestamp;
    }

    private sealed class FixedIssueAccessEvaluator(bool allowed) : IWorkProductIssueAccessEvaluator
    {
        public ValueTask<bool> CanIssueAsync(
            WorkProductIssueContext context,
            PrincipalId actorPrincipalId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(allowed);
    }

    private sealed class InMemoryIssueRepository(WorkProductIssueContext? context) : IWorkProductIssueRepository
    {
        public WorkProductIssueContext Context => context ?? throw new InvalidOperationException("No issue context is configured.");
        public WorkProductIssueResult? Recorded { get; private set; }

        public Task<WorkProductIssueContext?> FindIssueContextAsync(
            TenantId tenantId,
            WorkProductId workProductId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                context is not null
                && context.Record.WorkProduct.TenantId == tenantId
                && context.Record.WorkProduct.Id == workProductId
                    ? context
                    : null);

        public Task RecordIssueAsync(WorkProductIssueResult result, CancellationToken cancellationToken = default)
        {
            Recorded = result;
            return Task.CompletedTask;
        }
    }
}
