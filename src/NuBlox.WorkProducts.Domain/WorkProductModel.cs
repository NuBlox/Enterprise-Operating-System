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

    internal static WorkProduct Rehydrate(
        WorkProductId id,
        TenantId tenantId,
        string title,
        string productType,
        PrincipalId ownerPrincipalId,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc,
        WorkProductLifecycle lifecycle,
        int currentRevisionNumber) =>
        new(id, tenantId, title, productType, ownerPrincipalId, createdByPrincipalId, createdAtUtc, lifecycle, currentRevisionNumber);

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
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        WorkProductId = workProductId;
        RevisionNumber = revisionNumber;
        TitleSnapshot = titleSnapshot;
        State = state;
        CreatedByPrincipalId = createdByPrincipalId;
        CreatedAtUtc = createdAtUtc;
    }

    public WorkProductRevisionId Id { get; }
    public TenantId TenantId { get; }
    public WorkProductId WorkProductId { get; }
    public int RevisionNumber { get; }
    public string TitleSnapshot { get; }
    public WorkProductRevisionState State { get; }
    public PrincipalId CreatedByPrincipalId { get; }
    public DateTimeOffset CreatedAtUtc { get; }

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
            workProduct.CreatedAtUtc);
    }

    internal static WorkProductRevision Rehydrate(
        WorkProductRevisionId id,
        TenantId tenantId,
        WorkProductId workProductId,
        int revisionNumber,
        string titleSnapshot,
        WorkProductRevisionState state,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc) =>
        new(id, tenantId, workProductId, revisionNumber, titleSnapshot, state, createdByPrincipalId, createdAtUtc);
}
