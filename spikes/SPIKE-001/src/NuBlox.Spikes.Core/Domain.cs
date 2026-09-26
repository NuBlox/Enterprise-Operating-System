namespace NuBlox.Spikes.Core;

public sealed record CustomerContext(Guid CustomerId)
{
    public static CustomerContext Create(Guid customerId) =>
        customerId == Guid.Empty
            ? throw new ArgumentException("CustomerId must be non-empty.", nameof(customerId))
            : new(customerId);
}

public enum WorkRequestState
{
    Open = 1,
    AwaitingDecision = 2,
    Approved = 3,
    Rejected = 4,
    Completed = 5
}

public sealed class BusinessSubject
{
    private BusinessSubject(Guid id, Guid customerId, string reference, string title)
    {
        Id = id;
        CustomerId = customerId;
        Reference = reference;
        Title = title;
    }

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public string Reference { get; }
    public string Title { get; }

    public static BusinessSubject Create(CustomerContext context, string reference, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        return new BusinessSubject(Guid.NewGuid(), context.CustomerId, reference.Trim(), title.Trim());
    }
}

public sealed class WorkRequest
{
    private WorkRequest(Guid id, Guid customerId, Guid subjectId, string description)
    {
        Id = id;
        CustomerId = customerId;
        SubjectId = subjectId;
        Description = description;
        State = WorkRequestState.Open;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public Guid SubjectId { get; }
    public string Description { get; }
    public WorkRequestState State { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    public static WorkRequest Create(CustomerContext context, BusinessSubject subject, string description)
    {
        if (context.CustomerId != subject.CustomerId)
            throw new InvalidOperationException("Subject belongs to a different customer context.");

        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        return new WorkRequest(Guid.NewGuid(), context.CustomerId, subject.Id, description.Trim());
    }

    public void SubmitForDecision()
    {
        if (State != WorkRequestState.Open)
            throw new InvalidOperationException($"Cannot submit work from state {State}.");

        State = WorkRequestState.AwaitingDecision;
    }

    public void ApplyDecision(Decision decision)
    {
        if (decision.WorkRequestId != Id || decision.CustomerId != CustomerId)
            throw new InvalidOperationException("Decision does not belong to this work request/customer context.");

        if (State != WorkRequestState.AwaitingDecision)
            throw new InvalidOperationException($"Cannot apply a decision while work is in state {State}.");

        State = decision.Outcome switch
        {
            DecisionOutcome.Approved => WorkRequestState.Approved,
            DecisionOutcome.Rejected => WorkRequestState.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(decision))
        };
    }

    public void Complete()
    {
        if (State != WorkRequestState.Approved)
            throw new InvalidOperationException("Only approved work can be completed in this spike scenario.");

        State = WorkRequestState.Completed;
    }
}

public enum DecisionOutcome
{
    Approved = 1,
    Rejected = 2
}

public sealed record Decision(
    Guid Id,
    Guid CustomerId,
    Guid WorkRequestId,
    DecisionOutcome Outcome,
    Guid DecidedBy,
    DateTimeOffset DecidedAtUtc,
    string Rationale)
{
    public static Decision Create(
        CustomerContext context,
        WorkRequest workRequest,
        DecisionOutcome outcome,
        Guid decidedBy,
        string rationale)
    {
        if (context.CustomerId != workRequest.CustomerId)
            throw new InvalidOperationException("Work request belongs to a different customer context.");
        if (decidedBy == Guid.Empty)
            throw new ArgumentException("Decision maker must be non-empty.", nameof(decidedBy));
        ArgumentException.ThrowIfNullOrWhiteSpace(rationale);

        return new Decision(
            Guid.NewGuid(),
            context.CustomerId,
            workRequest.Id,
            outcome,
            decidedBy,
            DateTimeOffset.UtcNow,
            rationale.Trim());
    }
}

public sealed record AuditEvidence(
    Guid Id,
    Guid CustomerId,
    string EventType,
    Guid SubjectId,
    Guid ActorId,
    DateTimeOffset OccurredAtUtc,
    string Summary)
{
    public static AuditEvidence Create(
        CustomerContext context,
        string eventType,
        Guid subjectId,
        Guid actorId,
        string summary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        if (subjectId == Guid.Empty) throw new ArgumentException("Subject must be non-empty.", nameof(subjectId));
        if (actorId == Guid.Empty) throw new ArgumentException("Actor must be non-empty.", nameof(actorId));

        return new AuditEvidence(
            Guid.NewGuid(),
            context.CustomerId,
            eventType.Trim(),
            subjectId,
            actorId,
            DateTimeOffset.UtcNow,
            summary.Trim());
    }
}
