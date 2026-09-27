using Npgsql;
using NuBlox.Enterprise.Application;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;

namespace NuBlox.Enterprise.Infrastructure.PostgreSql;

public sealed class PostgresPersonRepository : IPersonRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresPersonRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task AddAsync(Person person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(
            connection,
            transaction,
            person.TenantId.Value,
            cancellationToken).ConfigureAwait(false);

        await using (var partyCommand = new NpgsqlCommand(
            """
            INSERT INTO enterprise.parties (
                tenant_id, party_id, party_kind, created_by_principal_id, created_at)
            VALUES (
                @tenant_id, @party_id, 'PERSON', @created_by_principal_id, @created_at);
            """,
            connection,
            transaction))
        {
            partyCommand.Parameters.AddWithValue("tenant_id", person.TenantId.Value);
            partyCommand.Parameters.AddWithValue("party_id", person.Id.Value);
            partyCommand.Parameters.AddWithValue("created_by_principal_id", person.CreatedByPrincipalId.Value);
            partyCommand.Parameters.AddWithValue("created_at", person.CreatedAtUtc);
            await partyCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var personCommand = new NpgsqlCommand(
            """
            INSERT INTO enterprise.people (tenant_id, party_id, display_name)
            VALUES (@tenant_id, @party_id, @display_name);
            """,
            connection,
            transaction))
        {
            personCommand.Parameters.AddWithValue("tenant_id", person.TenantId.Value);
            personCommand.Parameters.AddWithValue("party_id", person.Id.Value);
            personCommand.Parameters.AddWithValue("display_name", person.DisplayName);
            await personCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Person?> FindAsync(
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
            SELECT p.created_by_principal_id, p.created_at, person.display_name
            FROM enterprise.parties p
            JOIN enterprise.people person
              ON person.tenant_id = p.tenant_id
             AND person.party_id = p.party_id
            WHERE p.tenant_id = @tenant_id
              AND p.party_id = @party_id
              AND p.party_kind = 'PERSON';
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("party_id", partyId.Value);

        Person? person = null;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                person = Person.Restore(
                    partyId,
                    tenantId,
                    reader.GetString(2),
                    new PrincipalId(reader.GetGuid(0)),
                    reader.GetFieldValue<DateTimeOffset>(1));
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return person;
    }
}
