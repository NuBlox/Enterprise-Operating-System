using NuBlox.Kernel;

namespace NuBlox.WorkProducts.Application;

public sealed record CreateWorkProductCommand(
    TenantId TenantId,
    PrincipalId ActorPrincipalId,
    string Title,
    string ProductType,
    PrincipalId? OwnerPrincipalId = null);

public sealed record SubmitWorkProductForReviewCommand(
    TenantId TenantId,
    WorkProductId WorkProductId,
    PrincipalId ActorPrincipalId,
    PrincipalId ReviewerPrincipalId);

public sealed record RecordReviewDecisionCommand(
    TenantId TenantId,
    WorkProductId WorkProductId,
    ReviewRequestId ReviewRequestId,
    PrincipalId ActorPrincipalId,
    ReviewDecisionOutcome Outcome,
    string? Rationale = null);

public sealed record WorkProductRecord(WorkProduct WorkProduct, WorkProductRevision CurrentRevision);
public sealed record ReviewSubmission(WorkProductRevision Revision, ReviewRequest ReviewRequest);
public sealed record ReviewRequestAssignment(
    ReviewRequestId ReviewRequestId,
    TenantId TenantId,
    WorkProductRevisionId WorkProductRevisionId,
    PrincipalId RequestedPrincipalId);
public sealed record ReviewDecisionResult(
    WorkProductRevision Revision,
    ReviewRequestAssignment Assignment,
    ReviewDecision Decision);

public interface IWorkProductRepository
{
    Task AddAsync(WorkProductRecord record, CancellationToken cancellationToken = default);
    Task<WorkProductRecord?> FindAsync(TenantId tenantId, WorkProductId workProductId, CancellationToken cancellationToken = default);
    Task SubmitForReviewAsync(ReviewSubmission submission, CancellationToken cancellationToken = default);
}

public interface IWorkProductDecisionRepository
{
    Task<ReviewRequestAssignment?> FindOpenReviewRequestAsync(
        TenantId tenantId,
        ReviewRequestId reviewRequestId,
        CancellationToken cancellationToken = default);
    Task ApplyReviewDecisionAsync(ReviewDecisionResult result, CancellationToken cancellationToken = default);
}

public interface IWorkProductAccessEvaluator
{
    ValueTask<bool> CanSubmitForReviewAsync(
        WorkProductRecord record,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default);
}

public interface IWorkProductDecisionAuthorityEvaluator
{
    ValueTask<bool> HasDecisionAuthorityAsync(
        WorkProductRecord record,
        ReviewRequestAssignment assignment,
        PrincipalId actorPrincipalId,
        ReviewDecisionOutcome outcome,
        CancellationToken cancellationToken = default);
}

public sealed class WorkProductAccessDeniedException : Exception
{
    public WorkProductAccessDeniedException() : base("The Principal is not permitted to perform this Work Product action.") { }
}

public sealed class WorkProductAuthorityDeniedException : Exception
{
    public WorkProductAuthorityDeniedException() : base("The Principal does not hold the required business authority for this decision.") { }
}

