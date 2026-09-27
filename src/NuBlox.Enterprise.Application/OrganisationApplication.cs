using NuBlox.Kernel;

namespace NuBlox.Enterprise.Application;

public sealed record CreateOrganisationCommand(
    TenantId TenantId,
    PrincipalId ActorPrincipalId,
    string DisplayName);

public sealed record OrganisationRecord(Organisation Organisation);

public interface IOrganisationRepository
{
    Task AddAsync(Organisation organisation, CancellationToken cancellationToken = default);

    Task<Organisation?> FindAsync(
        TenantId tenantId,
        PartyId partyId,
        CancellationToken cancellationToken = default);
}

public interface IOrganisationAccessEvaluator
{
    ValueTask<bool> CanCreateAsync(
        TenantId tenantId,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default);

    ValueTask<bool> CanReadAsync(
        Organisation organisation,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default);
}

public sealed class OrganisationAccessDeniedException : Exception
{
    public OrganisationAccessDeniedException()
        : base("The Principal is not permitted to perform this Organisation operation.")
    {
    }
}

public interface IEnterpriseClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemEnterpriseClock : IEnterpriseClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class OrganisationApplicationService
{
    private readonly IOrganisationRepository _repository;
    private readonly IOrganisationAccessEvaluator _accessEvaluator;
    private readonly IEnterpriseClock _clock;

    public OrganisationApplicationService(
        IOrganisationRepository repository,
        IOrganisationAccessEvaluator accessEvaluator,
        IEnterpriseClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _accessEvaluator = accessEvaluator ?? throw new ArgumentNullException(nameof(accessEvaluator));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<OrganisationRecord> CreateAsync(
        CreateOrganisationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!await _accessEvaluator.CanCreateAsync(
                command.TenantId,
                command.ActorPrincipalId,
                cancellationToken).ConfigureAwait(false))
        {
            throw new OrganisationAccessDeniedException();
        }

        var organisation = Organisation.Create(
            command.TenantId,
            command.DisplayName,
            command.ActorPrincipalId,
            _clock.UtcNow);

        await _repository.AddAsync(organisation, cancellationToken).ConfigureAwait(false);
        return new OrganisationRecord(organisation);
    }

    public async Task<OrganisationRecord?> FindAsync(
        TenantId tenantId,
        PartyId partyId,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        var organisation = await _repository.FindAsync(tenantId, partyId, cancellationToken).ConfigureAwait(false);
        if (organisation is null)
        {
            return null;
        }

        if (!await _accessEvaluator.CanReadAsync(
                organisation,
                actorPrincipalId,
                cancellationToken).ConfigureAwait(false))
        {
            throw new OrganisationAccessDeniedException();
        }

        return new OrganisationRecord(organisation);
    }
}
