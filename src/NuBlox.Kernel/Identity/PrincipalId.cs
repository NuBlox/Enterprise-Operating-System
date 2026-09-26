namespace NuBlox.Kernel.Identity;

/// <summary>
/// Stable internal NuBlox application-principal identifier.
/// </summary>
public readonly record struct PrincipalId(Guid Value)
{
    public static PrincipalId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}
