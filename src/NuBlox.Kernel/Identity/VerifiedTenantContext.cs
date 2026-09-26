namespace NuBlox.Kernel.Identity;

/// <summary>
/// Server-verified working tenant context. The internal tenant identifier is authoritative;
/// the slug is retained only as resolved external addressing metadata.
/// </summary>
public sealed record VerifiedTenantContext
{
    public VerifiedTenantContext(TenantId tenantId, string tenantSlug)
    {
        if (tenantId.Value == Guid.Empty)
        {
            throw new ArgumentException("Tenant identifier cannot be empty.", nameof(tenantId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tenantSlug);

        TenantId = tenantId;
        TenantSlug = tenantSlug.Trim();
    }

    public TenantId TenantId { get; }

    public string TenantSlug { get; }
}
