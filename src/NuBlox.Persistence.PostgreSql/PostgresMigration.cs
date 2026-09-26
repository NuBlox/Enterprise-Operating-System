using System.Security.Cryptography;
using System.Text;

namespace NuBlox.Persistence.PostgreSql;

public sealed record PostgresMigration(string Id, string ResourceName, string Sql, string Sha256)
{
    public static PostgresMigration Create(string id, string resourceName, string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql))).ToLowerInvariant();
        return new PostgresMigration(id, resourceName, sql, hash);
    }
}
