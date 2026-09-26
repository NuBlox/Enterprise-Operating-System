using Npgsql;

namespace NuBlox.Persistence.PostgreSql;

public static class PostgresTenantSession
{
    public static async Task SetTenantAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        await using var command = new NpgsqlCommand(
            "SELECT set_config('nublox.tenant_id', @tenant_id, true);",
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.ToString("D"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
