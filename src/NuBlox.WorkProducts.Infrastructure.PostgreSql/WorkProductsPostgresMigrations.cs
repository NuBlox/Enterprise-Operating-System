using NuBlox.Persistence.PostgreSql;

namespace NuBlox.WorkProducts.Infrastructure.PostgreSql;

public static class WorkProductsPostgresMigrations
{
    public static IReadOnlyList<PostgresMigration> Load() =>
        PostgresMigrationCatalog.Load(typeof(WorkProductsPostgresMigrations).Assembly);
}
