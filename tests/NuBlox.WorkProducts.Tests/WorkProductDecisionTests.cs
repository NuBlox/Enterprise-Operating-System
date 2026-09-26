using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Authority;
using NuBlox.Kernel;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Tests;

[TestClass]
public sealed class WorkProductDecisionTests
{
    [TestMethod]
    public async Task ApprovedDecisionRequiresAuthorityAndCapturesEvidence()
    {
        var repository = new InMemoryDecisionRepository();
        var tenant = TenantId.New();
        var contributor = PrincipalId.New();
        var approver = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);
        var submittedAt = createdAt.AddMinutes(10);
        var decidedAt = submittedAt.AddMinutes(20);
        var access = new FixedAccessEvaluator(true, true);
        var authority = new FixedAuthorityEvaluator(AuthorityEvaluationResult.Granted("delegation:approval-board-17"));
        var service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt));
        var created = await service.CreateAsync(new CreateWorkProductCommand(tenant, contributor, "Design Basis", "governed-document"));

        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(submittedAt));
        var submission = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenant, created.WorkProduct.Id, contributor, approver));

        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(decidedAt));
        var result = await service.DecideReviewAsync(new DecideWorkProductReviewCommand(
            tenant,
            created.WorkProduct.Id,
            submission.ReviewRequest.Id,
            approver,
            ReviewDecisionOutcome.Approved,
            "Approved for issue.",
            "corr-123"));

        Assert.AreEqual(WorkProductRevisionState.Approved, result.Revision.State);
        Assert.AreEqual(ReviewDecisionOutcome.Approved, result.Decision.Outcome);
        Assert.AreEqual(approver, result.Decision.ActorPrincipalId);
        Assert.AreEqual("delegation:approval-board-17", result.Decision.AuthorityReference);
        Assert.AreEqual("Approved for issue.", result.Decision.Rationale);
        Assert.AreEqual(decidedAt, result.Decision.DecidedAtUtc);
        Assert.AreEqual("corr-123", result.Decision.CorrelationId);
        Assert.AreEqual("work-products.revision.approve", authority.LastContext?.Requirement.ActionCode);
        Assert.AreEqual(submission.Revision.Id.ToString(), authority.LastContext?.Requirement.SubjectId);
        Assert.AreSame(result, repository.Decision);
    }

    [TestMethod]
    public async Task AccessPermissionDoesNotSubstituteForBusinessAuthority()
    {
        var fixture = await CreateSubmittedFixtureAsync(
            allowDecisionAccess: true,
            AuthorityEvaluationResult.Denied("delegation-missing"));

        var exception = await Assert.ThrowsAsync<WorkProductAuthorityDeniedException>(() =>
            fixture.Service.DecideReviewAsync(new DecideWorkProductReviewCommand(
                fixture.Tenant,
                fixture.WorkProductId,
                fixture.ReviewRequestId,
                fixture.Actor,
                ReviewDecisionOutcome.Approved,
                "Attempt approval")));

        Assert.AreEqual("delegation-missing", exception.DenialReasonCode);
        Assert.IsNull(fixture.Repository.Decision);
        Assert.AreEqual(WorkProductRevisionState.InReview, fixture.Repository.Stored!.CurrentRevision.State);
    }

    [TestMethod]
    public async Task TechnicalAccessDenialStopsAuthorityEvaluation()
    {
        var fixture = await CreateSubmittedFixtureAsync(
            allowDecisionAccess: false,
            AuthorityEvaluationResult.Granted("delegation:any"));

        await Assert.ThrowsAsync<WorkProductDecisionAccessDeniedException>(() =>
            fixture.Service.DecideReviewAsync(new DecideWorkProductReviewCommand(
                fixture.Tenant,
                fixture.WorkProductId,
                fixture.ReviewRequestId,
                fixture.Actor,
                ReviewDecisionOutcome.Approved,
                "Attempt approval")));

        Assert.AreEqual(0, fixture.Authority.EvaluationCount);
        Assert.IsNull(fixture.Repository.Decision);
    }

    [TestMethod]
    public async Task RejectionClosesReviewAndTransitionsRevision()
    {
        var fixture = await CreateSubmittedFixtureAsync(
            allowDecisionAccess: true,
            AuthorityEvaluationResult.Granted("delegation:review-chair"));

        var result = await fixture.Service.DecideReviewAsync(new DecideWorkProductReviewCommand(
            fixture.Tenant,
            fixture.WorkProductId,
            fixture.ReviewRequestId,
            fixture.Actor,
            ReviewDecisionOutcome.Rejected,
            "Evidence is incomplete."));

        Assert.AreEqual(WorkProductRevisionState.Rejected, result.Revision.State);
        Assert.AreEqual(ReviewDecisionOutcome.Rejected, result.Decision.Outcome);
        Assert.AreEqual(ReviewRequestState.Completed, fixture.Repository.RequestState);
    }

    private static async Task<SubmittedFixture> CreateSubmittedFixtureAsync(
        bool allowDecisionAccess,
        AuthorityEvaluationResult authorityResult)
    {
        var repository = new InMemoryDecisionRepository();
        var tenant = TenantId.New();
        var contributor = PrincipalId.New();
        var actor = PrincipalId.New();
        var createdAt = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);
        var access = new FixedAccessEvaluator(true, allowDecisionAccess);
        var authority = new FixedAuthorityEvaluator(authorityResult);
        var service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt));
        var created = await service.CreateAsync(new CreateWorkProductCommand(tenant, contributor, "Design Basis", "governed-document"));
        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(10)));
        var submission = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenant, created.WorkProduct.Id, contributor, actor));
        service = new WorkProductApplicationService(repository, access, authority, new FixedClock(createdAt.AddMinutes(20)));

        return new SubmittedFixture(
            repository,
            authority,
            service,
            tenant,
            created.WorkProduct.Id,
            submission.ReviewRequest.Id,
            actor);
    }

    private sealed record SubmittedFixture(
        InMemoryDecisionRepository Repository,
        FixedAuthorityEvaluator Authority,
        WorkProductApplicationService Service,
        TenantId Tenant,
        WorkProductId WorkProductId,
        ReviewRequestId ReviewRequestId,
        PrincipalId Actor);

    private sealed class FixedClock(DateTimeOffset timestamp) : ISystemClock
    {
        public DateTimeOffset UtcNow => timestamp;
    }

    private sealed class FixedAccessEvaluator(bool allowSubmission, bool allowDecision) : IWorkProductAccessEvaluator
    {
        public ValueTask<bool> CanSubmitForReviewAsync(
            WorkProductRecord record,
            PrincipalId actorPrincipalId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(allowSubmission);

        public ValueTask<bool> CanDecideReviewAsync(
            ReviewDecisionContext context,
            PrincipalId actorPrincipalId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(allowDecision);
    }

    private sealed class FixedAuthorityEvaluator(AuthorityEvaluationResult result) : IBusinessAuthorityEvaluator
    {
        public int EvaluationCount { get; private set; }
        public AuthorityEvaluationContext? LastContext { get; private set; }

        public ValueTask<AuthorityEvaluationResult> EvaluateAsync(
            AuthorityEvaluationContext context,
            CancellationToken cancellationToken = default)
        {
            EvaluationCount++;
            LastContext = context;
            return ValueTask.FromResult(result);
        }
    }

    private sealed class InMemoryDecisionRepository : IWorkProductRepository
    {
        public WorkProductRecord? Stored { get; private set; }
        public ReviewRequest? Request { get; private set; }
        public ReviewRequestState RequestState { get; private set; }
        public ReviewDecisionResult? Decision { get; private set; }

        public Task AddAsync(WorkProductRecord record, CancellationToken cancellationToken = default)
        {
            Stored = record;
            return Task.CompletedTask;
        }

        public Task<WorkProductRecord?> FindAsync(
            TenantId tenantId,
            WorkProductId workProductId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Stored is not null && Stored.WorkProduct.TenantId == tenantId && Stored.WorkProduct.Id == workProductId
                    ? Stored
                    : null);

        public Task SubmitForReviewAsync(ReviewSubmission submission, CancellationToken cancellationToken = default)
        {
            if (Stored is null || Stored.CurrentRevision.State != WorkProductRevisionState.Draft)
            {
                throw new WorkProductStateConflictException("The revision is no longer Draft.");
            }

            Stored = new WorkProductRecord(Stored.WorkProduct, submission.Revision);
            Request = submission.ReviewRequest;
            RequestState = ReviewRequestState.Open;
            return Task.CompletedTask;
        }

        public Task<ReviewDecisionContext?> FindReviewDecisionContextAsync(
            TenantId tenantId,
            WorkProductId workProductId,
            ReviewRequestId reviewRequestId,
            CancellationToken cancellationToken = default)
        {
            if (Stored is null || Request is null || Stored.WorkProduct.TenantId != tenantId
                || Stored.WorkProduct.Id != workProductId || Request.Id != reviewRequestId)
            {
                return Task.FromResult<ReviewDecisionContext?>(null);
            }

            return Task.FromResult<ReviewDecisionContext?>(new ReviewDecisionContext(
                Stored,
                Request.Id,
                Request.Kind,
                Request.RequestedPrincipalId,
                RequestState));
        }

        public Task RecordDecisionAsync(ReviewDecisionResult result, CancellationToken cancellationToken = default)
        {
            if (Stored is null || RequestState != ReviewRequestState.Open || Stored.CurrentRevision.State != WorkProductRevisionState.InReview)
            {
                throw new WorkProductStateConflictException("The review request is no longer open.");
            }

            Stored = new WorkProductRecord(Stored.WorkProduct, result.Revision);
            RequestState = ReviewRequestState.Completed;
            Decision = result;
            return Task.CompletedTask;
        }
    }
}
