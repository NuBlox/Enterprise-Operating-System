using NuBlox.Kernel;

namespace NuBlox.Enterprise;

/// <summary>
/// Stable enterprise Party identifier. Party identity is distinct from Tenant, Principal and contextual business roles.
/// </summary>
public readonly record struct PartyId
{
    public PartyId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Party identifiers cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PartyId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}

public enum PartyKind
{
    Person = 1,
    Organisation = 2
}

/// <summary>
/// Canonical Person Party. Person identity is distinct from Principal/User Account, Work Relationship and Position.
/// </summary>
public sealed record Person
{
    private Person(
        PartyId id,
        TenantId tenantId,
        string displayName,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        Kind = PartyKind.Person;
        DisplayName = displayName;
        CreatedByPrincipalId = createdByPrincipalId;
        CreatedAtUtc = createdAtUtc;
    }

    public PartyId Id { get; }

    public TenantId TenantId { get; }

    public PartyKind Kind { get; }

    public string DisplayName { get; }

    public PrincipalId CreatedByPrincipalId { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public static Person Create(
        TenantId tenantId,
        string displayName,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc)
    {
        if (createdAtUtc == default)
        {
            throw new ArgumentException("Creation timestamp is required.", nameof(createdAtUtc));
        }

        return new Person(
            PartyId.New(),
            tenantId,
            RequireDisplayName(displayName),
            createdByPrincipalId,
            createdAtUtc.ToUniversalTime());
    }

    public static Person Restore(
        PartyId id,
        TenantId tenantId,
        string displayName,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc) =>
        new(
            id,
            tenantId,
            RequireDisplayName(displayName),
            createdByPrincipalId,
            createdAtUtc.ToUniversalTime());

    private static string RequireDisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Person display name cannot be empty.", nameof(value));
        }

        var canonical = value.Trim();
        if (canonical.Length > 240)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Person display name cannot exceed 240 characters.");
        }

        return canonical;
    }
}

/// <summary>
/// Canonical Organisation Party. Client/customer/vendor/supplier are contextual relationship roles and are not encoded here.
/// </summary>
public sealed record Organisation
{
    private Organisation(
        PartyId id,
        TenantId tenantId,
        string displayName,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        Kind = PartyKind.Organisation;
        DisplayName = displayName;
        CreatedByPrincipalId = createdByPrincipalId;
        CreatedAtUtc = createdAtUtc;
    }

    public PartyId Id { get; }

    public TenantId TenantId { get; }

    public PartyKind Kind { get; }

    public string DisplayName { get; }

    public PrincipalId CreatedByPrincipalId { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public static Organisation Create(
        TenantId tenantId,
        string displayName,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc)
    {
        if (createdAtUtc == default)
        {
            throw new ArgumentException("Creation timestamp is required.", nameof(createdAtUtc));
        }

        return new Organisation(
            PartyId.New(),
            tenantId,
            RequireDisplayName(displayName),
            createdByPrincipalId,
            createdAtUtc.ToUniversalTime());
    }

    public static Organisation Restore(
        PartyId id,
        TenantId tenantId,
        string displayName,
        PrincipalId createdByPrincipalId,
        DateTimeOffset createdAtUtc) =>
        new(
            id,
            tenantId,
            RequireDisplayName(displayName),
            createdByPrincipalId,
            createdAtUtc.ToUniversalTime());

    private static string RequireDisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Organisation display name cannot be empty.", nameof(value));
        }

        var canonical = value.Trim();
        if (canonical.Length > 240)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Organisation display name cannot exceed 240 characters.");
        }

        return canonical;
    }
}
