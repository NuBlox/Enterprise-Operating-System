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

public sealed record WorkProductRecord(WorkProduct WorkProduct, WorkProductRevision CurrentRevision);

public sealed record ReviewSubmission(WorkProductRevision Revision, ReviewRequest ReviewRequest);

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
}

public interface IWorkProductAccessEvaluator
{
    ValueTask<bool> CanSubmitForReviewAsync(
        WorkProductRecord record,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default);
}

public sealed class WorkProductAccessDeniedException : Exception
{
    public WorkProductAccessDeniedException()
        : base("The Principal is not permitted to submit this Work Product for review.")
    {
    }
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
    private readonly ISystemClock _clock;

    public WorkProductApplicationService(
        IWorkProductRepository repository,
        IWorkProductAccessEvaluator accessEvaluator,
        ISystemClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _accessEvaluator = accessEvaluator ?? throw new ArgumentNullException(nameof(accessEvaluator));
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
}
