using NuBlox.FoundationSpike.Audit;
using NuBlox.FoundationSpike.Decisions;
using NuBlox.FoundationSpike.Shared;
using NuBlox.FoundationSpike.Subjects;
using NuBlox.FoundationSpike.Work;

namespace NuBlox.FoundationSpike.Application;

public sealed record CreateGovernedWorkCommand(
    CustomerId CustomerId,
    string SubjectName,
    string WorkSummary,
    string DecisionOutcome);

public sealed record CreateGovernedWorkResult(
    SubjectId SubjectId,
    WorkId WorkId,
    DecisionId DecisionId,
    AuditEventId AuditEventId);

public sealed class CreateGovernedWorkHandler(
    ITransactionalSessionFactory sessionFactory,
    SubjectModule subjects,
    WorkModule work,
    DecisionModule decisions,
    AuditModule audit)
{
    public async Task<CreateGovernedWorkResult> HandleAsync(
        CreateGovernedWorkCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command.SubjectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.WorkSummary);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.DecisionOutcome);

        await using var session = await sessionFactory.OpenAsync(cancellationToken);

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

            var decisionId = await decisions.CreateAsync(
                session,
                command.CustomerId,
                workId,
                command.DecisionOutcome,
                cancellationToken);

            var auditId = await audit.AppendAsync(
                session,
                command.CustomerId,
                subjectType: "governed_work",
                subjectId: workId.Value,
                eventType: "governed_work.created",
                payload: new
                {
                    subjectId = subjectId.Value,
                    workId = workId.Value,
                    decisionId = decisionId.Value,
                    decisionOutcome = command.DecisionOutcome.Trim().ToUpperInvariant()
                },
                cancellationToken);

            await session.CommitAsync(cancellationToken);

            return new CreateGovernedWorkResult(subjectId, workId, decisionId, auditId);
        }
        catch
        {
            await session.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
