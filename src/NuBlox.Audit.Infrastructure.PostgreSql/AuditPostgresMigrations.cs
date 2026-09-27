using NuBlox.Persistence.PostgreSql;

namespace NuBlox.Audit.Infrastructure.PostgreSql;

public static class AuditPostgresMigrations
{
    public static IReadOnlyList<PostgresMigration> Load() =>
        PostgresMigrationCatalog.Load(typeof(AuditPostgresMigrations).Assembly);
}
