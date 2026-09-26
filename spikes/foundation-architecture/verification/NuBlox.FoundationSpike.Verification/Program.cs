using System.Data.Common;
using NuBlox.FoundationSpike.Application;
using NuBlox.FoundationSpike.Audit;
using NuBlox.FoundationSpike.Decisions;
using NuBlox.FoundationSpike.Infrastructure;
using NuBlox.FoundationSpike.Shared;
using NuBlox.FoundationSpike.Subjects;
using NuBlox.FoundationSpike.Work;

var connectionString = Environment.GetEnvironmentVariable("NUBLOX_SPIKE_CONNECTION_STRING")
    ?? "Host=localhost;Port=55432;Database=nublox_spike;Username=nublox;Password=nublox_spike_dev_only";

var factory = new PostgresTransactionalSessionFactory(connectionString);
var subjects = new SubjectModule();
var work = new WorkModule();
var decisions = new DecisionModule();
var audit = new AuditModule();
var handler = new CreateGovernedWorkHandler(factory, subjects, work, decisions, audit);

var customerA = new CustomerId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
var customerB = new CustomerId(Guid.Parse("22222222-2222-2222-2222-222222222222"));

Console.WriteLine("SPIKE-001: verifying atomic governed-work transaction...");

var committed = await handler.HandleAsync(
    new CreateGovernedWorkCommand(
        customerA,
        "Foundation spike subject",
        "Prove modular atomic transaction",
        "APPROVED"));

Ensure(
    await ExistsAsync(factory, "subjects.business_subjects", customerA.Value, committed.SubjectId.Value),
    "Committed subject was not found.");
Ensure(
    await ExistsAsync(factory, "work.work_requests", customerA.Value, committed.WorkId.Value),
    "Committed work record was not found.");
Ensure(
    await ExistsAsync(factory, "decisions.approval_decisions", customerA.Value, committed.DecisionId.Value),
    "Committed decision was not found.");
Ensure(
    await ExistsAsync(factory, "audit.events", customerA.Value, committed.AuditEventId.Value),
    "Committed audit event was not found.");

Console.WriteLine("PASS: one transaction committed subject, work, decision and audit evidence.");

Console.WriteLine("SPIKE-003 precursor: verifying cross-customer relational isolation...");

var crossCustomerRejected = false;
await using (var session = await factory.OpenAsync())
{
    try
    {
        await work.CreateAsync(
            session,
            customerB,
            committed.SubjectId,
            "This insert must be rejected by the composite foreign key.");

        await session.RollbackAsync();
    }
    catch (Exception exception)
    {
        crossCustomerRejected = true;
        await session.RollbackAsync();
        Console.WriteLine($"Expected database rejection: {exception.GetType().Name}");
    }
}

Ensure(crossCustomerRejected, "Cross-customer subject/work relationship was not rejected.");
Console.WriteLine("PASS: database constraint rejected cross-customer relationship.");

Console.WriteLine("SPIKE-001: verifying explicit rollback leaves no partial module state...");

var rollbackSubjectId = SubjectId.New();
var rollbackWorkId = WorkId.New();

await using (var session = await factory.OpenAsync())
{
    rollbackSubjectId = await subjects.CreateAsync(
        session,
        customerB,
        "Rollback subject");

    rollbackWorkId = await work.CreateAsync(
        session,
        customerB,
        rollbackSubjectId,
        "Rollback work");

    await session.RollbackAsync();
}

Ensure(
    !await ExistsAsync(factory, "subjects.business_subjects", customerB.Value, rollbackSubjectId.Value),
    "Rolled-back subject still exists.");
Ensure(
    !await ExistsAsync(factory, "work.work_requests", customerB.Value, rollbackWorkId.Value),
    "Rolled-back work record still exists.");

Console.WriteLine("PASS: rollback removed all partial module writes.");
Console.WriteLine();
Console.WriteLine("FOUNDATION SPIKE VERIFICATION PASSED.");

return;

static void Ensure(bool condition, string failureMessage)
{
    if (!condition)
    {
        throw new InvalidOperationException($"SPIKE VERIFICATION FAILED: {failureMessage}");
    }
}

static async Task<bool> ExistsAsync(
    ITransactionalSessionFactory factory,
    string trustedTableName,
    Guid customerId,
    Guid id)
{
    await using var session = await factory.OpenAsync();

    await using DbCommand command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = $"SELECT EXISTS (SELECT 1 FROM {trustedTableName} WHERE customer_id = @customer_id AND id = @id);";
    command.AddParameter("customer_id", customerId);
    command.AddParameter("id", id);

    var result = await command.ExecuteScalarAsync();
    await session.RollbackAsync();

    return result is true;
}
