using System.Diagnostics;
using NuBlox.FoundationSpike.Infrastructure;
using NuBlox.FoundationSpike.Reporting;
using NuBlox.FoundationSpike.Shared;

var connectionString = Environment.GetEnvironmentVariable("NUBLOX_SPIKE_CONNECTION_STRING")
    ?? "Host=localhost;Port=55432;Database=nublox_spike;Username=nublox;Password=nublox_spike_dev_only";

var admin = new PostgresTransactionalSessionFactory(connectionString);
var scoped = new PostgresCustomerScopedTransactionalSessionFactory(connectionString);
var reports = new OperationalWorkReports();
var customerA = CustomerId.New();
var customerB = CustomerId.New();
var subjectA = SubjectId.New();
var subjectB = SubjectId.New();
var earlyWork = WorkId.New();
var completedWork = WorkId.New();
var laterWork = WorkId.New();
var otherWork = WorkId.New();
var may2025 = new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero);
var july2025 = new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero);
var feb2027 = new DateTimeOffset(2027, 2, 1, 0, 0, 0, TimeSpan.Zero);

// Direct SQL seeds fixed historical timestamps solely in this disposable
// verifier. Product writes continue through WorkModule's normal timestamp.
await using (var session = await admin.OpenAsync())
{
    await using var command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = """
        INSERT INTO subjects.business_subjects (customer_id, id)
        VALUES (@customer_a, @subject_a), (@customer_b, @subject_b);

        INSERT INTO work.work_requests (customer_id, id, subject_id, summary, state, created_at)
        VALUES
            (@customer_a, @early_id, @subject_a, 'Early open work', 'OPEN', '2025-01-01 00:00:00+00'),
            (@customer_a, @completed_id, @subject_a, 'Completed work', 'COMPLETED', '2025-06-01 00:00:00+00'),
            (@customer_a, @later_id, @subject_a, 'Later open work', 'OPEN', '2026-01-01 00:00:00+00'),
            (@customer_b, @other_id, @subject_b, 'Other customer work', 'OPEN', '2025-02-01 00:00:00+00');
        """;
    command.AddParameter("customer_a", customerA.Value);
    command.AddParameter("customer_b", customerB.Value);
    command.AddParameter("subject_a", subjectA.Value);
    command.AddParameter("subject_b", subjectB.Value);
    command.AddParameter("early_id", earlyWork.Value);
    command.AddParameter("completed_id", completedWork.Value);
    command.AddParameter("later_id", laterWork.Value);
    command.AddParameter("other_id", otherWork.Value);
    await command.ExecuteNonQueryAsync();
    await session.CommitAsync();
}

Console.WriteLine("SPIKE-008: checking controlled measure definitions, time basis and drill-through...");
Ensure(OperationalWorkReports.CreatedThroughDefinition.Source == "work.work_requests", "Created measure source missing.");
Ensure(OperationalWorkReports.CreatedThroughDefinition.TimeBasis.Contains("creation", StringComparison.OrdinalIgnoreCase),
    "Created measure must disclose creation time basis.");

await using (var session = await scoped.OpenAsync(customerA))
{
    Ensure(await reports.CountCreatedThroughAsync(session, customerA, may2025) == 1,
        "May cutoff should include only the early work.");
    Ensure(await reports.CountCreatedThroughAsync(session, customerA, july2025) == 2,
        "July cutoff should include early and completed work.");
    Ensure(await reports.CountCurrentOpenAsync(session, customerA) == 2,
        "Current OPEN count should include early and later work, independent of historical cutoff.");

    var firstPage = await reports.ListCreatedThroughAsync(session, customerA, july2025, 0, 1);
    var secondPage = await reports.ListCreatedThroughAsync(session, customerA, july2025, 1, 1);
    Ensure(firstPage.Count == 1 && firstPage[0].WorkId == earlyWork, "First drill-through page is wrong.");
    Ensure(secondPage.Count == 1 && secondPage[0].WorkId == completedWork, "Second drill-through page is wrong.");
    Ensure(firstPage[0].SubjectId == subjectA && firstPage[0].Summary == "Early open work",
        "Drill-through lost the authoritative source context.");

    var open = await reports.ListCurrentOpenAsync(session, customerA, 0, 10);
    Ensure(open.Count == 2 && open.Any(row => row.WorkId == earlyWork) && open.Any(row => row.WorkId == laterWork),
        "Current OPEN drill-through does not reconcile to its count.");
    Ensure(!open.Any(row => row.WorkId == completedWork), "Completed work appeared in OPEN detail.");
    await session.RollbackAsync();
}

Console.WriteLine("PASS: creation cutoffs and current state are distinct; paged source rows reconcile to both counts.");

