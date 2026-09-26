using NuBlox.Kernel;

namespace NuBlox.WorkProducts;

public readonly record struct ReviewDecisionId
{
    public ReviewDecisionId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Review Decision identifiers cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static ReviewDecisionId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public enum ReviewDecisionOutcome
{
    ChangesRequired = 1,
    Rejected = 2,
    Approved = 3
}

public sealed record ReviewDecision
{
    private ReviewDecision(
        ReviewDecisionId id,
        TenantId tenantId,
        ReviewRequestId reviewRequestId,
        WorkProductRevisionId workProductRevisionId,
        ReviewDecisionOutcome outcome,
        PrincipalId decidedByPrincipalId,
        DateTimeOffset decidedAtUtc,
        string? rationale)
    {
        Id = id;
        TenantId = tenantId;
        ReviewRequestId = reviewRequestId;
        WorkProductRevisionId = workProductRevisionId;
        Outcome = outcome;
        DecidedByPrincipalId = decidedByPrincipalId;
        DecidedAtUtc = decidedAtUtc;
        Rationale = rationale;
    }

    public ReviewDecisionId Id { get; }
    public TenantId TenantId { get; }
    public ReviewRequestId ReviewRequestId { get; }
    public WorkProductRevisionId WorkProductRevisionId { get; }
    public ReviewDecisionOutcome Outcome { get; }
    public PrincipalId DecidedByPrincipalId { get; }
    public DateTimeOffset DecidedAtUtc { get; }
    public string? Rationale { get; }

    public static ReviewDecision Create(
        TenantId tenantId,
        ReviewRequestId reviewRequestId,
        WorkProductRevision revision,
        ReviewDecisionOutcome outcome,
        PrincipalId decidedByPrincipalId,
        DateTimeOffset decidedAtUtc,
        string? rationale)
    {
        ArgumentNullException.ThrowIfNull(revision);
        if (revision.TenantId != tenantId) throw new InvalidOperationException("Decision and revision Tenant identities must match.");
        if (revision.State != WorkProductRevisionState.InReview)
        {
            throw new InvalidOperationException($"A decision requires an InReview revision; current state is {revision.State}.");
        }
        if (decidedAtUtc == default) throw new ArgumentException("Decision timestamp is required.", nameof(decidedAtUtc));

        var canonicalRationale = string.IsNullOrWhiteSpace(rationale) ? null : rationale.Trim();
        if (canonicalRationale is { Length: > 1000 })
        {
            throw new ArgumentOutOfRangeException(nameof(rationale), "Decision rationale cannot exceed 1000 characters.");
        }
        if (outcome is ReviewDecisionOutcome.ChangesRequired or ReviewDecisionOutcome.Rejected
            && canonicalRationale is null)
        {
            throw new ArgumentException("Changes-required and rejected decisions require a rationale.", nameof(rationale));
        }

        return new ReviewDecision(
            ReviewDecisionId.New(),
            tenantId,
            reviewRequestId,
            revision.Id,
            outcome,
            decidedByPrincipalId,
            decidedAtUtc.ToUniversalTime(),
            canonicalRationale);
    }

    public WorkProductRevision ApplyTo(WorkProductRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        if (revision.Id != WorkProductRevisionId || revision.State != WorkProductRevisionState.InReview)
        {
            throw new InvalidOperationException("The decision cannot be applied to the supplied revision state/identity.");
        }

        var targetState = Outcome switch
        {
            ReviewDecisionOutcome.ChangesRequired => WorkProductRevisionState.ChangesRequired,
            ReviewDecisionOutcome.Rejected => WorkProductRevisionState.Rejected,
            ReviewDecisionOutcome.Approved => WorkProductRevisionState.Approved,
            _ => throw new InvalidOperationException($"Unknown Review Decision outcome '{Outcome}'.")
        };

        return WorkProductRevision.Restore(
            revision.Id,
            revision.TenantId,
            revision.WorkProductId,
            revision.RevisionNumber,
            revision.TitleSnapshot,
            targetState,
            revision.CreatedByPrincipalId,
            revision.CreatedAtUtc,
            revision.SubmittedByPrincipalId,
            revision.SubmittedAtUtc);
    }
}
