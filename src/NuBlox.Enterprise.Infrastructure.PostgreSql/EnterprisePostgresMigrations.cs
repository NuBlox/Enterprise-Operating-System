using NuBlox.Persistence.PostgreSql;

namespace NuBlox.Enterprise.Infrastructure.PostgreSql;

public static class EnterprisePostgresMigrations
{
    public static IReadOnlyList<PostgresMigration> Load() =>
        PostgresMigrationCatalog.Load(typeof(EnterprisePostgresMigrations).Assembly);
}
