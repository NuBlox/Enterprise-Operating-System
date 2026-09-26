using System.Security.Claims;

namespace NuBlox.Kernel.Identity;

public static class PrincipalContextResolver
{
    public static bool TryResolve(ClaimsPrincipal claimsPrincipal, out PrincipalContext? principalContext)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);

        principalContext = null;

        if (claimsPrincipal.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        string? principalIdValue = claimsPrincipal.FindFirstValue(NuBloxClaimTypes.PrincipalId);
        string? principalKindValue = claimsPrincipal.FindFirstValue(NuBloxClaimTypes.PrincipalKind);
        string? issuer = claimsPrincipal.FindFirstValue(NuBloxClaimTypes.Issuer);
        string? subject = claimsPrincipal.FindFirstValue(NuBloxClaimTypes.Subject);

        if (!Guid.TryParse(principalIdValue, out Guid principalId)
            || principalId == Guid.Empty
            || !Enum.TryParse(principalKindValue, ignoreCase: true, out PrincipalKind principalKind)
            || !Enum.IsDefined(principalKind)
            || string.IsNullOrWhiteSpace(issuer)
            || string.IsNullOrWhiteSpace(subject))
        {
            return false;
        }

        principalContext = new PrincipalContext(
            new PrincipalId(principalId),
            principalKind,
            issuer,
            subject);

        return true;
    }

    public static RequestContext CreateForVerifiedTenant(PrincipalContext principalContext, TenantId verifiedTenantId)
    {
        ArgumentNullException.ThrowIfNull(principalContext);
        return new RequestContext(principalContext, verifiedTenantId);
    }
}
