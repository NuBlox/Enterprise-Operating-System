using Npgsql;
using NuBlox.Kernel.Identity;

namespace NuBlox.Persistence.PostgreSql;

public static class PostgresTenantSession
{
    public static async Task SetTenantAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        if (tenantId.Value == Guid.Empty)
        {
            throw new ArgumentException("Tenant identifier cannot be empty.", nameof(tenantId));
        }

        await using var command = new NpgsqlCommand(
            "SELECT set_config('nublox.tenant_id', @tenant_id, true);",
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
