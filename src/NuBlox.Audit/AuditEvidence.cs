using System.Text.RegularExpressions;
using NuBlox.Kernel;

namespace NuBlox.Audit;

public readonly record struct AuditEventId
{
    public AuditEventId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Audit event identifiers cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static AuditEventId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}

public enum AuditActorKind
{
    HumanPrincipal = 1,
    ServicePrincipal = 2,
    Integration = 3
}

public enum AuditOutcome
{
    Succeeded = 1,
    Denied = 2,
    Failed = 3,
    Corrected = 4
}

public sealed record AuditEvidenceReference
{
    public AuditEvidenceReference(string referenceType, string referenceId)
    {
        ReferenceType = AuditCode.Require(referenceType, nameof(referenceType));
        ReferenceId = AuditText.Require(referenceId, nameof(referenceId), 256);
    }

    public string ReferenceType { get; }

    public string ReferenceId { get; }
}

public sealed record AuditSubject
{
    public AuditSubject(string subjectType, string subjectId)
    {
        SubjectType = AuditCode.Require(subjectType, nameof(subjectType));
        SubjectId = AuditText.Require(subjectId, nameof(subjectId), 256);
    }

    public string SubjectType { get; }

    public string SubjectId { get; }
}

/// <summary>
/// Immutable, payload-minimised evidence of a material action or decision. The model intentionally contains
/// references rather than arbitrary dictionaries/request payloads so credentials, tokens and business payloads
/// cannot be persisted through this contract by convenience.
/// </summary>
public sealed class AuditEvidence
{
    public AuditEvidence(
        AuditEventId eventId,
        TenantId tenantId,
        PrincipalId actorPrincipalId,
        AuditActorKind actorKind,
        string actionCode,
        AuditSubject subject,
        DateTimeOffset recordedAtUtc,
        AuditOutcome outcome,
        string source,
        string? correlationId = null,
        string? traceId = null,
        DateTimeOffset? effectiveAt = null,
        string? reasonReference = null,
        IReadOnlyCollection<AuditEvidenceReference>? evidenceReferences = null,
        AuditEventId? correctsEventId = null)
    {
        if (recordedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Audit recorded time must be UTC.", nameof(recordedAtUtc));
        }

        if (correctsEventId is { } correction && correction == eventId)
        {
            throw new ArgumentException("A correction event must have a new event identifier.", nameof(correctsEventId));
        }

        EventId = eventId;
        TenantId = tenantId;
        ActorPrincipalId = actorPrincipalId;
        ActorKind = actorKind;
        ActionCode = AuditCode.Require(actionCode, nameof(actionCode));
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        RecordedAtUtc = recordedAtUtc;
        Outcome = outcome;
        Source = AuditCode.Require(source, nameof(source));
        CorrelationId = AuditText.Optional(correlationId, nameof(correlationId), 128);
        TraceId = AuditText.Optional(traceId, nameof(traceId), 64);
        EffectiveAt = effectiveAt;
        ReasonReference = AuditText.Optional(reasonReference, nameof(reasonReference), 256);
        EvidenceReferences = evidenceReferences is null ? Array.Empty<AuditEvidenceReference>() : [.. evidenceReferences];
        CorrectsEventId = correctsEventId;
    }

    public AuditEventId EventId { get; }

    public TenantId TenantId { get; }

    public PrincipalId ActorPrincipalId { get; }

    public AuditActorKind ActorKind { get; }

    public string ActionCode { get; }

    public AuditSubject Subject { get; }

    public DateTimeOffset RecordedAtUtc { get; }

    public DateTimeOffset? EffectiveAt { get; }

    public AuditOutcome Outcome { get; }

    public string Source { get; }

    public string? CorrelationId { get; }

    public string? TraceId { get; }

    public string? ReasonReference { get; }

    public IReadOnlyList<AuditEvidenceReference> EvidenceReferences { get; }

    public AuditEventId? CorrectsEventId { get; }
}

public interface IAuditEvidenceAppender
{
    ValueTask AppendAsync(AuditEvidence evidence, CancellationToken cancellationToken = default);
}

internal static partial class AuditCode
{
    [GeneratedRegex("^[a-z][a-z0-9_.-]{1,79}$", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();

    public static string Require(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A governed code is required.", parameterName);
        }

        var normalised = value.Trim().ToLowerInvariant();
        if (!Pattern().IsMatch(normalised))
        {
            throw new ArgumentException("The value is not a valid governed code.", parameterName);
        }

        return normalised;
    }
}

internal static class AuditText
{
    public static string Require(string value, string parameterName, int maximumLength)
    {
        var result = Optional(value, parameterName, maximumLength);
        return result ?? throw new ArgumentException("A value is required.", parameterName);
    }

    public static string? Optional(string? value, string parameterName, int maximumLength)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length == 0 || trimmed.Length > maximumLength)
        {
            throw new ArgumentException($"The value must contain between 1 and {maximumLength} characters.", parameterName);
        }

        return trimmed;
    }
}
