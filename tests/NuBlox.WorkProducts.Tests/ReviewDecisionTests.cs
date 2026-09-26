using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Kernel;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Tests;

[TestClass]
public sealed class ReviewDecisionTests
{
    [TestMethod]
    public async Task AssignedAuthorisedPrincipalCanApproveInReviewRevision()
    {
        var fixture = CreateFixture(authorityGranted: true);

        var result = await fixture.Service.RecordReviewDecisionAsync(new RecordReviewDecisionCommand(
            fixture.TenantId,
            fixture.Record.WorkProduct.Id,
            fixture.Assignment.ReviewRequestId,
            fixture.ReviewerPrincipalId,
            ReviewDecisionOutcome.Approved));

        Assert.AreEqual(WorkProductRevisionState.Approved, result.Revision.State);
        Assert.AreEqual(ReviewDecisionOutcome.Approved, result.Decision.Outcome);
        Assert.AreEqual(fixture.ReviewerPrincipalId, result.Decision.DecidedByPrincipalId);
        Assert.AreSame(result, fixture.DecisionRepository.Applied);
    }

    [TestMethod]
    public async Task AssignedPrincipalWithoutBusinessAuthorityCannotApprove()
    {
        var fixture = CreateFixture(authorityGranted: false);

        await Assert.ThrowsAsync<WorkProductAuthorityDeniedException>(() =>
            fixture.Service.RecordReviewDecisionAsync(new RecordReviewDecisionCommand(
                fixture.TenantId,
                fixture.Record.WorkProduct.Id,
                fixture.Assignment.ReviewRequestId,
                fixture.ReviewerPrincipalId,
                ReviewDecisionOutcome.Approved)));

        Assert.IsNull(fixture.DecisionRepository.Applied);
    }

    [TestMethod]
    public async Task DifferentPrincipalCannotUseAnotherReviewersAssignment()
    {
        var fixture = CreateFixture(authorityGranted: true);

        await Assert.ThrowsAsync<WorkProductAccessDeniedException>(() =>
            fixture.Service.RecordReviewDecisionAsync(new RecordReviewDecisionCommand(
                fixture.TenantId,
                fixture.Record.WorkProduct.Id,
                fixture.Assignment.ReviewRequestId,
                PrincipalId.New(),
                ReviewDecisionOutcome.Approved)));

        Assert.IsNull(fixture.DecisionRepository.Applied);
    }

    [TestMethod]
    public async Task ChangesRequiredDecisionRequiresRationaleAndPreservesSubmissionEvidence()
    {
        var fixture = CreateFixture(authorityGranted: true);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            fixture.Service.RecordReviewDecisionAsync(new RecordReviewDecisionCommand(
                fixture.TenantId,
                fixture.Record.WorkProduct.Id,
                fixture.Assignment.ReviewRequestId,
                fixture.ReviewerPrincipalId,
                ReviewDecisionOutcome.ChangesRequired)));

        var result = await fixture.Service.RecordReviewDecisionAsync(new RecordReviewDecisionCommand(
            fixture.TenantId,
            fixture.Record.WorkProduct.Id,
            fixture.Assignment.ReviewRequestId,
            fixture.ReviewerPrincipalId,
            ReviewDecisionOutcome.ChangesRequired,
            "Update the verification evidence."));

        Assert.AreEqual(WorkProductRevisionState.ChangesRequired, result.Revision.State);
        Assert.AreEqual(fixture.Record.CurrentRevision.SubmittedAtUtc, result.Revision.SubmittedAtUtc);
        Assert.AreEqual("Update the verification evidence.", result.Decision.Rationale);
    }

    private static Fixture CreateFixture(bool authorityGranted)
    {
        var tenantId = TenantId.New();
        var contributor = PrincipalId.New();
        var reviewer = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);
        var decidedAt = createdAt.AddHours(1);
        var product = WorkProduct.Create(tenantId, "Design Basis", "governed-document", contributor, contributor, createdAt);
        var revision = WorkProductRevision.CreateInitial(product).SubmitForReview(contributor, createdAt.AddMinutes(10));
        var record = new WorkProductRecord(product, revision);
        var assignment = new ReviewRequestAssignment(ReviewRequestId.New(), tenantId, revision.Id, reviewer);
        var recordRepository = new FixedRecordRepository(record);
        var decisionRepository = new FixedDecisionRepository(assignment);
        var service = new WorkProductApplicationService(
            recordRepository,
            decisionRepository,
            new AllowSubmissionAccessEvaluator(),
            new FixedAuthorityEvaluator(authorityGranted),
            new FixedClock(decidedAt));
        return new Fixture(tenantId, reviewer, record, assignment, decisionRepository, service);
    }

    private sealed record Fixture(
        TenantId TenantId,
        PrincipalId ReviewerPrincipalId,
        WorkProductRecord Record,
        ReviewRequestAssignment Assignment,
        FixedDecisionRepository DecisionRepository,
        WorkProductApplicationService Service);

    private sealed class FixedRecordRepository(WorkProductRecord record) : IWorkProductRepository
    {
        public Task AddAsync(WorkProductRecord value, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SubmitForReviewAsync(ReviewSubmission submission, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<WorkProductRecord?> FindAsync(TenantId tenantId, WorkProductId workProductId, CancellationToken cancellationToken = default) =>
            Task.FromResult<WorkProductRecord?>(tenantId == record.WorkProduct.TenantId && workProductId == record.WorkProduct.Id ? record : null);
    }

    private sealed class FixedDecisionRepository(ReviewRequestAssignment assignment) : IWorkProductDecisionRepository
    {
        public ReviewDecisionResult? Applied { get; private set; }
        public Task<ReviewRequestAssignment?> FindOpenReviewRequestAsync(TenantId tenantId, ReviewRequestId reviewRequestId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ReviewRequestAssignment?>(tenantId == assignment.TenantId && reviewRequestId == assignment.ReviewRequestId ? assignment : null);
        public Task ApplyReviewDecisionAsync(ReviewDecisionResult result, CancellationToken cancellationToken = default)
        {
            Applied = result;
            return Task.CompletedTask;
        }
    }

    private sealed class AllowSubmissionAccessEvaluator : IWorkProductAccessEvaluator
    {
        public ValueTask<bool> CanSubmitForReviewAsync(WorkProductRecord record, PrincipalId actorPrincipalId, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);
    }

    private sealed class FixedAuthorityEvaluator(bool granted) : IWorkProductDecisionAuthorityEvaluator
    {
        public ValueTask<bool> HasDecisionAuthorityAsync(
            WorkProductRecord record,
            ReviewRequestAssignment assignment,
            PrincipalId actorPrincipalId,
            ReviewDecisionOutcome outcome,
            CancellationToken cancellationToken = default) => ValueTask.FromResult(granted);
    }

    private sealed class FixedClock(DateTimeOffset timestamp) : ISystemClock
    {
        public DateTimeOffset UtcNow => timestamp;
    }
}
