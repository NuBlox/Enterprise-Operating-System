using NuBlox.Enterprise;
using NuBlox.Enterprise.Application;
using NuBlox.Kernel;

namespace NuBlox.Runtime.PostgreSql;

/// <summary>
/// First-slice enterprise administration policy. It is deliberately configuration-backed and deny-by-default
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
