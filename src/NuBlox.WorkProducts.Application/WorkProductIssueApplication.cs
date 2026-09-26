using NuBlox.Kernel;

namespace NuBlox.WorkProducts.Application;

public sealed record IssueWorkProductRevisionCommand(
    TenantId TenantId,
    WorkProductId WorkProductId,
    PrincipalId ActorPrincipalId,
    string? CorrelationId = null);

public sealed record WorkProductIssueContext(
    WorkProductRecord Record,
    DecisionEvidenceId ApprovalDecisionEvidenceId);

public sealed record WorkProductIssueResult(
    WorkProductRevision Revision,
    WorkProductIssueEvidence Evidence,
    WorkProductDeliveryIntent? DeliveryIntent = null);

public interface IWorkProductIssueRepository
{
    Task<WorkProductIssueContext?> FindIssueContextAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default);

    Task RecordIssueAsync(
        WorkProductIssueResult result,
        CancellationToken cancellationToken = default);
}

public interface IWorkProductIssueAccessEvaluator
{
    ValueTask<bool> CanIssueAsync(
        WorkProductIssueContext context,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default);
}

public sealed class WorkProductIssueAccessDeniedException : Exception
{
    public WorkProductIssueAccessDeniedException()
        : base("The Principal is not permitted to issue this Work Product revision.")
    {
    }
}

public sealed class WorkProductIssueService
{
    private readonly IWorkProductIssueRepository _repository;
    private readonly IWorkProductIssueAccessEvaluator _accessEvaluator;
    private readonly ISystemClock _clock;

    public WorkProductIssueService(
        IWorkProductIssueRepository repository,
        IWorkProductIssueAccessEvaluator accessEvaluator,
        ISystemClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _accessEvaluator = accessEvaluator ?? throw new ArgumentNullException(nameof(accessEvaluator));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<WorkProductIssueResult> IssueAsync(
        IssueWorkProductRevisionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var context = await _repository.FindIssueContextAsync(
            command.TenantId,
            command.WorkProductId,
            cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("An approved Work Product revision with approval evidence was not found in the verified Tenant context.");

        if (context.Record.CurrentRevision.State != WorkProductRevisionState.Approved)
        {
            throw new WorkProductStateConflictException("Only an Approved revision can be issued.");
        }

        if (!await _accessEvaluator.CanIssueAsync(
            context,
            command.ActorPrincipalId,
            cancellationToken).ConfigureAwait(false))
        {
            throw new WorkProductIssueAccessDeniedException();
        }

        var issuedAt = _clock.UtcNow;
        WorkProductRevision issuedRevision;
        WorkProductIssueEvidence evidence;
        try
        {
            evidence = WorkProductIssueEvidence.Create(
                context.Record.CurrentRevision,
                context.ApprovalDecisionEvidenceId,
                command.ActorPrincipalId,
                issuedAt,
                command.CorrelationId);
            issuedRevision = context.Record.CurrentRevision.Issue(command.ActorPrincipalId, issuedAt);
        }
        catch (InvalidOperationException exception)
        {
            throw new WorkProductStateConflictException(exception.Message);
        }

        var deliveryIntent = WorkProductDeliveryIntent.Create(evidence);
        var result = new WorkProductIssueResult(issuedRevision, evidence, deliveryIntent);
        await _repository.RecordIssueAsync(result, cancellationToken).ConfigureAwait(false);
        return result;
    }
}
