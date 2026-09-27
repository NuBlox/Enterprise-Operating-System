using NuBlox.Kernel;

namespace NuBlox.Enterprise.Application;

public sealed record CreatePersonCommand(
    TenantId TenantId,
    PrincipalId ActorPrincipalId,
    string DisplayName);

public sealed record PersonRecord(Person Person);

public interface IPersonRepository
{
    Task AddAsync(Person person, CancellationToken cancellationToken = default);

    Task<Person?> FindAsync(
        TenantId tenantId,
        PartyId partyId,
        CancellationToken cancellationToken = default);
}

public interface IPersonAccessEvaluator
{
    ValueTask<bool> CanCreateAsync(
        TenantId tenantId,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default);

    ValueTask<bool> CanReadAsync(
        Person person,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default);
}

public sealed class PersonAccessDeniedException : Exception
{
    public PersonAccessDeniedException()
        : base("The Principal is not permitted to perform this Person operation.")
    {
    }
}

public sealed class PersonApplicationService
{
    private readonly IPersonRepository _repository;
    private readonly IPersonAccessEvaluator _accessEvaluator;
    private readonly IEnterpriseClock _clock;

    public PersonApplicationService(
        IPersonRepository repository,
        IPersonAccessEvaluator accessEvaluator,
        IEnterpriseClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _accessEvaluator = accessEvaluator ?? throw new ArgumentNullException(nameof(accessEvaluator));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<PersonRecord> CreateAsync(
        CreatePersonCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!await _accessEvaluator.CanCreateAsync(
                command.TenantId,
                command.ActorPrincipalId,
                cancellationToken).ConfigureAwait(false))
        {
            throw new PersonAccessDeniedException();
        }

        var person = Person.Create(
            command.TenantId,
            command.DisplayName,
            command.ActorPrincipalId,
            _clock.UtcNow);

        await _repository.AddAsync(person, cancellationToken).ConfigureAwait(false);
        return new PersonRecord(person);
    }

    public async Task<PersonRecord?> FindAsync(
        TenantId tenantId,
        PartyId partyId,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        var person = await _repository.FindAsync(tenantId, partyId, cancellationToken).ConfigureAwait(false);
        if (person is null)
        {
            return null;
        }

        if (!await _accessEvaluator.CanReadAsync(
                person,
                actorPrincipalId,
                cancellationToken).ConfigureAwait(false))
        {
            throw new PersonAccessDeniedException();
        }

        return new PersonRecord(person);
    }
}
