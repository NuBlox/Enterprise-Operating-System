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
