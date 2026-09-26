using NuBlox.FoundationSpike.Shared;
using Npgsql;

namespace NuBlox.FoundationSpike.Infrastructure;

public sealed class PostgresTransactionalSessionFactory(string connectionString) : ITransactionalSessionFactory
{
    public async Task<ITransactionalSession> OpenAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        return new PostgresTransactionalSession(connection, transaction);
    }
}

public sealed class PostgresCustomerScopedTransactionalSessionFactory(
    string connectionString) : ICustomerScopedTransactionalSessionFactory
{
    public async Task<ITransactionalSession> OpenAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var session = new PostgresTransactionalSession(connection, transaction);

        try
        {
            await using (var roleCommand = connection.CreateCommand())
            {
                roleCommand.Transaction = transaction;
                roleCommand.CommandText = "SET LOCAL ROLE nublox_app;";
                await roleCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var contextCommand = connection.CreateCommand())
            {
                contextCommand.Transaction = transaction;
                contextCommand.CommandText = "SELECT set_config('app.customer_id', @customer_id, true);";
                contextCommand.Parameters.AddWithValue("customer_id", customerId.Value.ToString());
                await contextCommand.ExecuteScalarAsync(cancellationToken);
            }

            return session;
        }
        catch
        {
            await session.DisposeAsync();
            throw;
        }
    }
}

internal sealed class PostgresTransactionalSession(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction) : ITransactionalSession
{
    private bool _completed;

    public System.Data.Common.DbConnection Connection => connection;
    public System.Data.Common.DbTransaction Transaction => transaction;

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
        {
            throw new InvalidOperationException("The transaction has already completed.");
        }

        await transaction.CommitAsync(cancellationToken);
        _completed = true;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
        {
            return;
        }

        await transaction.RollbackAsync(cancellationToken);
        _completed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_completed)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _completed = true;
        }

        await transaction.DisposeAsync();
        await connection.DisposeAsync();
    }
}
