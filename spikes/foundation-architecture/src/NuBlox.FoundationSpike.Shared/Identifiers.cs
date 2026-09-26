namespace NuBlox.FoundationSpike.Shared;

public readonly record struct CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct SubjectId(Guid Value)
{
    public static SubjectId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct WorkId(Guid Value)
{
    public static WorkId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct DecisionId(Guid Value)
{
    public static DecisionId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct AuditEventId(Guid Value)
{
    public static AuditEventId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
