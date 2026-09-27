using NuBlox.Enterprise;
using NuBlox.Enterprise.Application;
using NuBlox.Kernel;

namespace NuBlox.Runtime.PostgreSql;

/// <summary>
/// Bounded enterprise administration policy. It is deliberately configuration-backed and deny-by-default
/// until a richer governed responsibility/authority model is introduced.
/// </summary>
public sealed class ConfiguredOrganisationAccessEvaluator : IOrganisationAccessEvaluator
{
    private readonly HashSet<PrincipalId> _administrators;

    public ConfiguredOrganisationAccessEvaluator(IEnumerable<PrincipalId> administrators)
    {
        ArgumentNullException.ThrowIfNull(administrators);
        _administrators = [.. administrators];
    }

    public ValueTask<bool> CanCreateAsync(
        TenantId tenantId,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(_administrators.Contains(actorPrincipalId));

    public ValueTask<bool> CanReadAsync(
        Organisation organisation,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(organisation);
        return ValueTask.FromResult(_administrators.Contains(actorPrincipalId));
    }
}

/// <summary>
/// Bounded Person administration policy. Person is enterprise identity, not application identity;
/// a Principal may administer a Person without being that Person and a Person may exist without a Principal.
/// </summary>
public sealed class ConfiguredPersonAccessEvaluator : IPersonAccessEvaluator
{
    private readonly HashSet<PrincipalId> _administrators;

    public ConfiguredPersonAccessEvaluator(IEnumerable<PrincipalId> administrators)
    {
        ArgumentNullException.ThrowIfNull(administrators);
        _administrators = [.. administrators];
    }

    public ValueTask<bool> CanCreateAsync(
        TenantId tenantId,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(_administrators.Contains(actorPrincipalId));

    public ValueTask<bool> CanReadAsync(
        Person person,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);
        return ValueTask.FromResult(_administrators.Contains(actorPrincipalId));
    }
}
