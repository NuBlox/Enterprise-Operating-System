namespace NuBlox.Kernel.Identity;

public interface ITenantParticipationAuthorizer
{
    ValueTask<bool> CanAccessTenantAsync(
        PrincipalId principalId,
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}
