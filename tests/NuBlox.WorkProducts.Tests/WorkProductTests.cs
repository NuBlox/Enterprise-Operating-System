using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Kernel;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Tests;

[TestClass]
public sealed class WorkProductTests
{
    [TestMethod]
    public async Task CreateProducesActiveProductAndInitialDraftRevision()
    {
        var repository = new InMemoryRepository();
        var timestamp = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);
        var actor = PrincipalId.New();
        var tenant = TenantId.New();
        var service = CreateService(repository, timestamp, allowSubmission: true);

        var result = await service.CreateAsync(new CreateWorkProductCommand(
            tenant, actor, "  Configuration Plan  ", "  governed-document  "));

        Assert.AreEqual(tenant, result.WorkProduct.TenantId);
        Assert.AreEqual("Configuration Plan", result.WorkProduct.Title);
        Assert.AreEqual("governed-document", result.WorkProduct.ProductType);
        Assert.AreEqual(actor, result.WorkProduct.OwnerPrincipalId);
        Assert.AreEqual(WorkProductLifecycle.Active, result.WorkProduct.Lifecycle);
        Assert.AreEqual(1, result.WorkProduct.CurrentRevisionNumber);
        Assert.AreEqual(WorkProductRevisionState.Draft, result.CurrentRevision.State);
        Assert.IsNull(result.CurrentRevision.SubmittedAtUtc);
    }

    [TestMethod]
    public async Task SubmitForReviewCreatesRoutedOpenRequestAndSubmissionEvidence()
    {
        var repository = new InMemoryRepository();
        var createdAt = new DateTimeOffset(2026, 9, 26, 22, 0, 0, TimeSpan.Zero);
        var submittedAt = createdAt.AddMinutes(15);
        var actor = PrincipalId.New();
        var reviewer = PrincipalId.New();
        var tenant = TenantId.New();
        var service = CreateService(repository, createdAt, allowSubmission: true);
        var created = await service.CreateAsync(new CreateWorkProductCommand(
            tenant, actor, "Configuration Plan", "governed-document"));
        service = CreateService(repository, submittedAt, allowSubmission: true);

        var result = await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenant, created.WorkProduct.Id, actor, reviewer));

        Assert.AreEqual(WorkProductRevisionState.InReview, result.Revision.State);
        Assert.AreEqual(actor, result.Revision.SubmittedByPrincipalId);
        Assert.AreEqual(submittedAt, result.Revision.SubmittedAtUtc);
        Assert.AreEqual(ReviewRequestKind.Review, result.ReviewRequest.Kind);
        Assert.AreEqual(ReviewRequestState.Open, result.ReviewRequest.State);
        Assert.AreEqual(reviewer, result.ReviewRequest.RequestedPrincipalId);
        Assert.AreEqual(actor, result.ReviewRequest.RequestedByPrincipalId);
        Assert.AreSame(result, repository.Submission);
    }

    [TestMethod]
    public async Task SubmitForReviewFailsClosedWhenAccessEvaluatorDeniesActor()
    {
        var repository = new InMemoryRepository();
        var tenant = TenantId.New();
        var actor = PrincipalId.New();
        var service = CreateService(repository, DateTimeOffset.UtcNow, allowSubmission: true);
        var created = await service.CreateAsync(new CreateWorkProductCommand(
            tenant, actor, "Design Basis", "governed-document"));
        service = CreateService(repository, DateTimeOffset.UtcNow.AddMinutes(1), allowSubmission: false);

        await Assert.ThrowsAsync<WorkProductAccessDeniedException>(() => service.SubmitForReviewAsync(
            new SubmitWorkProductForReviewCommand(tenant, created.WorkProduct.Id, actor, PrincipalId.New())));

        Assert.IsNull(repository.Submission);
        Assert.AreEqual(WorkProductRevisionState.Draft, repository.Stored!.CurrentRevision.State);
    }

    [TestMethod]
    public async Task RevisionCannotBeSubmittedTwice()
    {
        var repository = new InMemoryRepository();
        var tenant = TenantId.New();
        var actor = PrincipalId.New();
        var service = CreateService(repository, DateTimeOffset.UtcNow, allowSubmission: true);
        var created = await service.CreateAsync(new CreateWorkProductCommand(
            tenant, actor, "Design Basis", "governed-document"));
        await service.SubmitForReviewAsync(new SubmitWorkProductForReviewCommand(
            tenant, created.WorkProduct.Id, actor, PrincipalId.New()));

        await Assert.ThrowsAsync<WorkProductStateConflictException>(() => service.SubmitForReviewAsync(
            new SubmitWorkProductForReviewCommand(tenant, created.WorkProduct.Id, actor, PrincipalId.New())));
    }

    private static WorkProductApplicationService CreateService(
        InMemoryRepository repository,
        DateTimeOffset timestamp,
        bool allowSubmission) =>
        new(repository, new FixedAccessEvaluator(allowSubmission), new FixedClock(timestamp));

    private sealed class FixedClock(DateTimeOffset timestamp) : ISystemClock
    {
        public DateTimeOffset UtcNow => timestamp;
    }

    private sealed class FixedAccessEvaluator(bool allow) : IWorkProductAccessEvaluator
    {
        public ValueTask<bool> CanSubmitForReviewAsync(
            WorkProductRecord record,
            PrincipalId actorPrincipalId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(allow);
        }
    }

    private sealed class InMemoryRepository : IWorkProductRepository
    {
        public WorkProductRecord? Stored { get; private set; }
        public ReviewSubmission? Submission { get; private set; }

        public Task AddAsync(WorkProductRecord record, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Stored = record;
            return Task.CompletedTask;
        }

        public Task<WorkProductRecord?> FindAsync(
            TenantId tenantId,
            WorkProductId workProductId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                Stored is not null
                && Stored.WorkProduct.TenantId == tenantId
                && Stored.WorkProduct.Id == workProductId
                    ? Stored
                    : null);
        }

        public Task SubmitForReviewAsync(ReviewSubmission submission, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Stored is null || Stored.CurrentRevision.State != WorkProductRevisionState.Draft)
            {
                throw new WorkProductStateConflictException("The revision is no longer Draft.");
            }

            Submission = submission;
            Stored = new WorkProductRecord(Stored.WorkProduct, submission.Revision);
            return Task.CompletedTask;
        }
    }
}
