using Npgsql;
using NuBlox.Kernel.Audit;

namespace NuBlox.Persistence.Postgres;

public sealed class PostgresAuditEventAppender : IAuditEventAppender
{
    private readonly string _connectionString;

    public PostgresAuditEventAppender(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public async ValueTask AppendAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);

        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(_connectionString);
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await AppendWithinTransactionAsync(auditEvent, connection, transaction, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public static async ValueTask AppendWithinTransactionAsync(
        AuditEvent auditEvent,
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        await using (NpgsqlCommand tenantContextCommand = new(
            "SELECT set_config('nublox.tenant_id', @tenant_id, true);",
            connection,
            transaction))
        {
            tenantContextCommand.Parameters.AddWithValue("tenant_id", auditEvent.TenantId.ToString());
            await tenantContextCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using NpgsqlCommand command = new(
            """
            INSERT INTO nublox_platform.audit_events
                (audit_event_id, tenant_id, principal_id, event_type, subject_type,
                 subject_id, recorded_at_utc, correlation_id)
            VALUES
                (@audit_event_id, @tenant_id, @principal_id, @event_type, @subject_type,
                 @subject_id, @recorded_at_utc, @correlation_id);
            """,
            connection,
            transaction);

        command.Parameters.AddWithValue("audit_event_id", auditEvent.AuditEventId);
        command.Parameters.AddWithValue("tenant_id", auditEvent.TenantId.Value);
        command.Parameters.AddWithValue("principal_id", auditEvent.PrincipalId.Value);
        command.Parameters.AddWithValue("event_type", auditEvent.EventType);
        command.Parameters.AddWithValue("subject_type", auditEvent.SubjectType);
        command.Parameters.AddWithValue("subject_id", auditEvent.SubjectId);
        command.Parameters.AddWithValue("recorded_at_utc", auditEvent.RecordedAtUtc);
        command.Parameters.AddWithValue(
            "correlation_id",
            (object?)auditEvent.CorrelationId ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
