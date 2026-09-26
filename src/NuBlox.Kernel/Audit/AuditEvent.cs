using NuBlox.Kernel.Identity;

namespace NuBlox.Kernel.Audit;

public sealed record AuditEvent
{
    public AuditEvent(
        Guid auditEventId,
        TenantId tenantId,
        PrincipalId principalId,
        string eventType,
        string subjectType,
        string subjectId,
        DateTimeOffset recordedAtUtc,
        string? correlationId = null)
    {
        if (auditEventId == Guid.Empty)
        {
            throw new ArgumentException("Audit event ID cannot be empty.", nameof(auditEventId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectId);

        if (recordedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Audit recorded time must be UTC.", nameof(recordedAtUtc));
        }

        AuditEventId = auditEventId;
        TenantId = tenantId;
        PrincipalId = principalId;
        EventType = eventType;
        SubjectType = subjectType;
        SubjectId = subjectId;
        RecordedAtUtc = recordedAtUtc;
        CorrelationId = correlationId;
    }

    public Guid AuditEventId { get; }
    public TenantId TenantId { get; }
    public PrincipalId PrincipalId { get; }
    public string EventType { get; }
    public string SubjectType { get; }
    public string SubjectId { get; }
    public DateTimeOffset RecordedAtUtc { get; }
    public string? CorrelationId { get; }
}

public interface IAuditEventAppender
{
    ValueTask AppendAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}
