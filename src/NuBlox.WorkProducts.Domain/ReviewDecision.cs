using NuBlox.Kernel;

namespace NuBlox.WorkProducts;

public readonly record struct DecisionEvidenceId
{
    public DecisionEvidenceId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Decision evidence identifiers cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static DecisionEvidenceId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public enum ReviewDecisionOutcome
{
    Approved = 1,
    Rejected = 2
}

public sealed record WorkProductDecision
{
    private WorkProductDecision(
        DecisionEvidenceId id,
        TenantId tenantId,
        ReviewRequestId reviewRequestId,
        WorkProductRevisionId workProductRevisionId,
        PrincipalId actorPrincipalId,
        ReviewDecisionOutcome outcome,
        string rationale,
        string authorityReference,
        DateTimeOffset decidedAtUtc,
        string? correlationId)
    {
        Id = id;
        TenantId = tenantId;
        ReviewRequestId = reviewRequestId;
        WorkProductRevisionId = workProductRevisionId;
        ActorPrincipalId = actorPrincipalId;
        Outcome = outcome;
        Rationale = rationale;
        AuthorityReference = authorityReference;
        DecidedAtUtc = decidedAtUtc;
        CorrelationId = correlationId;
    }

    public DecisionEvidenceId Id { get; }
    public TenantId TenantId { get; }
    public ReviewRequestId ReviewRequestId { get; }
    public WorkProductRevisionId WorkProductRevisionId { get; }
    public PrincipalId ActorPrincipalId { get; }
    public ReviewDecisionOutcome Outcome { get; }
    public string Rationale { get; }
    public string AuthorityReference { get; }
    public DateTimeOffset DecidedAtUtc { get; }
    public string? CorrelationId { get; }

    public static WorkProductDecision Create(
        WorkProductRevision revision,
        ReviewRequestId reviewRequestId,
        PrincipalId actorPrincipalId,
        ReviewDecisionOutcome outcome,
        string rationale,
        string authorityReference,
        DateTimeOffset decidedAtUtc,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(revision);
        if (revision.State != WorkProductRevisionState.InReview)
        {
            throw new InvalidOperationException("A decision can only be recorded while the revision is InReview.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(rationale);
        ArgumentException.ThrowIfNullOrWhiteSpace(authorityReference);
        if (decidedAtUtc == default) throw new ArgumentException("Decision timestamp is required.", nameof(decidedAtUtc));

        var canonicalRationale = rationale.Trim();
        if (canonicalRationale.Length > 2000) throw new ArgumentOutOfRangeException(nameof(rationale), "Rationale cannot exceed 2000 characters.");

        var canonicalCorrelation = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
        if (canonicalCorrelation?.Length > 128) throw new ArgumentOutOfRangeException(nameof(correlationId), "Correlation identifier cannot exceed 128 characters.");

        return new WorkProductDecision(
            DecisionEvidenceId.New(),
            revision.TenantId,
            reviewRequestId,
            revision.Id,
            actorPrincipalId,
            outcome,
            canonicalRationale,
            authorityReference.Trim(),
            decidedAtUtc.ToUniversalTime(),
            canonicalCorrelation);
    }

    public WorkProductRevision ApplyTo(WorkProductRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        if (revision.Id != WorkProductRevisionId || revision.TenantId != TenantId)
        {
            throw new InvalidOperationException("Decision evidence does not match the revision.");
        }
        if (revision.State != WorkProductRevisionState.InReview)
        {
            throw new InvalidOperationException("Only an InReview revision can receive a decision.");
        }

        var nextState = Outcome switch
        {
            ReviewDecisionOutcome.Approved => WorkProductRevisionState.Approved,
            ReviewDecisionOutcome.Rejected => WorkProductRevisionState.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(Outcome))
        };

        return WorkProductRevision.Restore(
            revision.Id,
            revision.TenantId,
            revision.WorkProductId,
            revision.RevisionNumber,
            revision.TitleSnapshot,
            nextState,
            revision.CreatedByPrincipalId,
            revision.CreatedAtUtc,
            revision.SubmittedByPrincipalId,
            revision.SubmittedAtUtc);
    }
}
