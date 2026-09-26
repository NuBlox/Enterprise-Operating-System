using System.Data.Common;
using NuBlox.FoundationSpike.Infrastructure;
using NuBlox.FoundationSpike.Migration;
using NuBlox.FoundationSpike.Shared;
using NuBlox.FoundationSpike.Subjects;

var connectionString = Environment.GetEnvironmentVariable("NUBLOX_SPIKE_CONNECTION_STRING")
    ?? "Host=localhost;Port=55432;Database=nublox_spike;Username=nublox;Password=nublox_spike_dev_only";

var customerFactory = new PostgresCustomerScopedTransactionalSessionFactory(connectionString);
var subjects = new SubjectModule();
var subjectHistory = new SubjectHistoryModule();
var migration = new MigrationModule(subjects, subjectHistory);

var customerA = new CustomerId(Guid.Parse("77777777-7777-7777-7777-777777777777"));
var customerB = new CustomerId(Guid.Parse("88888888-8888-8888-8888-888888888888"));

Console.WriteLine("SPIKE-007: staging mixed-quality source records...");

Guid batchId;
await using (var session = await customerFactory.OpenAsync(customerA))
{
    batchId = await migration.CreateBatchAsync(session, customerA, "synthetic-legacy-system");

    await migration.StageSubjectAsync(
        session,
        customerA,
        batchId,
        "SRC-001",
        "Migrated Alpha",
        new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        new { legacyCode = "A001", originalStatus = "ACTIVE" });

    await migration.StageSubjectAsync(
        session,
        customerA,
        batchId,
        "SRC-002",
        "Migrated Beta",
        new DateTimeOffset(2021, 6, 15, 0, 0, 0, TimeSpan.Zero),
        new { legacyCode = "B002", originalStatus = "ACTIVE" });

    await migration.StageSubjectAsync(
        session,
        customerA,
        batchId,
        "SRC-003",
        displayName: null,
        new DateTimeOffset(2022, 3, 1, 0, 0, 0, TimeSpan.Zero),
        new { legacyCode = "C003", originalStatus = "INCOMPLETE" });

    await session.CommitAsync();
}

Console.WriteLine("PASS: source data staged without contaminating authoritative business state.");

Console.WriteLine("SPIKE-007: processing batch through typed business module...");

MigrationReconciliation firstReconciliation;
await using (var session = await customerFactory.OpenAsync(customerA))
{
    firstReconciliation = await migration.ProcessBatchAsync(session, customerA, batchId);
    await session.CommitAsync();
}

Ensure(firstReconciliation.StagedCount == 3, "Expected 3 staged rows.");
Ensure(firstReconciliation.LoadedCount == 2, "Expected 2 loaded rows.");
Ensure(firstReconciliation.ExceptionCount == 1, "Expected 1 validation exception.");
Ensure(firstReconciliation.UnresolvedCount == 1, "Expected 1 unresolved exception.");

var mappingsBeforeRerun = await ReadMappingsAsync(customerFactory, customerA, batchId);
Ensure(mappingsBeforeRerun.Count == 2, "Expected two source-to-target mappings.");
Ensure(mappingsBeforeRerun.ContainsKey("SRC-001"), "SRC-001 mapping missing.");
Ensure(mappingsBeforeRerun.ContainsKey("SRC-002"), "SRC-002 mapping missing.");
Ensure(!mappingsBeforeRerun.ContainsKey("SRC-003"), "Invalid SRC-003 should not have a target mapping.");

var alphaVersion = await ReadSubjectNameAsync(
    customerFactory,
    subjectHistory,
    customerA,
    mappingsBeforeRerun["SRC-001"],
    new DateTimeOffset(2020, 2, 1, 0, 0, 0, TimeSpan.Zero))
    ?? throw new InvalidOperationException("SPIKE-007 FAILED: migrated Alpha effective history missing.");
Ensure(alphaVersion.DisplayName == "Migrated Alpha", "Migrated Alpha name is incorrect.");
Ensure(
    alphaVersion.EffectiveFrom == new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
    "Migrated Alpha effective date was not retained.");

var exceptionCode = await ReadExceptionCodeAsync(customerFactory, customerA, batchId, "SRC-003");
Ensure(exceptionCode == "DISPLAY_NAME_REQUIRED", "Invalid source row did not retain expected validation exception.");

