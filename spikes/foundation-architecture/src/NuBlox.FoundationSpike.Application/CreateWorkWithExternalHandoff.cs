using NuBlox.FoundationSpike.Audit;
using NuBlox.FoundationSpike.Integration;
using NuBlox.FoundationSpike.Shared;
using NuBlox.FoundationSpike.Subjects;
using NuBlox.FoundationSpike.Work;

namespace NuBlox.FoundationSpike.Application;

public sealed record CreateWorkWithExternalHandoffCommand(
    CustomerId CustomerId,
    string SubjectName,
    string WorkSummary,
    string IdempotencyKey,
    string DestinationReference);

public sealed record CreateWorkWithExternalHandoffResult(
    SubjectId SubjectId,
    WorkId WorkId,
    OutboxMessageId OutboxMessageId,
    AuditEventId AuditEventId);

public sealed class CreateWorkWithExternalHandoffHandler(
    ICustomerScopedTransactionalSessionFactory sessionFactory,
    SubjectModule subjects,
    WorkModule work,
    OutboxModule outbox,
    AuditModule audit)
{
    public async Task<CreateWorkWithExternalHandoffResult> HandleAsync(
        CreateWorkWithExternalHandoffCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command.SubjectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.WorkSummary);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.IdempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.DestinationReference);

        await using var session = await sessionFactory.OpenAsync(
            command.CustomerId,
            cancellationToken);

        try
        {
            var subjectId = await subjects.CreateAsync(
                session,
                command.CustomerId,
                command.SubjectName,
                cancellationToken);

            var workId = await work.CreateAsync(
                session,
                command.CustomerId,
                subjectId,
                command.WorkSummary,
                cancellationToken);

            var outboxId = await outbox.EnqueueAsync(
                session,
                command.CustomerId,
                command.IdempotencyKey,
                operation: "EXTERNAL_WORK_HANDOFF",
                subjectType: "work_request",
                subjectId: workId.Value,
                payload: new
                {
                    workId = workId.Value,
                    subjectId = subjectId.Value,
                    destinationReference = command.DestinationReference
                },
                cancellationToken);

            var auditId = await audit.AppendAsync(
                session,
                command.CustomerId,
                subjectType: "work_request",
                subjectId: workId.Value,
                eventType: "external_handoff.intent_recorded",
                payload: new
                {
                    outboxMessageId = outboxId.Value,
                    idempotencyKey = command.IdempotencyKey,
                    destinationReference = command.DestinationReference
                },
                cancellationToken);

            await session.CommitAsync(cancellationToken);

            return new CreateWorkWithExternalHandoffResult(
                subjectId,
                workId,
                outboxId,
                auditId);
        }
        catch
        {
            await session.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
