using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using NuBlox.Audit;
using NuBlox.Persistence.PostgreSql;

namespace NuBlox.Audit.Infrastructure.PostgreSql;

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

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO audit.evidence (
                tenant_id, audit_event_id, actor_principal_id, actor_kind,
                action_code, subject_type, subject_id, recorded_at, effective_at,
                outcome, source, correlation_id, trace_id, reason_reference,
                evidence_references, corrects_event_id)
            VALUES (
                @tenant_id, @audit_event_id, @actor_principal_id, @actor_kind,
                @action_code, @subject_type, @subject_id, @recorded_at, @effective_at,
                @outcome, @source, @correlation_id, @trace_id, @reason_reference,
                @evidence_references, @corrects_event_id);
            """,
            connection,
            transaction);

        command.Parameters.AddWithValue("tenant_id", evidence.TenantId.Value);
        command.Parameters.AddWithValue("audit_event_id", evidence.EventId.Value);
        command.Parameters.AddWithValue("actor_principal_id", evidence.ActorPrincipalId.Value);
        command.Parameters.AddWithValue("actor_kind", FormatActorKind(evidence.ActorKind));
        command.Parameters.AddWithValue("action_code", evidence.ActionCode);
        command.Parameters.AddWithValue("subject_type", evidence.Subject.SubjectType);
        command.Parameters.AddWithValue("subject_id", evidence.Subject.SubjectId);
        command.Parameters.AddWithValue("recorded_at", evidence.RecordedAtUtc);
        command.Parameters.AddWithValue("effective_at", evidence.EffectiveAt is { } effectiveAt ? effectiveAt : DBNull.Value);
        command.Parameters.AddWithValue("outcome", FormatOutcome(evidence.Outcome));
        command.Parameters.AddWithValue("source", evidence.Source);
        command.Parameters.AddWithValue("correlation_id", evidence.CorrelationId is { } correlation ? correlation : DBNull.Value);
        command.Parameters.AddWithValue("trace_id", evidence.TraceId is { } trace ? trace : DBNull.Value);
        command.Parameters.AddWithValue("reason_reference", evidence.ReasonReference is { } reason ? reason : DBNull.Value);
        command.Parameters.AddWithValue(
            "evidence_references",
            NpgsqlDbType.Jsonb,
            JsonSerializer.Serialize(evidence.EvidenceReferences));
        command.Parameters.AddWithValue(
            "corrects_event_id",
            evidence.CorrectsEventId is { } corrected ? corrected.Value : DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string FormatActorKind(AuditActorKind actorKind) => actorKind switch
    {
        AuditActorKind.HumanPrincipal => "HUMAN_PRINCIPAL",
        AuditActorKind.ServicePrincipal => "SERVICE_PRINCIPAL",
        AuditActorKind.Integration => "INTEGRATION",
        _ => throw new ArgumentOutOfRangeException(nameof(actorKind))
    };

    private static string FormatOutcome(AuditOutcome outcome) => outcome switch
    {
        AuditOutcome.Succeeded => "SUCCEEDED",
        AuditOutcome.Denied => "DENIED",
        AuditOutcome.Failed => "FAILED",
        AuditOutcome.Corrected => "CORRECTED",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome))
    };
}
