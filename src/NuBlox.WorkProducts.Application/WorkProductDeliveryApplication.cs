using NuBlox.Kernel;

namespace NuBlox.WorkProducts.Application;

public readonly record struct WorkProductDeliveryIntentId
{
    public WorkProductDeliveryIntentId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Delivery intent identifiers cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static WorkProductDeliveryIntentId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public enum WorkProductDeliveryState
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

public sealed record WorkProductDeliveryIntent(
    WorkProductDeliveryIntentId Id,
    TenantId TenantId,
    WorkProductId WorkProductId,
    WorkProductRevisionId WorkProductRevisionId,
    IssueEvidenceId IssueEvidenceId,
    string ConsequenceType,
    string IdempotencyKey,
    DateTimeOffset CreatedAtUtc,
    string? CorrelationId)
{
    public const string IssuedConsequenceType = "WORK_PRODUCT_ISSUED";

    public static WorkProductDeliveryIntent Create(WorkProductIssueEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        return new WorkProductDeliveryIntent(
            WorkProductDeliveryIntentId.New(),
            evidence.TenantId,
            evidence.WorkProductId,
            evidence.WorkProductRevisionId,
            evidence.Id,
            IssuedConsequenceType,
            $"work-product-issued:{evidence.TenantId}:{evidence.Id}",
            evidence.IssuedAtUtc,
            evidence.CorrelationId);
    }
}

public sealed record WorkProductDeliveryClaim(
    WorkProductDeliveryIntent Intent,
    int AttemptNumber,
    string WorkerId,
    DateTimeOffset LeaseExpiresAtUtc);

public sealed record WorkProductDeliveryStatus(
    WorkProductDeliveryIntent Intent,
    WorkProductDeliveryState State,
    int AttemptCount,
    DateTimeOffset? NextAttemptAtUtc,
    string? ClaimedBy,
    DateTimeOffset? ClaimExpiresAtUtc,
    string? LastFailureCode,
    DateTimeOffset? CompletedAtUtc);

public enum WorkProductDeliveryOutcomeKind
{
    Succeeded = 1,
    RetryableFailure = 2,
    TerminalFailure = 3
}

public sealed record WorkProductDeliveryOutcome(
    WorkProductDeliveryOutcomeKind Kind,
    string? FailureCode = null,
    TimeSpan? RetryAfter = null)
{
    public static WorkProductDeliveryOutcome Succeeded() => new(WorkProductDeliveryOutcomeKind.Succeeded);

    public static WorkProductDeliveryOutcome Retryable(string failureCode, TimeSpan retryAfter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failureCode);
        if (retryAfter <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(retryAfter));
        return new WorkProductDeliveryOutcome(WorkProductDeliveryOutcomeKind.RetryableFailure, failureCode.Trim(), retryAfter);
    }

    public static WorkProductDeliveryOutcome Terminal(string failureCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failureCode);
        return new WorkProductDeliveryOutcome(WorkProductDeliveryOutcomeKind.TerminalFailure, failureCode.Trim());
    }
}

public interface IWorkProductDeliveryStore
{
    Task<WorkProductDeliveryClaim?> ClaimNextAsync(
        TenantId tenantId,
        string workerId,
        DateTimeOffset nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    Task RecordCompletedAsync(
        TenantId tenantId,
        WorkProductDeliveryIntentId intentId,
        string workerId,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken = default);

    Task RecordRetryAsync(
        TenantId tenantId,
        WorkProductDeliveryIntentId intentId,
        string workerId,
        DateTimeOffset nextAttemptAtUtc,
        string failureCode,
        CancellationToken cancellationToken = default);

    Task RecordFailedAsync(
        TenantId tenantId,
        WorkProductDeliveryIntentId intentId,
        string workerId,
        string failureCode,
        CancellationToken cancellationToken = default);

    Task<WorkProductDeliveryStatus?> FindAsync(
        TenantId tenantId,
        WorkProductDeliveryIntentId intentId,
        CancellationToken cancellationToken = default);
}

public interface IWorkProductDeliveryProvider
{
    Task<WorkProductDeliveryOutcome> DeliverAsync(
        WorkProductDeliveryClaim claim,
        CancellationToken cancellationToken = default);
}

public sealed class WorkProductDeliveryWorker
{
    private readonly IWorkProductDeliveryStore _store;
    private readonly IWorkProductDeliveryProvider _provider;
    private readonly ISystemClock _clock;

    public WorkProductDeliveryWorker(
        IWorkProductDeliveryStore store,
        IWorkProductDeliveryProvider provider,
        ISystemClock clock)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<bool> ProcessOneAsync(
        TenantId tenantId,
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        if (leaseDuration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(leaseDuration));

        var claim = await _store.ClaimNextAsync(
            tenantId,
            workerId.Trim(),
            _clock.UtcNow,
            leaseDuration,
            cancellationToken).ConfigureAwait(false);
        if (claim is null)
        {
            return false;
        }

        var outcome = await _provider.DeliverAsync(claim, cancellationToken).ConfigureAwait(false);
        switch (outcome.Kind)
        {
            case WorkProductDeliveryOutcomeKind.Succeeded:
                await _store.RecordCompletedAsync(
                    tenantId,
                    claim.Intent.Id,
                    claim.WorkerId,
                    _clock.UtcNow,
                    cancellationToken).ConfigureAwait(false);
                break;
            case WorkProductDeliveryOutcomeKind.RetryableFailure:
                await _store.RecordRetryAsync(
                    tenantId,
                    claim.Intent.Id,
                    claim.WorkerId,
                    _clock.UtcNow.Add(outcome.RetryAfter ?? throw new InvalidOperationException("Retryable delivery requires a retry delay.")),
                    outcome.FailureCode ?? throw new InvalidOperationException("Retryable delivery requires a failure code."),
                    cancellationToken).ConfigureAwait(false);
                break;
            case WorkProductDeliveryOutcomeKind.TerminalFailure:
                await _store.RecordFailedAsync(
                    tenantId,
                    claim.Intent.Id,
                    claim.WorkerId,
                    outcome.FailureCode ?? throw new InvalidOperationException("Terminal delivery requires a failure code."),
                    cancellationToken).ConfigureAwait(false);
                break;
            default:
                throw new InvalidOperationException($"Unknown delivery outcome '{outcome.Kind}'.");
        }

        return true;
    }
}