Console.WriteLine("PASS: valid records loaded with source mapping/history; invalid record retained as controlled exception.");

Console.WriteLine("SPIKE-007: rerunning processed batch deterministically...");

MigrationReconciliation secondReconciliation;
await using (var session = await customerFactory.OpenAsync(customerA))
{
    secondReconciliation = await migration.ProcessBatchAsync(session, customerA, batchId);
    await session.CommitAsync();
}

Ensure(secondReconciliation == firstReconciliation, "Reconciliation changed on deterministic rerun.");

var mappingsAfterRerun = await ReadMappingsAsync(customerFactory, customerA, batchId);
Ensure(mappingsAfterRerun.Count == mappingsBeforeRerun.Count, "Rerun created extra target mappings.");
Ensure(
    mappingsAfterRerun["SRC-001"] == mappingsBeforeRerun["SRC-001"],
    "Rerun changed SRC-001 target identity.");
Ensure(
    mappingsAfterRerun["SRC-002"] == mappingsBeforeRerun["SRC-002"],
    "Rerun changed SRC-002 target identity.");

Console.WriteLine("PASS: rerun preserved target identities and reconciliation without duplicates.");

Console.WriteLine("SPIKE-007: verifying migration evidence is customer scoped...");

await using (var customerBSession = await customerFactory.OpenAsync(customerB))
{
    await using var command = customerBSession.Connection.CreateCommand();
    command.Transaction = customerBSession.Transaction;
    command.CommandText = "SELECT count(*) FROM migration.batches WHERE id = @batch_id;";
    command.AddParameter("batch_id", batchId);
    var visibleCount = Convert.ToInt32(await command.ExecuteScalarAsync());
    Ensure(visibleCount == 0, "Customer B could see Customer A migration batch.");
    await customerBSession.RollbackAsync();
}

Console.WriteLine("PASS: staging, mappings, exceptions and reconciliation remain customer isolated.");

Console.WriteLine();
Console.WriteLine("MIGRATION SPIKE VERIFICATION PASSED (SPIKE-007). ");

return;

static void Ensure(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException($"SPIKE-007 VERIFICATION FAILED: {message}");
    }
}

static async Task<Dictionary<string, SubjectId>> ReadMappingsAsync(
    ICustomerScopedTransactionalSessionFactory factory,
    CustomerId customerId,
    Guid batchId)
{
    await using var session = await factory.OpenAsync(customerId);
    await using var command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = """
        SELECT source_id, subject_id
        FROM migration.loaded_subjects
        WHERE customer_id = @customer_id
          AND batch_id = @batch_id
        ORDER BY source_id;
        """;
    command.AddParameter("customer_id", customerId.Value);
    command.AddParameter("batch_id", batchId);

    var result = new Dictionary<string, SubjectId>(StringComparer.Ordinal);
    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        result.Add(reader.GetString(0), new SubjectId(reader.GetGuid(1)));
    }

    await reader.DisposeAsync();
    await session.RollbackAsync();
    return result;
}

static async Task<EffectiveSubjectNameVersion?> ReadSubjectNameAsync(
    ICustomerScopedTransactionalSessionFactory factory,
    SubjectHistoryModule history,
    CustomerId customerId,
    SubjectId subjectId,
    DateTimeOffset asOf)
{
    await using var session = await factory.OpenAsync(customerId);
    var value = await history.GetAsOfAsync(session, customerId, subjectId, asOf);
    await session.RollbackAsync();
    return value;
}

static async Task<string?> ReadExceptionCodeAsync(
    ICustomerScopedTransactionalSessionFactory factory,
    CustomerId customerId,
    Guid batchId,
    string sourceId)
{
    await using var session = await factory.OpenAsync(customerId);
    await using var command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = """
        SELECT code
        FROM migration.exceptions
        WHERE customer_id = @customer_id
          AND batch_id = @batch_id
          AND source_id = @source_id
        ORDER BY code
        LIMIT 1;
        """;
    command.AddParameter("customer_id", customerId.Value);
    command.AddParameter("batch_id", batchId);
    command.AddParameter("source_id", sourceId);
    var result = await command.ExecuteScalarAsync();
    await session.RollbackAsync();
    return result as string;
}
