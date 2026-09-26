using NuBlox.Authority;
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

public sealed record DecideWorkProductReviewCommand(
    TenantId TenantId,
    WorkProductId WorkProductId,
    ReviewRequestId ReviewRequestId,
    PrincipalId ActorPrincipalId,
    ReviewDecisionOutcome Outcome,
    string Rationale,
    string? CorrelationId = null);

public sealed record WorkProductRecord(WorkProduct WorkProduct, WorkProductRevision CurrentRevision);

public sealed record ReviewSubmission(WorkProductRevision Revision, ReviewRequest ReviewRequest);

public sealed record ReviewDecisionContext(
    WorkProductRecord Record,
    ReviewRequestId ReviewRequestId,
    ReviewRequestKind Kind,
    PrincipalId RequestedPrincipalId,
    ReviewRequestState State);

public sealed record ReviewDecisionResult(WorkProductRevision Revision, WorkProductDecision Decision);

public interface IWorkProductRepository
{
    Task AddAsync(WorkProductRecord record, CancellationToken cancellationToken = default);

    Task<WorkProductRecord?> FindAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default);

    Task SubmitForReviewAsync(
        ReviewSubmission submission,
        CancellationToken cancellationToken = default);

    Task<ReviewDecisionContext?> FindReviewDecisionContextAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        ReviewRequestId reviewRequestId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<ReviewDecisionContext?>(null);

    Task RecordDecisionAsync(
        ReviewDecisionResult result,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("This repository does not support review decisions.");
}

public interface IWorkProductAccessEvaluator
{
    ValueTask<bool> CanSubmitForReviewAsync(
        WorkProductRecord record,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default);

    ValueTask<bool> CanDecideReviewAsync(
        ReviewDecisionContext context,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(false);
}

public sealed class WorkProductAccessDeniedException : Exception
{
    public WorkProductAccessDeniedException()
        : base("The Principal is not permitted to submit this Work Product for review.")
    {
    }
}

public sealed class WorkProductDecisionAccessDeniedException : Exception
{
    public WorkProductDecisionAccessDeniedException()
        : base("The Principal is not permitted to act on this review request.")
    {
    }
}

public sealed class WorkProductAuthorityDeniedException : Exception
{
    public WorkProductAuthorityDeniedException(string? denialReasonCode)
        : base("The Principal does not hold the required business authority for this decision.")
    {
        DenialReasonCode = denialReasonCode;
    }

    public string? DenialReasonCode { get; }
}

public sealed class WorkProductStateConflictException : Exception
{
    public WorkProductStateConflictException(string message)
        : base(message)
    {
    }
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
    private readonly IWorkProductAccessEvaluator _accessEvaluator;
    private readonly IBusinessAuthorityEvaluator _authorityEvaluator;
    private readonly ISystemClock _clock;

    public WorkProductApplicationService(
        IWorkProductRepository repository,
        IWorkProductAccessEvaluator accessEvaluator,
        ISystemClock clock)
        : this(repository, accessEvaluator, new DenyAllAuthorityEvaluator(), clock)
    {
    }

    public WorkProductApplicationService(
        IWorkProductRepository repository,
        IWorkProductAccessEvaluator accessEvaluator,
        IBusinessAuthorityEvaluator authorityEvaluator,
        ISystemClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _accessEvaluator = accessEvaluator ?? throw new ArgumentNullException(nameof(accessEvaluator));
        _authorityEvaluator = authorityEvaluator ?? throw new ArgumentNullException(nameof(authorityEvaluator));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<WorkProductRecord> CreateAsync(
        CreateWorkProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workProduct = WorkProduct.Create(
            command.TenantId,
            command.Title,
            command.ProductType,
            command.OwnerPrincipalId ?? command.ActorPrincipalId,
            command.ActorPrincipalId,
            _clock.UtcNow);
        var revision = WorkProductRevision.CreateInitial(workProduct);
        var record = new WorkProductRecord(workProduct, revision);

        await _repository.AddAsync(record, cancellationToken).ConfigureAwait(false);
        return record;
    }

    public Task<WorkProductRecord?> FindAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default) =>
        _repository.FindAsync(tenantId, workProductId, cancellationToken);

    public async Task<ReviewSubmission> SubmitForReviewAsync(
        SubmitWorkProductForReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var record = await _repository.FindAsync(
            command.TenantId,
            command.WorkProductId,
            cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The Work Product was not found in the verified Tenant context.");

        if (!await _accessEvaluator.CanSubmitForReviewAsync(
            record,
            command.ActorPrincipalId,
            cancellationToken).ConfigureAwait(false))
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
            submittedRevision,
            command.ReviewerPrincipalId,
            command.ActorPrincipalId,
            submittedRevision.SubmittedAtUtc!.Value);
        var submission = new ReviewSubmission(submittedRevision, reviewRequest);

        await _repository.SubmitForReviewAsync(submission, cancellationToken).ConfigureAwait(false);
        return submission;
    }

    public async Task<ReviewDecisionResult> DecideReviewAsync(
        DecideWorkProductReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var context = await _repository.FindReviewDecisionContextAsync(
            command.TenantId,
            command.WorkProductId,
            command.ReviewRequestId,
            cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The review request was not found in the verified Tenant context.");

        if (context.State != ReviewRequestState.Open || context.Record.CurrentRevision.State != WorkProductRevisionState.InReview)
        {
            throw new WorkProductStateConflictException("The review request is no longer open for a decision.");
        }

        if (!await _accessEvaluator.CanDecideReviewAsync(
            context,
            command.ActorPrincipalId,
            cancellationToken).ConfigureAwait(false))
        {
            throw new WorkProductDecisionAccessDeniedException();
        }

        var actionCode = command.Outcome switch
        {
            ReviewDecisionOutcome.Approved => "work-products.revision.approve",
            ReviewDecisionOutcome.Rejected => "work-products.revision.reject",
            _ => throw new InvalidOperationException($"Unknown review decision outcome '{command.Outcome}'.")
        };

        var authority = await _authorityEvaluator.EvaluateAsync(
            new AuthorityEvaluationContext(
                command.TenantId,
                command.ActorPrincipalId,
                new AuthorityRequirement(
                    actionCode,
                    "work-product-revision",
                    context.Record.CurrentRevision.Id.ToString())),
            cancellationToken).ConfigureAwait(false);

        if (!authority.IsGranted)
        {
            throw new WorkProductAuthorityDeniedException(authority.DenialReasonCode);
        }

        WorkProductDecision decision;
        WorkProductRevision decidedRevision;
        try
        {
            decision = WorkProductDecision.Create(
                context.Record.CurrentRevision,
                context.ReviewRequestId,
                command.ActorPrincipalId,
                command.Outcome,
                command.Rationale,
                authority.AuthorityReference!,
                _clock.UtcNow,
                command.CorrelationId);
            decidedRevision = decision.ApplyTo(context.Record.CurrentRevision);
        }
        catch (InvalidOperationException exception)
        {
            throw new WorkProductStateConflictException(exception.Message);
        }

        var result = new ReviewDecisionResult(decidedRevision, decision);
        await _repository.RecordDecisionAsync(result, cancellationToken).ConfigureAwait(false);
        return result;
    }

    private sealed class DenyAllAuthorityEvaluator : IBusinessAuthorityEvaluator
    {
        public ValueTask<AuthorityEvaluationResult> EvaluateAsync(
            AuthorityEvaluationContext context,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(AuthorityEvaluationResult.Denied("authority-evaluator-not-configured"));
    }
}
