namespace NuBlox.Kernel.Identity;

/// <summary>
/// Resolves an external tenant route into a server-verified working tenant context.
/// The requested slug never grants access by itself.
/// </summary>
public sealed class TenantContextResolver
{
    private readonly ITenantDirectory _tenantDirectory;
    private readonly ITenantParticipationAuthorizer _participationAuthorizer;

    public TenantContextResolver(
        ITenantDirectory tenantDirectory,
        ITenantParticipationAuthorizer participationAuthorizer)
    {
        _tenantDirectory = tenantDirectory ?? throw new ArgumentNullException(nameof(tenantDirectory));
        _participationAuthorizer = participationAuthorizer ?? throw new ArgumentNullException(nameof(participationAuthorizer));
    }

    public async ValueTask<TenantContextResolution> ResolveAsync(
        AuthenticatedPrincipal principal,
        string? requestedTenantSlug,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (!principal.IsEnabled)
        {
            return TenantContextResolution.Failure(TenantContextResolutionStatus.PrincipalDisabled);
        }

        if (string.IsNullOrWhiteSpace(requestedTenantSlug))
        {
            return TenantContextResolution.Failure(TenantContextResolutionStatus.MissingTenant);
        }

        var normalizedSlug = requestedTenantSlug.Trim();
        var tenantId = await _tenantDirectory.ResolveTenantIdAsync(normalizedSlug, cancellationToken);

        if (tenantId is null || tenantId.Value.Value == Guid.Empty)
        {
            return TenantContextResolution.Failure(TenantContextResolutionStatus.TenantNotFound);
        }

        var authorised = await _participationAuthorizer.CanAccessTenantAsync(
            principal.PrincipalId,
            tenantId.Value,
            cancellationToken);

        if (!authorised)
        {
            return TenantContextResolution.Failure(TenantContextResolutionStatus.AccessDenied);
        }

        return TenantContextResolution.Success(new VerifiedTenantContext(tenantId.Value, normalizedSlug));
    }
}