Console.WriteLine("SPIKE-008: checking aggregate and detail isolation under restricted role...");
await using (var session = await scoped.OpenAsync(customerB))
{
    Ensure(await reports.CountCreatedThroughAsync(session, customerB, july2025) == 1,
        "Customer B should have one created work item.");
    Ensure(await reports.CountCurrentOpenAsync(session, customerB) == 1,
        "Customer B should have one current OPEN item.");
    var rows = await reports.ListCreatedThroughAsync(session, customerB, july2025, 0, 10);
    Ensure(rows.Count == 1 && rows[0].WorkId == otherWork, "Customer B detail crossed customer boundary.");
    Ensure(await reports.CountCreatedThroughAsync(session, customerA, july2025) == 0,
        "Explicit customer A parameter bypassed customer B's RLS context.");
    Ensure((await reports.ListCreatedThroughAsync(session, customerA, july2025, 0, 10)).Count == 0,
        "Explicit customer A detail bypassed customer B's RLS context.");
    await session.RollbackAsync();
}

await using (var session = await admin.OpenAsync())
{
    await using (var command = session.Connection.CreateCommand())
    {
        command.Transaction = session.Transaction;
        command.CommandText = "SET LOCAL ROLE nublox_app;";
        await command.ExecuteNonQueryAsync();
    }

    Ensure(await reports.CountCreatedThroughAsync(session, customerA, july2025) == 0,
        "Missing customer context exposed an aggregate.");
    Ensure((await reports.ListCreatedThroughAsync(session, customerA, july2025, 0, 10)).Count == 0,
        "Missing customer context exposed detail.");
    await session.RollbackAsync();
}

Console.WriteLine("PASS: aggregate and detail queries fail closed for wrong or missing customer context.");

Console.WriteLine("SPIKE-008: seeding 20,000 synthetic work records and inspecting query plans...");
await using (var session = await admin.OpenAsync())
{
    await using var command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = """
        INSERT INTO work.work_requests (customer_id, id, subject_id, summary, state, created_at)
        SELECT @customer_id, gen_random_uuid(), @subject_id, 'Synthetic work ' || g,
               CASE WHEN g % 3 = 0 THEN 'COMPLETED' ELSE 'OPEN' END,
               '2025-01-01 00:00:00+00'::timestamptz + (g % 365) * interval '1 day'
        FROM generate_series(1, 20000) AS g;
        """;
    command.AddParameter("customer_id", customerA.Value);
    command.AddParameter("subject_id", subjectA.Value);
    Ensure(await command.ExecuteNonQueryAsync() == 20000, "Synthetic dataset is incomplete.");
    await session.CommitAsync();
}

await using (var session = await scoped.OpenAsync(customerA))
{
    var timer = Stopwatch.StartNew();
    var created = await reports.CountCreatedThroughAsync(session, customerA, feb2027);
    timer.Stop();
    Console.WriteLine($"Created count at 20,003 rows: {timer.Elapsed.TotalMilliseconds:F2} ms");
    Ensure(created == 20003, "Cumulative count at volume is wrong.");
    Ensure(timer.Elapsed < TimeSpan.FromSeconds(5), "Cumulative count exceeded the spike's 5-second CI bound.");

    timer.Restart();
    var detail = await reports.ListCreatedThroughAsync(session, customerA, feb2027, 0, 50);
    timer.Stop();
    Console.WriteLine($"First 50 source records at 20,003 rows: {timer.Elapsed.TotalMilliseconds:F2} ms");
    Ensure(detail.Count == 50 && detail.All(row => row.SubjectId == subjectA),
        "Volume drill-through returned an incomplete or unrelated page.");
    Ensure(timer.Elapsed < TimeSpan.FromSeconds(5), "Drill-through exceeded the spike's 5-second CI bound.");

    Ensure(await reports.CountCurrentOpenAsync(session, customerA) == 13336,
        "Current OPEN count at volume is wrong.");

    Console.WriteLine("COUNT query plan with buffers:");
    Console.WriteLine(await reports.ExplainCreatedThroughAsync(session, customerA, feb2027, details: false));
    Console.WriteLine("FIRST-PAGE drill-through query plan with buffers:");
    Console.WriteLine(await reports.ExplainCreatedThroughAsync(session, customerA, feb2027, details: true));
    await session.RollbackAsync();
}

Console.WriteLine("REPORTING SPIKE VERIFICATION PASSED (SPIKE-008).");

static void Ensure(bool condition, string reason)
{
    if (!condition)
    {
        throw new InvalidOperationException($"SPIKE-008 VERIFICATION FAILED: {reason}");
    }
}
