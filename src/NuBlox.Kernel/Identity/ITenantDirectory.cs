namespace NuBlox.Kernel.Identity;

public interface ITenantDirectory
{
    ValueTask<TenantId?> ResolveTenantIdAsync(
        string tenantSlug,
        CancellationToken cancellationToken = default);
}
