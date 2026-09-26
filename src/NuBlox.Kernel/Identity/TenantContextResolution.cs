namespace NuBlox.Kernel.Identity;

public enum TenantContextResolutionStatus
{
    Success = 1,
    MissingTenant = 2,
    TenantNotFound = 3,
    PrincipalDisabled = 4,
    AccessDenied = 5
}

public sealed record TenantContextResolution
{
    private TenantContextResolution(
        TenantContextResolutionStatus status,
        VerifiedTenantContext? context)
    {
        Status = status;
        Context = context;
    }

    public TenantContextResolutionStatus Status { get; }

    public VerifiedTenantContext? Context { get; }

    public bool IsSuccess => Status == TenantContextResolutionStatus.Success;

    public static TenantContextResolution Success(VerifiedTenantContext context) =>
        new(TenantContextResolutionStatus.Success, context ?? throw new ArgumentNullException(nameof(context)));

    public static TenantContextResolution Failure(TenantContextResolutionStatus status)
    {
        if (status == TenantContextResolutionStatus.Success)
        {
            throw new ArgumentException("Use Success for a successful resolution.", nameof(status));
        }

        return new(status, null);
    }
}
