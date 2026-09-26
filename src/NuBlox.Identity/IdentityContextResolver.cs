using NuBlox.Kernel;

namespace NuBlox.Identity;

public sealed class AuthenticationRequiredException : UnauthorizedAccessException
{
    public AuthenticationRequiredException()
        : base("An authenticated NuBlox principal is required for this operation.")
    {
    }
}

public sealed class PrincipalDisabledException : UnauthorizedAccessException
{
    public PrincipalDisabledException()
        : base("The authenticated NuBlox principal is not enabled for application access.")
    {
    }
}

public sealed class TenantContextDeniedException : UnauthorizedAccessException
{
    public TenantContextDeniedException()
        : base("The requested tenant context could not be established for this principal.")
    {
    }

    public TenantContextDeniedException(Exception innerException)
        : base("The requested tenant context could not be established for this principal.", innerException)
    {
    }
}

/// <summary>
/// Resolves an authenticated principal and an untrusted tenant route into the verified server-side context
/// required by protected application operations.
/// </summary>
public sealed class IdentityContextResolver
{
    private readonly ITenantDirectory _tenantDirectory;
    private readonly IPrincipalTenantAccessEvaluator _accessEvaluator;

    public IdentityContextResolver(
        ITenantDirectory tenantDirectory,
        IPrincipalTenantAccessEvaluator accessEvaluator)
    {
        _tenantDirectory = tenantDirectory ?? throw new ArgumentNullException(nameof(tenantDirectory));
        _accessEvaluator = accessEvaluator ?? throw new ArgumentNullException(nameof(accessEvaluator));
    }

    public async ValueTask<AuthenticatedRequestContext> ResolveAsync(
        AuthenticatedPrincipal? principal,
        string requestedTenantSlug,
        CancellationToken cancellationToken = default)
    {
        if (principal is null)
        {
            throw new AuthenticationRequiredException();
        }

        if (!principal.IsEnabled)
        {
            throw new PrincipalDisabledException();
        }

        TenantRouteSlug routeSlug;
        try
        {
            routeSlug = new TenantRouteSlug(requestedTenantSlug);
        }
        catch (ArgumentException exception)
        {
            throw new TenantContextDeniedException(exception);
        }

        var tenant = await _tenantDirectory
            .FindBySlugAsync(routeSlug, cancellationToken)
            .ConfigureAwait(false);

        if (tenant is null || !tenant.IsEnabled)
        {
            throw new TenantContextDeniedException();
        }

        var hasAccess = await _accessEvaluator
            .HasAccessAsync(principal.PrincipalId, tenant.TenantId, cancellationToken)
            .ConfigureAwait(false);

        if (!hasAccess)
        {
            throw new TenantContextDeniedException();
        }

        var verifiedTenant = new VerifiedTenantContext(tenant.TenantId, tenant.CanonicalSlug);
        return new AuthenticatedRequestContext(principal, verifiedTenant);
    }
}
