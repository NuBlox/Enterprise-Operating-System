namespace NuBlox.Kernel.Identity;

/// <summary>
/// Stable internal NuBlox tenant identifier. External slugs are never used as isolation keys.
/// </summary>
public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}
