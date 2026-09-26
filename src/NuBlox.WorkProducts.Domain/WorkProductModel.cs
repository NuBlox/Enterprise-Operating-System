using NuBlox.Kernel;

namespace NuBlox.WorkProducts;

public readonly record struct WorkProductId
{
    public WorkProductId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Work Product identifiers cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static WorkProductId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public readonly record struct WorkProductRevisionId
{
    public WorkProductRevisionId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Work Product revision identifiers cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static WorkProductRevisionId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public readonly record struct ReviewRequestId
{
    public ReviewRequestId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Review Request identifiers cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static ReviewRequestId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public enum WorkProductLifecycle
{
    Active = 1,
    Superseded = 2,
    Withdrawn = 3
}

public enum WorkProductRevisionState
{
    Draft = 1,
    InReview = 2,
    ChangesRequired = 3,
    Rejected = 4,
    Approved = 5,
    Issued = 6,
    Superseded = 7
}

public enum ReviewRequestKind
{
    Review = 1,
    Approval = 2
}

public enum ReviewRequestState
{
    Open = 1,
    Completed = 2,
    Cancelled = 3
}

public sealed record WorkProduct
{
    private WorkProduct(
        WorkProductId id,
        TenantId tenantId,
        string title,
        string productType,
        PrincipalId ownerPrincipalId,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc,
        WorkProductLifecycle lifecycle,
        int currentRevisionNumber)
    {
        Id = id;
        TenantId = tenantId;
        Title = title;
        ProductType = productType;
        OwnerPrincipalId = ownerPrincipalId;
        CreatedByPrincipalId = createdByPrincipalId;
        CreatedAtUtc = createdAtUtc;
        Lifecycle = lifecycle;
        CurrentRevisionNumber = currentRevisionNumber;
    }

    public WorkProductId Id { get; }
    public TenantId TenantId { get; }
    public string Title { get; }
    public string ProductType { get; }
    public PrincipalId OwnerPrincipalId { get; }
    public PrincipalId CreatedByPrincipalId { get; }
    public DateTimeOffset CreatedAtUtc { get; }
    public WorkProductLifecycle Lifecycle { get; }
    public int CurrentRevisionNumber { get; }

    public static WorkProduct Create(
        TenantId tenantId,
        string title,
        string productType,
        PrincipalId ownerPrincipalId,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc)
    {
        var canonicalTitle = RequireText(title, nameof(title), 240);
        var canonicalType = RequireText(productType, nameof(productType), 80);
        if (createdAtUtc == default) throw new ArgumentException("Creation timestamp is required.", nameof(createdAtUtc));

        return new WorkProduct(
            WorkProductId.New(),
            tenantId,
            canonicalTitle,
            canonicalType,
            ownerPrincipalId,
            createdByPrincipalId,
            createdAtUtc.ToUniversalTime(),
            WorkProductLifecycle.Active,
            1);
    }

    public static WorkProduct Restore(
        WorkProductId id,
        TenantId tenantId,
        string title,
        string productType,
        PrincipalId ownerPrincipalId,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc,
        WorkProductLifecycle lifecycle,
        int currentRevisionNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(currentRevisionNumber);
        return new WorkProduct(
            id,
            tenantId,
            RequireText(title, nameof(title), 240),
            RequireText(productType, nameof(productType), 80),
            ownerPrincipalId,
            createdByPrincipalId,
            createdAtUtc.ToUniversalTime(),
            lifecycle,
            currentRevisionNumber);
    }

    private static string RequireText(string value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value cannot be empty.", parameterName);
        var canonical = value.Trim();
        if (canonical.Length > maximumLength) throw new ArgumentOutOfRangeException(parameterName, $"Value cannot exceed {maximumLength} characters.");
        return canonical;
    }
}

public sealed record WorkProductRevision
{
    private WorkProductRevision(
        WorkProductRevisionId id,
        TenantId tenantId,
        WorkProductId workProductId,
        int revisionNumber,
        string titleSnapshot,
        WorkProductRevisionState state,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc,
        PrincipalId? submittedByPrincipalId,
        DateTimeOffset? submittedAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        WorkProductId = workProductId;
        RevisionNumber = revisionNumber;
        TitleSnapshot = titleSnapshot;
        State = state;
        CreatedByPrincipalId = createdByPrincipalId;
        CreatedAtUtc = createdAtUtc;
        SubmittedByPrincipalId = submittedByPrincipalId;
        SubmittedAtUtc = submittedAtUtc;
    }

    public WorkProductRevisionId Id { get; }
    public TenantId TenantId { get; }
    public WorkProductId WorkProductId { get; }
    public int RevisionNumber { get; }
    public string TitleSnapshot { get; }
    public WorkProductRevisionState State { get; }
    public PrincipalId CreatedByPrincipalId { get; }
    public DateTimeOffset CreatedAtUtc { get; }
    public PrincipalId? SubmittedByPrincipalId { get; }
    public DateTimeOffset? SubmittedAtUtc { get; }

    public static WorkProductRevision CreateInitial(WorkProduct workProduct)
    {
        ArgumentNullException.ThrowIfNull(workProduct);
        return new WorkProductRevision(
            WorkProductRevisionId.New(),
            workProduct.TenantId,
            workProduct.Id,
            1,
            workProduct.Title,
            WorkProductRevisionState.Draft,
            workProduct.CreatedByPrincipalId,
            workProduct.CreatedAtUtc,
            null,
            null);
    }

    public WorkProductRevision SubmitForReview(PrincipalId actorPrincipalId, DateTimeOffset submittedAtUtc)
    {
        if (State != WorkProductRevisionState.Draft)
        {
            throw new InvalidOperationException($"Only a Draft revision can be submitted for review; current state is {State}.");
        }

        if (submittedAtUtc == default) throw new ArgumentException("Submission timestamp is required.", nameof(submittedAtUtc));

        return this with
        {
            State = WorkProductRevisionState.InReview,
            SubmittedByPrincipalId = actorPrincipalId,
            SubmittedAtUtc = submittedAtUtc.ToUniversalTime()
        };
    }

    public static WorkProductRevision Restore(
        WorkProductRevisionId id,
        TenantId tenantId,
        WorkProductId workProductId,
        int revisionNumber,
        string titleSnapshot,
        WorkProductRevisionState state,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc,
        PrincipalId? submittedByPrincipalId = null,
        DateTimeOffset? submittedAtUtc = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(revisionNumber);
        if (string.IsNullOrWhiteSpace(titleSnapshot)) throw new ArgumentException("Revision title cannot be empty.", nameof(titleSnapshot));
        if ((submittedByPrincipalId is null) != (submittedAtUtc is null))
        {
            throw new ArgumentException("Submission actor and timestamp must either both be present or both be absent.");
        }
        if (state != WorkProductRevisionState.Draft && submittedAtUtc is null)
        {
            throw new ArgumentException("A non-Draft revision requires submission evidence.", nameof(submittedAtUtc));
        }

        return new WorkProductRevision(
            id,
            tenantId,
            workProductId,
            revisionNumber,
            titleSnapshot.Trim(),
            state,
            createdByPrincipalId,
            createdAtUtc.ToUniversalTime(),
            submittedByPrincipalId,
            submittedAtUtc?.ToUniversalTime());
    }
}

public sealed record ReviewRequest
{
    private ReviewRequest(
        ReviewRequestId id,
        TenantId tenantId,
        WorkProductRevisionId workProductRevisionId,
        ReviewRequestKind kind,
        PrincipalId requestedPrincipalId,
        PrincipalId requestedByPrincipalId,
        DateTimeOffset requestedAtUtc,
        ReviewRequestState state)
    {
        Id = id;
        TenantId = tenantId;
        WorkProductRevisionId = workProductRevisionId;
        Kind = kind;
        RequestedPrincipalId = requestedPrincipalId;
        RequestedByPrincipalId = requestedByPrincipalId;
        RequestedAtUtc = requestedAtUtc;
        State = state;
    }

    public ReviewRequestId Id { get; }
    public TenantId TenantId { get; }
    public WorkProductRevisionId WorkProductRevisionId { get; }
    public ReviewRequestKind Kind { get; }
    public PrincipalId RequestedPrincipalId { get; }
    public PrincipalId RequestedByPrincipalId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public ReviewRequestState State { get; }

    public static ReviewRequest CreateReview(
        WorkProductRevision revision,
        PrincipalId reviewerPrincipalId,
        PrincipalId requestedByPrincipalId,
        DateTimeOffset requestedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(revision);
        if (revision.State != WorkProductRevisionState.InReview)
        {
            throw new InvalidOperationException("A review request can only be created for a revision that is InReview.");
        }
        if (requestedAtUtc == default) throw new ArgumentException("Request timestamp is required.", nameof(requestedAtUtc));

        return new ReviewRequest(
            ReviewRequestId.New(),
            revision.TenantId,
            revision.Id,
            ReviewRequestKind.Review,
            reviewerPrincipalId,
            requestedByPrincipalId,
            requestedAtUtc.ToUniversalTime(),
            ReviewRequestState.Open);
    }
}
