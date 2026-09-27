using Npgsql;
using NuBlox.Audit;
using NuBlox.Persistence.PostgreSql;

namespace NuBlox.Runtime.PostgreSql;

public sealed class PostgresAuditEvidenceAppender : IAuditEvidenceAppender
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresAuditEvidenceAppender(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async ValueTask AppendAsync(
        AuditEvidence evidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(
            connection,
            transaction,
            evidence.TenantId.Value,
            cancellationToken).ConfigureAwait(false);

        await using (var command = new NpgsqlCommand(
            """
            INSERT INTO audit.events (
                tenant_id, event_id, actor_principal_id, actor_kind,
                action_code, subject_type, subject_id, recorded_at,
                effective_at, outcome, source, correlation_id, trace_id,
                reason_reference, corrects_event_id)
            VALUES (
                @tenant_id, @event_id, @actor_principal_id, @actor_kind,
                @action_code, @subject_type, @subject_id, @recorded_at,
                @effective_at, @outcome, @source, @correlation_id, @trace_id,
                @reason_reference, @corrects_event_id);
            """,
            connection,
            transaction))
        {
            command.Parameters.AddWithValue("tenant_id", evidence.TenantId.Value);
            command.Parameters.AddWithValue("event_id", evidence.EventId.Value);
            command.Parameters.AddWithValue("actor_principal_id", evidence.ActorPrincipalId.Value);
            command.Parameters.AddWithValue("actor_kind", evidence.ActorKind.ToString());
            command.Parameters.AddWithValue("action_code", evidence.ActionCode);
            command.Parameters.AddWithValue("subject_type", evidence.Subject.SubjectType);
            command.Parameters.AddWithValue("subject_id", evidence.Subject.SubjectId);
            command.Parameters.AddWithValue("recorded_at", evidence.RecordedAtUtc);
            command.Parameters.AddWithValue("effective_at", (object?)evidence.EffectiveAt ?? DBNull.Value);
            command.Parameters.AddWithValue("outcome", evidence.Outcome.ToString());
            command.Parameters.AddWithValue("source", evidence.Source);
            command.Parameters.AddWithValue("correlation_id", (object?)evidence.CorrelationId ?? DBNull.Value);
            command.Parameters.AddWithValue("trace_id", (object?)evidence.TraceId ?? DBNull.Value);
            command.Parameters.AddWithValue("reason_reference", (object?)evidence.ReasonReference ?? DBNull.Value);
            command.Parameters.AddWithValue("corrects_event_id", (object?)evidence.CorrectsEventId?.Value ?? DBNull.Value);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        for (var ordinal = 0; ordinal < evidence.EvidenceReferences.Count; ordinal++)
        {
            var reference = evidence.EvidenceReferences[ordinal];
            await using var command = new NpgsqlCommand(
                """
                INSERT INTO audit.evidence_references (
                    tenant_id, event_id, ordinal, reference_type, reference_id)
                VALUES (@tenant_id, @event_id, @ordinal, @reference_type, @reference_id);
                """,
                connection,
                transaction);
            command.Parameters.AddWithValue("tenant_id", evidence.TenantId.Value);
            command.Parameters.AddWithValue("event_id", evidence.EventId.Value);
            command.Parameters.AddWithValue("ordinal", ordinal);
            command.Parameters.AddWithValue("reference_type", reference.ReferenceType);
            command.Parameters.AddWithValue("reference_id", reference.ReferenceId);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
