using System.Data.Common;

namespace NuBlox.FoundationSpike.Shared;

public interface ITransactionalSession : IAsyncDisposable
{
    DbConnection Connection { get; }
    DbTransaction Transaction { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

// Spike/admin harness only. Application use cases should prefer the customer-scoped factory.
public interface ITransactionalSessionFactory
{
    Task<ITransactionalSession> OpenAsync(CancellationToken cancellationToken = default);
}

public interface ICustomerScopedTransactionalSessionFactory
{
    Task<ITransactionalSession> OpenAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default);
}

public static class DbCommandExtensions
{
    public static DbParameter AddParameter(this DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
        return parameter;
    }
}
