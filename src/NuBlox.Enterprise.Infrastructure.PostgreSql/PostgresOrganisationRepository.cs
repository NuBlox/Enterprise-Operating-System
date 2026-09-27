using Npgsql;
using NuBlox.Enterprise.Application;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;

namespace NuBlox.Enterprise.Infrastructure.PostgreSql;

public sealed class PostgresOrganisationRepository : IOrganisationRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresOrganisationRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task AddAsync(Organisation organisation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(organisation);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(
            connection,
            transaction,
            organisation.TenantId.Value,
            cancellationToken).ConfigureAwait(false);

        await using (var partyCommand = new NpgsqlCommand(
            """
            INSERT INTO enterprise.parties (
                tenant_id, party_id, party_kind, created_by_principal_id, created_at)
            VALUES (
                @tenant_id, @party_id, 'ORGANISATION', @created_by_principal_id, @created_at);
            """,
            connection,
            transaction))
        {
            partyCommand.Parameters.AddWithValue("tenant_id", organisation.TenantId.Value);
            partyCommand.Parameters.AddWithValue("party_id", organisation.Id.Value);
            partyCommand.Parameters.AddWithValue("created_by_principal_id", organisation.CreatedByPrincipalId.Value);
            partyCommand.Parameters.AddWithValue("created_at", organisation.CreatedAtUtc);
            await partyCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var organisationCommand = new NpgsqlCommand(
            """
            INSERT INTO enterprise.organisations (tenant_id, party_id, display_name)
            VALUES (@tenant_id, @party_id, @display_name);
            """,
            connection,
            transaction))
        {
            organisationCommand.Parameters.AddWithValue("tenant_id", organisation.TenantId.Value);
            organisationCommand.Parameters.AddWithValue("party_id", organisation.Id.Value);
            organisationCommand.Parameters.AddWithValue("display_name", organisation.DisplayName);
            await organisationCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Organisation?> FindAsync(
        TenantId tenantId,
        PartyId partyId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(
            connection,
            transaction,
            tenantId.Value,
            cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(
            """
            SELECT p.created_by_principal_id, p.created_at, o.display_name
            FROM enterprise.parties p
            JOIN enterprise.organisations o
              ON o.tenant_id = p.tenant_id
             AND o.party_id = p.party_id
            WHERE p.tenant_id = @tenant_id
              AND p.party_id = @party_id
              AND p.party_kind = 'ORGANISATION';
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("party_id", partyId.Value);

        Organisation? organisation = null;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                organisation = Organisation.Restore(
                    partyId,
                    tenantId,
                    reader.GetString(2),
                    new PrincipalId(reader.GetGuid(0)),
                    reader.GetFieldValue<DateTimeOffset>(1));
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return organisation;
    }
}
