using Npgsql;

namespace NuBlox.Persistence.PostgreSql;

public sealed class PostgresMigrationRunner
{
    private const long MigrationLockKey = 732194871642013551;
    private readonly NpgsqlDataSource _dataSource;
    private readonly IReadOnlyList<PostgresMigration> _migrations;

    public PostgresMigrationRunner(NpgsqlDataSource dataSource, IReadOnlyList<PostgresMigration>? migrations = null)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _migrations = migrations ?? PostgresMigrationCatalog.Load();
    }

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await ExecuteNonQueryAsync(connection, transaction, $"SELECT pg_advisory_xact_lock({MigrationLockKey});", cancellationToken);
        await EnsureJournalAsync(connection, transaction, cancellationToken);

        var applied = await LoadAppliedAsync(connection, transaction, cancellationToken);

        foreach (var migration in _migrations)
        {
            if (applied.TryGetValue(migration.Id, out var appliedChecksum))
            {
                if (!string.Equals(appliedChecksum, migration.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Migration '{migration.Id}' checksum mismatch. Applied={appliedChecksum}, current={migration.Sha256}.");
                }

                continue;
            }

            await ExecuteNonQueryAsync(connection, transaction, migration.Sql, cancellationToken);

            await using var insert = new NpgsqlCommand(
                """
                INSERT INTO nublox_meta.schema_migrations (migration_id, sha256)
                VALUES (@migration_id, @sha256);
                """,
                connection,
                transaction);
            insert.Parameters.AddWithValue("migration_id", migration.Id);
            insert.Parameters.AddWithValue("sha256", migration.Sha256);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task EnsureJournalAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE SCHEMA IF NOT EXISTS nublox_meta;

            CREATE TABLE IF NOT EXISTS nublox_meta.schema_migrations (
                migration_id text PRIMARY KEY,
                sha256 text NOT NULL CHECK (length(sha256) = 64),
                applied_at timestamptz NOT NULL DEFAULT now()
            );
            """;

        await ExecuteNonQueryAsync(connection, transaction, sql, cancellationToken);
    }

    private static async Task<Dictionary<string, string>> LoadAppliedAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "SELECT migration_id, sha256 FROM nublox_meta.schema_migrations ORDER BY migration_id;",
            connection,
            transaction);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var applied = new Dictionary<string, string>(StringComparer.Ordinal);
        while (await reader.ReadAsync(cancellationToken))
        {
            applied.Add(reader.GetString(0), reader.GetString(1));
        }

        return applied;
    }

    private static async Task ExecuteNonQueryAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
