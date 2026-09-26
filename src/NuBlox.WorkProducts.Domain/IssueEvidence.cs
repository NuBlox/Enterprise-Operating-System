using NuBlox.Kernel;

namespace NuBlox.WorkProducts;

public readonly record struct IssueEvidenceId
{
    public IssueEvidenceId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Issue evidence identifiers cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static IssueEvidenceId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public sealed record WorkProductIssueEvidence
{
    private WorkProductIssueEvidence(
        IssueEvidenceId id,
        TenantId tenantId,
        WorkProductId workProductId,
        WorkProductRevisionId workProductRevisionId,
        DecisionEvidenceId approvalDecisionEvidenceId,
        PrincipalId issuedByPrincipalId,
        DateTimeOffset issuedAtUtc,
        string? correlationId)
    {
        Id = id;
        TenantId = tenantId;
        WorkProductId = workProductId;
        WorkProductRevisionId = workProductRevisionId;
        ApprovalDecisionEvidenceId = approvalDecisionEvidenceId;
        IssuedByPrincipalId = issuedByPrincipalId;
        IssuedAtUtc = issuedAtUtc;
        CorrelationId = correlationId;
    }

    public IssueEvidenceId Id { get; }
    public TenantId TenantId { get; }
    public WorkProductId WorkProductId { get; }
    public WorkProductRevisionId WorkProductRevisionId { get; }
    public DecisionEvidenceId ApprovalDecisionEvidenceId { get; }
    public PrincipalId IssuedByPrincipalId { get; }
    public DateTimeOffset IssuedAtUtc { get; }
    public string? CorrelationId { get; }

    public static WorkProductIssueEvidence Create(
        WorkProductRevision approvedRevision,
        DecisionEvidenceId approvalDecisionEvidenceId,
        PrincipalId actorPrincipalId,
        DateTimeOffset issuedAtUtc,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(approvedRevision);
        if (approvedRevision.State != WorkProductRevisionState.Approved)
        {
            throw new InvalidOperationException("Issue evidence can only be created for an Approved revision.");
        }
        if (issuedAtUtc == default) throw new ArgumentException("Issue timestamp is required.", nameof(issuedAtUtc));

        var canonicalCorrelation = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
        if (canonicalCorrelation?.Length > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(correlationId), "Correlation identifier cannot exceed 128 characters.");
        }

        return new WorkProductIssueEvidence(
            IssueEvidenceId.New(),
            approvedRevision.TenantId,
            approvedRevision.WorkProductId,
            approvedRevision.Id,
            approvalDecisionEvidenceId,
            actorPrincipalId,
            issuedAtUtc.ToUniversalTime(),
            canonicalCorrelation);
    }
}
