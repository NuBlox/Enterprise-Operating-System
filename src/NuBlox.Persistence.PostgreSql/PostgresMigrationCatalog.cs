using System.Reflection;

namespace NuBlox.Persistence.PostgreSql;

public static class PostgresMigrationCatalog
{
    private const string Marker = ".Migrations.";

    public static IReadOnlyList<PostgresMigration> Load(Assembly? assembly = null)
    {
        assembly ??= typeof(PostgresMigrationCatalog).Assembly;

        var migrations = assembly
            .GetManifestResourceNames()
            .Where(static name => name.Contains(Marker, StringComparison.Ordinal) && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static name => name, StringComparer.Ordinal)
            .Select(name => LoadMigration(assembly, name))
            .ToArray();

        if (migrations.Length == 0)
        {
            throw new InvalidOperationException("No embedded PostgreSQL migrations were found.");
        }

        var duplicateId = migrations
            .GroupBy(static migration => migration.Id, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicateId is not null)
        {
            throw new InvalidOperationException($"Duplicate PostgreSQL migration id '{duplicateId.Key}'.");
        }

        return migrations;
    }

    private static PostgresMigration LoadMigration(Assembly assembly, string resourceName)
    {
        var markerIndex = resourceName.IndexOf(Marker, StringComparison.Ordinal);
        var fileName = resourceName[(markerIndex + Marker.Length)..];
        var id = Path.GetFileNameWithoutExtension(fileName);

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Migration resource '{resourceName}' could not be opened.");
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        return PostgresMigration.Create(id, resourceName, sql);
    }
}
