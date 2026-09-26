using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace NuBlox.Persistence.Postgres;

public sealed class PostgresMigrationRunner
{
    private const long MigrationLockKey = 1_314_269_753;
    private readonly string _connectionString;

    public PostgresMigrationRunner(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<MigrationResult>> ApplyAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(_connectionString);
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);

        await BootstrapJournalAsync(connection, cancellationToken);
        await AcquireMigrationLockAsync(connection, cancellationToken);

        try
        {
            var results = new List<MigrationResult>();

            foreach (EmbeddedMigration migration in LoadEmbeddedMigrations())
            {
                string? existingChecksum = await GetExistingChecksumAsync(
                    connection,
                    migration.Id,
                    cancellationToken);

                if (existingChecksum is not null)
                {
                    if (!string.Equals(existingChecksum, migration.ChecksumSha256, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"Migration '{migration.Id}' was already applied with a different checksum.");
                    }

                    results.Add(new MigrationResult(migration.Id, MigrationState.AlreadyApplied));
                    continue;
                }

                await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

                await using (NpgsqlCommand migrationCommand = new(migration.Sql, connection, transaction))
                {
                    await migrationCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await using (NpgsqlCommand journalCommand = new(
                    """
                    INSERT INTO nublox_platform.schema_migrations
                        (migration_id, checksum_sha256, applied_at_utc)
                    VALUES
                        (@migration_id, @checksum_sha256, now());
                    """,
                    connection,
                    transaction))
                {
                    journalCommand.Parameters.AddWithValue("migration_id", migration.Id);
                    journalCommand.Parameters.AddWithValue("checksum_sha256", migration.ChecksumSha256);
                    await journalCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                results.Add(new MigrationResult(migration.Id, MigrationState.Applied));
            }

            return results;
        }
        finally
        {
            await ReleaseMigrationLockAsync(connection, cancellationToken);
        }
    }

    private static async Task BootstrapJournalAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE SCHEMA IF NOT EXISTS nublox_platform;

            CREATE TABLE IF NOT EXISTS nublox_platform.schema_migrations
            (
                migration_id text PRIMARY KEY,
                checksum_sha256 char(64) NOT NULL,
                applied_at_utc timestamptz NOT NULL
            );
            """;

        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task AcquireMigrationLockAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using NpgsqlCommand command = new(
            "SELECT pg_advisory_lock(@lock_key);",
            connection);
        command.Parameters.AddWithValue("lock_key", MigrationLockKey);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ReleaseMigrationLockAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using NpgsqlCommand command = new(
            "SELECT pg_advisory_unlock(@lock_key);",
            connection);
        command.Parameters.AddWithValue("lock_key", MigrationLockKey);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string?> GetExistingChecksumAsync(
        NpgsqlConnection connection,
        string migrationId,
        CancellationToken cancellationToken)
    {
        await using NpgsqlCommand command = new(
            """
            SELECT checksum_sha256
            FROM nublox_platform.schema_migrations
            WHERE migration_id = @migration_id;
            """,
            connection);
        command.Parameters.AddWithValue("migration_id", migrationId);

        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return value as string;
    }

    private static IReadOnlyList<EmbeddedMigration> LoadEmbeddedMigrations()
    {
        Assembly assembly = typeof(PostgresMigrationRunner).Assembly;
        const string marker = ".Migrations.";

        return assembly
            .GetManifestResourceNames()
            .Where(name => name.Contains(marker, StringComparison.Ordinal)
                && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.Ordinal)
            .Select(name =>
            {
                using Stream stream = assembly.GetManifestResourceStream(name)
                    ?? throw new InvalidOperationException($"Embedded migration '{name}' could not be opened.");
                using StreamReader reader = new(stream, Encoding.UTF8);
                string sql = reader.ReadToEnd();
                string migrationId = name[(name.IndexOf(marker, StringComparison.Ordinal) + marker.Length)..];
                string checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)))
                    .ToLowerInvariant();

                return new EmbeddedMigration(migrationId, checksum, sql);
            })
            .ToArray();
    }

    private sealed record EmbeddedMigration(string Id, string ChecksumSha256, string Sql);
}

public sealed record MigrationResult(string MigrationId, MigrationState State);

public enum MigrationState
{
    Applied = 1,
    AlreadyApplied = 2
}
