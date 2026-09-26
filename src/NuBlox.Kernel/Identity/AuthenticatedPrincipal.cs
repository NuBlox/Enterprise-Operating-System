namespace NuBlox.Kernel.Identity;

/// <summary>
/// Normalised application identity after external authentication has been validated.
/// This type intentionally contains no provider-specific claims or canonical Person data.
/// </summary>
public sealed record AuthenticatedPrincipal
{
    public AuthenticatedPrincipal(
        PrincipalId principalId,
        PrincipalKind kind,
        ExternalIdentityBinding externalIdentity,
        bool isEnabled = true)
    {
        if (principalId.Value == Guid.Empty)
        {
            throw new ArgumentException("Principal identifier cannot be empty.", nameof(principalId));
        }

        PrincipalId = principalId;
        Kind = kind;
        ExternalIdentity = externalIdentity ?? throw new ArgumentNullException(nameof(externalIdentity));
        IsEnabled = isEnabled;
    }

    public PrincipalId PrincipalId { get; }

    public PrincipalKind Kind { get; }

    public ExternalIdentityBinding ExternalIdentity { get; }

    public bool IsEnabled { get; }
}