public sealed class WorkProductStateConflictException : Exception
{
    public WorkProductStateConflictException(string message) : base(message) { }
}

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class WorkProductApplicationService
{
    private readonly IWorkProductRepository _repository;
    private readonly IWorkProductDecisionRepository _decisionRepository;
    private readonly IWorkProductAccessEvaluator _accessEvaluator;
    private readonly IWorkProductDecisionAuthorityEvaluator _authorityEvaluator;
    private readonly ISystemClock _clock;

    public WorkProductApplicationService(
        IWorkProductRepository repository,
        IWorkProductAccessEvaluator accessEvaluator,
        ISystemClock clock)
        : this(repository, new UnavailableDecisionRepository(), accessEvaluator, new DenyDecisionAuthorityEvaluator(), clock)
    {
    }

    public WorkProductApplicationService(
        IWorkProductRepository repository,
        IWorkProductDecisionRepository decisionRepository,
        IWorkProductAccessEvaluator accessEvaluator,
        IWorkProductDecisionAuthorityEvaluator authorityEvaluator,
        ISystemClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _decisionRepository = decisionRepository ?? throw new ArgumentNullException(nameof(decisionRepository));
        _accessEvaluator = accessEvaluator ?? throw new ArgumentNullException(nameof(accessEvaluator));
        _authorityEvaluator = authorityEvaluator ?? throw new ArgumentNullException(nameof(authorityEvaluator));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<WorkProductRecord> CreateAsync(CreateWorkProductCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var workProduct = WorkProduct.Create(
            command.TenantId, command.Title, command.ProductType,
            command.OwnerPrincipalId ?? command.ActorPrincipalId,
            command.ActorPrincipalId, _clock.UtcNow);
        var record = new WorkProductRecord(workProduct, WorkProductRevision.CreateInitial(workProduct));
        await _repository.AddAsync(record, cancellationToken).ConfigureAwait(false);
        return record;
    }

    public Task<WorkProductRecord?> FindAsync(TenantId tenantId, WorkProductId workProductId, CancellationToken cancellationToken = default) =>
        _repository.FindAsync(tenantId, workProductId, cancellationToken);

    public async Task<ReviewSubmission> SubmitForReviewAsync(SubmitWorkProductForReviewCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var record = await RequireRecordAsync(command.TenantId, command.WorkProductId, cancellationToken).ConfigureAwait(false);
        if (!await _accessEvaluator.CanSubmitForReviewAsync(record, command.ActorPrincipalId, cancellationToken).ConfigureAwait(false))
        {
            throw new WorkProductAccessDeniedException();
        }

        WorkProductRevision submittedRevision;
        try
        {
            submittedRevision = record.CurrentRevision.SubmitForReview(command.ActorPrincipalId, _clock.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            throw new WorkProductStateConflictException(exception.Message);
        }

        var reviewRequest = ReviewRequest.CreateReview(
            submittedRevision, command.ReviewerPrincipalId, command.ActorPrincipalId, submittedRevision.SubmittedAtUtc!.Value);
        var submission = new ReviewSubmission(submittedRevision, reviewRequest);
        await _repository.SubmitForReviewAsync(submission, cancellationToken).ConfigureAwait(false);
        return submission;
    }

    public async Task<ReviewDecisionResult> RecordReviewDecisionAsync(RecordReviewDecisionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var record = await RequireRecordAsync(command.TenantId, command.WorkProductId, cancellationToken).ConfigureAwait(false);
        var assignment = await _decisionRepository.FindOpenReviewRequestAsync(
            command.TenantId, command.ReviewRequestId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The open Review Request was not found in the verified Tenant context.");

        if (assignment.WorkProductRevisionId != record.CurrentRevision.Id
            || assignment.RequestedPrincipalId != command.ActorPrincipalId)
        {
            throw new WorkProductAccessDeniedException();
        }

        if (!await _authorityEvaluator.HasDecisionAuthorityAsync(
            record, assignment, command.ActorPrincipalId, command.Outcome, cancellationToken).ConfigureAwait(false))
        {
            throw new WorkProductAuthorityDeniedException();
        }

        ReviewDecision decision;
        WorkProductRevision decidedRevision;
        try
        {
            decision = ReviewDecision.Create(
                command.TenantId, command.ReviewRequestId, record.CurrentRevision,
                command.Outcome, command.ActorPrincipalId, _clock.UtcNow, command.Rationale);
            decidedRevision = decision.ApplyTo(record.CurrentRevision);
        }
        catch (InvalidOperationException exception)
        {
            throw new WorkProductStateConflictException(exception.Message);
        }

        var result = new ReviewDecisionResult(decidedRevision, assignment, decision);
        await _decisionRepository.ApplyReviewDecisionAsync(result, cancellationToken).ConfigureAwait(false);
        return result;
    }

    private async Task<WorkProductRecord> RequireRecordAsync(TenantId tenantId, WorkProductId workProductId, CancellationToken cancellationToken) =>
        await _repository.FindAsync(tenantId, workProductId, cancellationToken).ConfigureAwait(false)
        ?? throw new KeyNotFoundException("The Work Product was not found in the verified Tenant context.");

    private sealed class DenyDecisionAuthorityEvaluator : IWorkProductDecisionAuthorityEvaluator
    {
        public ValueTask<bool> HasDecisionAuthorityAsync(
            WorkProductRecord record,
            ReviewRequestAssignment assignment,
            PrincipalId actorPrincipalId,
            ReviewDecisionOutcome outcome,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(false);
        }
    }

    private sealed class UnavailableDecisionRepository : IWorkProductDecisionRepository
    {
        public Task<ReviewRequestAssignment?> FindOpenReviewRequestAsync(TenantId tenantId, ReviewRequestId reviewRequestId, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Review decision persistence is not configured.");

        public Task ApplyReviewDecisionAsync(ReviewDecisionResult result, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Review decision persistence is not configured.");
    }
}
