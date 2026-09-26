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

// Unrestricted factory exists only for adversarial/setup checks inside this disposable verifier.
var adminFactory = new PostgresTransactionalSessionFactory(connectionString);
// Application use cases receive only the scoped factory: a customer context is mandatory.
var customerFactory = new PostgresCustomerScopedTransactionalSessionFactory(connectionString);
var subjects = new SubjectModule();
var subjectHistory = new SubjectHistoryModule();
var work = new WorkModule();
var decisions = new DecisionModule();
var audit = new AuditModule();
var handler = new CreateGovernedWorkHandler(customerFactory, subjects, work, decisions, audit);

var customerA = new CustomerId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
var customerB = new CustomerId(Guid.Parse("22222222-2222-2222-2222-222222222222"));

Console.WriteLine("SPIKE-001: verifying atomic governed-work transaction through a customer-scoped application session...");

var committed = await handler.HandleAsync(
    new CreateGovernedWorkCommand(
        customerA,
        "Foundation spike subject",
        "Prove modular atomic transaction",
        "APPROVED"));

Ensure(
    await ExistsAsync(adminFactory, "subjects.business_subjects", customerA.Value, committed.SubjectId.Value),
    "Committed subject was not found.");
Ensure(
    await ExistsAsync(adminFactory, "work.work_requests", customerA.Value, committed.WorkId.Value),
    "Committed work record was not found.");
Ensure(
    await ExistsAsync(adminFactory, "decisions.approval_decisions", customerA.Value, committed.DecisionId.Value),
    "Committed decision was not found.");
Ensure(
    await ExistsAsync(adminFactory, "audit.events", customerA.Value, committed.AuditEventId.Value),
    "Committed audit event was not found.");

Console.WriteLine("PASS: one customer-scoped application transaction committed subject, work, decision and audit evidence.");

Console.WriteLine("SPIKE-003 precursor: verifying cross-customer relational isolation...");

var crossCustomerRejected = false;
await using (var session = await adminFactory.OpenAsync())
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

await using (var session = await adminFactory.OpenAsync())
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
    !await ExistsAsync(adminFactory, "subjects.business_subjects", customerB.Value, rollbackSubjectId.Value),
    "Rolled-back subject still exists.");
Ensure(
    !await ExistsAsync(adminFactory, "work.work_requests", customerB.Value, rollbackWorkId.Value),
    "Rolled-back work record still exists.");

Console.WriteLine("PASS: rollback removed all partial module writes.");

Console.WriteLine("SPIKE-002: verifying business-effective history and as-of reconstruction...");

var historySubjectId = SubjectId.New();
var initialEffective = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
var renameEffective = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

await using (var session = await adminFactory.OpenAsync())
{
    historySubjectId = await subjects.CreateIdentityAsync(
        session,
        customerA);

    await subjectHistory.InitialiseAsync(
        session,
        customerA,
        historySubjectId,
        "Original Name",
        initialEffective,
        "Initial effective version");

    await session.CommitAsync();
}

await using (var session = await adminFactory.OpenAsync())
{
    await subjectHistory.ChangeNameAsync(
        session,
        customerA,
        historySubjectId,
        "Renamed Subject",
        renameEffective,
        "Approved rename");

    await session.CommitAsync();
}

var originalAsOf = await GetNameAsOfAsync(
    adminFactory,
    subjectHistory,
    customerA,
    historySubjectId,
    new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero))
    ?? throw new InvalidOperationException("SPIKE VERIFICATION FAILED: Original as-of version was not found.");

var renamedAsOf = await GetNameAsOfAsync(
    adminFactory,
    subjectHistory,
    customerA,
    historySubjectId,
    new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero))
    ?? throw new InvalidOperationException("SPIKE VERIFICATION FAILED: Renamed as-of version was not found.");

Ensure(originalAsOf.DisplayName == "Original Name", "Original as-of query returned the wrong name.");
Ensure(originalAsOf.EffectiveFrom == initialEffective, "Original effective-from value was not preserved.");
Ensure(originalAsOf.EffectiveTo == renameEffective, "Original version was not closed at the rename effective date.");
Ensure(originalAsOf.RecordedAt > renameEffective, "Technical recorded-at timestamp was not distinct from historical business-effective time.");

Ensure(renamedAsOf.DisplayName == "Renamed Subject", "Later as-of query returned the wrong name.");
Ensure(renamedAsOf.EffectiveFrom == renameEffective, "Renamed effective-from value was not preserved.");
Ensure(renamedAsOf.EffectiveTo is null, "Current version should remain open-ended.");

Console.WriteLine("PASS: as-of queries reconstruct different business-effective states while preserving technical recording time.");

Console.WriteLine("SPIKE-002: verifying overlapping effective periods are rejected by the database...");

