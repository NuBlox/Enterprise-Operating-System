namespace NuBlox.Kernel;

/// <summary>
/// Stable NuBlox application-principal identifier independent of any external identity provider.
/// </summary>
public readonly record struct PrincipalId
{
    public PrincipalId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Principal identifiers cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PrincipalId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}

/// <summary>
/// Stable internal tenant identifier used for isolation and authorised working context.
/// Route slugs are external addressing values and are not substitutes for this identifier.
/// </summary>
public readonly record struct TenantId
{
    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Tenant identifiers cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static TenantId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}
