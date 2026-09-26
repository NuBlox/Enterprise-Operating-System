using NuBlox.Persistence.Postgres;

string? connectionString = Environment.GetEnvironmentVariable("NUBLOX_DATABASE_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("NUBLOX_DATABASE_CONNECTION_STRING is required.");
    return 2;
}

var runner = new PostgresMigrationRunner(connectionString);
IReadOnlyList<MigrationResult> results = await runner.ApplyAsync();

foreach (MigrationResult result in results)
{
    Console.WriteLine($"{result.MigrationId}: {result.State}");
}

return 0;
