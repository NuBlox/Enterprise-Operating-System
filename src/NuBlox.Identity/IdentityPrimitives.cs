using System.Text.RegularExpressions;
using NuBlox.Kernel;

namespace NuBlox.Identity;

/// <summary>
/// Application-principal kind. External participants remain human principals; their business relationship
/// and access scope are modelled separately from authentication identity.
/// </summary>
public enum PrincipalKind
{
    Human = 1,
    Service = 2
}

/// <summary>
/// Trusted external authentication identity keyed by issuer and subject.
/// </summary>
public sealed record ExternalIdentity
{
    public ExternalIdentity(string issuer, string subject)
    {
        Issuer = RequireValue(issuer, nameof(issuer));
        Subject = RequireValue(subject, nameof(subject));
    }

    public string Issuer { get; }

    public string Subject { get; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Identity values cannot be empty.", parameterName);
        }

        return value.Trim();
    }
}

/// <summary>
/// Normalised NuBlox principal produced only after an external authentication identity has been validated
/// and linked by the authentication boundary.
/// </summary>
public sealed class AuthenticatedPrincipal
{
    private AuthenticatedPrincipal(
        PrincipalId principalId,
        PrincipalKind kind,
        ExternalIdentity externalIdentity,
        bool isEnabled)
    {
        PrincipalId = principalId;
        Kind = kind;
        ExternalIdentity = externalIdentity ?? throw new ArgumentNullException(nameof(externalIdentity));
        IsEnabled = isEnabled;
    }

    public PrincipalId PrincipalId { get; }

    public PrincipalKind Kind { get; }

    public ExternalIdentity ExternalIdentity { get; }

    public bool IsEnabled { get; }

    public static AuthenticatedPrincipal CreateHuman(
        PrincipalId principalId,
        ExternalIdentity externalIdentity,
        bool isEnabled = true) =>
        new(principalId, PrincipalKind.Human, externalIdentity, isEnabled);

    public static AuthenticatedPrincipal CreateService(
        PrincipalId principalId,
        ExternalIdentity externalIdentity,
        bool isEnabled = true) =>
        new(principalId, PrincipalKind.Service, externalIdentity, isEnabled);
}

/// <summary>
/// External routing/discovery slug. It is never itself proof of tenant access.
/// </summary>
public readonly record struct TenantRouteSlug
{
    private static readonly Regex Pattern = new(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public TenantRouteSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tenant route slug cannot be empty.", nameof(value));
        }

        var normalised = value.Trim().ToLowerInvariant();
        if (!Pattern.IsMatch(normalised))
        {
            throw new ArgumentException("Tenant route slug is not in the canonical lower-case route format.", nameof(value));
        }

        Value = normalised;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

/// <summary>
/// Trusted tenant directory result. The route slug has already been resolved to the stable internal TenantId.
/// </summary>
public sealed record TenantDirectoryEntry(
    TenantId TenantId,
    TenantRouteSlug CanonicalSlug,
    bool IsEnabled);

public interface ITenantDirectory
{
    ValueTask<TenantDirectoryEntry?> FindBySlugAsync(
        TenantRouteSlug routeSlug,
        CancellationToken cancellationToken = default);
}

public interface IPrincipalTenantAccessEvaluator
{
    ValueTask<bool> HasAccessAsync(
        PrincipalId principalId,
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Verified server-side tenant context. The constructor is internal so callers outside the identity boundary
/// cannot manufacture a trusted context from an arbitrary tenant identifier or route value.
/// </summary>
public sealed class VerifiedTenantContext
{
    internal VerifiedTenantContext(TenantId tenantId, TenantRouteSlug canonicalSlug)
    {
        TenantId = tenantId;
        CanonicalSlug = canonicalSlug;
    }

    public TenantId TenantId { get; }

    public TenantRouteSlug CanonicalSlug { get; }
}

/// <summary>
/// Normalised context supplied to protected application operations after authentication and tenant access checks.
/// </summary>
public sealed class AuthenticatedRequestContext
{
    internal AuthenticatedRequestContext(
        AuthenticatedPrincipal principal,
        VerifiedTenantContext tenant)
    {
        Principal = principal;
        Tenant = tenant;
    }

    public AuthenticatedPrincipal Principal { get; }

    public VerifiedTenantContext Tenant { get; }
}