var overlapRejected = false;
await using (var session = await adminFactory.OpenAsync())
{
    try
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO subjects.business_subject_names
                (customer_id, subject_id, effective_from, effective_to, display_name, change_reason)
            VALUES
                (@customer_id, @subject_id, @effective_from, @effective_to, @display_name, @change_reason);
            """;
        command.AddParameter("customer_id", customerA.Value);
        command.AddParameter("subject_id", historySubjectId.Value);
        command.AddParameter("effective_from", new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        command.AddParameter("effective_to", new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc));
        command.AddParameter("display_name", "Invalid overlapping version");
        command.AddParameter("change_reason", "Overlap test");

        await command.ExecuteNonQueryAsync();
        await session.RollbackAsync();
    }
    catch (Exception exception)
    {
        overlapRejected = true;
        await session.RollbackAsync();
        Console.WriteLine($"Expected overlap rejection: {exception.GetType().Name}");
    }
}

Ensure(overlapRejected, "Overlapping effective-dated subject state was not rejected.");
Console.WriteLine("PASS: database exclusion constraint rejected overlapping effective periods.");

Console.WriteLine("SPIKE-003: verifying row-level customer isolation through the mandatory application context...");

var customerBCommitted = await handler.HandleAsync(
    new CreateGovernedWorkCommand(
        customerB,
        "Customer B isolation subject",
        "Prove customer B application context",
        "APPROVED"));

await using (var session = await customerFactory.OpenAsync(customerA))
{
    Ensure(
        await VisibleByIdAsync(session, "subjects.business_subjects", committed.SubjectId.Value),
        "Customer A could not read its own subject through the scoped application session.");
    Ensure(
        !await VisibleByIdAsync(session, "subjects.business_subjects", customerBCommitted.SubjectId.Value),
        "Customer A could read Customer B's subject through the scoped application session.");
    Ensure(
        !await VisibleByIdAsync(session, "work.work_requests", customerBCommitted.WorkId.Value),
        "Customer A could read Customer B's work through the scoped application session.");
    Ensure(
        !await VisibleByIdAsync(session, "decisions.approval_decisions", customerBCommitted.DecisionId.Value),
        "Customer A could read Customer B's decision through the scoped application session.");
    Ensure(
        !await VisibleByIdAsync(session, "audit.events", customerBCommitted.AuditEventId.Value),
        "Customer A could read Customer B's audit event through the scoped application session.");

    await session.RollbackAsync();
}

Console.WriteLine("PASS: scoped application session sees own-customer data and hides another customer's subject/work/decision/audit rows.");

await using (var session = await adminFactory.OpenAsync())
{
    await SetApplicationRoleAsync(session);

    Ensure(
        !await VisibleByIdAsync(session, "subjects.business_subjects", committed.SubjectId.Value),
        "Application role without customer context could read a customer row.");

    await session.RollbackAsync();
}

Console.WriteLine("PASS: missing customer context exposes no customer rows.");

var wrongCustomerWriteRejected = false;
await using (var session = await customerFactory.OpenAsync(customerA))
{
    try
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO subjects.business_subjects (customer_id, id)
            VALUES (@customer_id, @id);
            """;
        command.AddParameter("customer_id", customerB.Value);
        command.AddParameter("id", Guid.NewGuid());

        await command.ExecuteNonQueryAsync();
        await session.RollbackAsync();
    }
    catch (Exception exception)
    {
        wrongCustomerWriteRejected = true;
        await session.RollbackAsync();
        Console.WriteLine($"Expected row-policy write rejection: {exception.GetType().Name}");
    }
}

Ensure(wrongCustomerWriteRejected, "Customer-scoped application session could write another customer's row.");
Console.WriteLine("PASS: row-level security rejected a wrong-customer write.");

Console.WriteLine("SPIKE-003: verifying background-style scoped work cannot lose customer context...");

var customerACount = await CountVisibleSubjectsAsync(customerFactory, customerA);
var customerBCount = await CountVisibleSubjectsAsync(customerFactory, customerB);
Ensure(customerACount > 0, "Customer A background-style scoped query returned no own rows.");
Ensure(customerBCount > 0, "Customer B background-style scoped query returned no own rows.");
Ensure(
    !await IsVisibleFromCustomerAsync(customerFactory, customerA, customerBCommitted.SubjectId.Value),
    "Background-style Customer A session could observe a Customer B subject.");

Console.WriteLine("PASS: reusable scoped-session factory carries customer context into background-style operations.");

Console.WriteLine();
Console.WriteLine("FOUNDATION SPIKE VERIFICATION PASSED (SPIKE-001 + SPIKE-002 + SPIKE-003).");

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

static async Task<EffectiveSubjectNameVersion?> GetNameAsOfAsync(
    ITransactionalSessionFactory factory,
    SubjectHistoryModule subjectHistory,
    CustomerId customerId,
    SubjectId subjectId,
    DateTimeOffset effectiveAt)
{
    await using var session = await factory.OpenAsync();
    var version = await subjectHistory.GetAsOfAsync(
        session,
        customerId,
        subjectId,
        effectiveAt);
    await session.RollbackAsync();
    return version;
}

static async Task SetApplicationRoleAsync(ITransactionalSession session)
{
    await using var command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = "SET LOCAL ROLE nublox_app;";
    await command.ExecuteNonQueryAsync();
}

static async Task<bool> VisibleByIdAsync(
    ITransactionalSession session,
    string trustedTableName,
    Guid id)
{
    await using var command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = $"SELECT EXISTS (SELECT 1 FROM {trustedTableName} WHERE id = @id);";
    command.AddParameter("id", id);

    var result = await command.ExecuteScalarAsync();
    return result is true;
}

static async Task<int> CountVisibleSubjectsAsync(
    ICustomerScopedTransactionalSessionFactory factory,
    CustomerId customerId)
{
    await using var session = await factory.OpenAsync(customerId);
    await using var command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = "SELECT count(*) FROM subjects.business_subjects;";
    var result = await command.ExecuteScalarAsync();
    await session.RollbackAsync();
    return Convert.ToInt32(result);
}

static async Task<bool> IsVisibleFromCustomerAsync(
    ICustomerScopedTransactionalSessionFactory factory,
    CustomerId customerId,
    Guid subjectId)
{
    await using var session = await factory.OpenAsync(customerId);
    var visible = await VisibleByIdAsync(session, "subjects.business_subjects", subjectId);
    await session.RollbackAsync();
    return visible;
}
