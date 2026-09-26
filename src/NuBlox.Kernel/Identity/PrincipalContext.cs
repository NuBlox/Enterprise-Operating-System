namespace NuBlox.Kernel.Identity;

public enum PrincipalKind
{
    Human = 1,
    Service = 2
}

public sealed record PrincipalContext(
    PrincipalId PrincipalId,
    PrincipalKind Kind,
    string Issuer,
    string Subject)
{
    public PrincipalContext
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(Subject);
    }
}

public sealed record RequestContext(PrincipalContext Principal, TenantId TenantId);
