using NuBlox.Enterprise.Infrastructure.PostgreSql;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Infrastructure.PostgreSql;

namespace NuBlox.Runtime.PostgreSql;

public static class PostgresRuntimeMigrations
{
    public static IReadOnlyList<PostgresMigration> LoadAll()
    {
        var migrations = PostgresMigrationCatalog.Load()
            .Concat(EnterprisePostgresMigrations.Load())
            .Concat(WorkProductsPostgresMigrations.Load())
            .Concat(PostgresMigrationCatalog.Load(typeof(PostgresRuntimeMigrations).Assembly))
            .OrderBy(static migration => migration.Id, StringComparer.Ordinal)
            .ToArray();

        var duplicate = migrations
            .GroupBy(static migration => migration.Id, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Duplicate runtime PostgreSQL migration id '{duplicate.Key}'.");
        }

        return migrations;
    }
}
