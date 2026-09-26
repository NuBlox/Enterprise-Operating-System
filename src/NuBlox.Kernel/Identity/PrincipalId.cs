namespace NuBlox.Kernel.Identity;

public readonly record struct PrincipalId
{
    public PrincipalId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Principal ID cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public override string ToString() => Value.ToString("D");
}
