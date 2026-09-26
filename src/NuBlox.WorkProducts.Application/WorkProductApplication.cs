using NuBlox.Kernel;

namespace NuBlox.WorkProducts.Application;

public sealed record CreateWorkProductCommand(
    TenantId TenantId,
    PrincipalId ActorPrincipalId,
    string Title,
    string ProductType,
    PrincipalId? OwnerPrincipalId = null);

public sealed record WorkProductRecord(WorkProduct WorkProduct, WorkProductRevision CurrentRevision);

public interface IWorkProductRepository
{
    Task AddAsync(WorkProductRecord record, CancellationToken cancellationToken = default);

    Task<WorkProductRecord?> FindAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default);
}

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class WorkProductApplicationService
{
    private readonly IWorkProductRepository _repository;
    private readonly ISystemClock _clock;

    public WorkProductApplicationService(IWorkProductRepository repository, ISystemClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<WorkProductRecord> CreateAsync(
        CreateWorkProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workProduct = WorkProduct.Create(
            command.TenantId,
            command.Title,
            command.ProductType,
            command.OwnerPrincipalId ?? command.ActorPrincipalId,
            command.ActorPrincipalId,
            _clock.UtcNow);
        var revision = WorkProductRevision.CreateInitial(workProduct);
        var record = new WorkProductRecord(workProduct, revision);

        await _repository.AddAsync(record, cancellationToken).ConfigureAwait(false);
        return record;
    }

    public Task<WorkProductRecord?> FindAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default) =>
        _repository.FindAsync(tenantId, workProductId, cancellationToken);
}
