using NuBlox.FoundationSpike.Application;
using NuBlox.FoundationSpike.Audit;
using NuBlox.FoundationSpike.Infrastructure;
using NuBlox.FoundationSpike.Integration;
using NuBlox.FoundationSpike.Shared;
using NuBlox.FoundationSpike.Subjects;
using NuBlox.FoundationSpike.Work;

if (args.Length != 1 || args[0] is not ("seed" or "resume"))
{
    throw new ArgumentException("Run with seed or resume, in separate processes.");
}

var connectionString = Environment.GetEnvironmentVariable("NUBLOX_SPIKE_CONNECTION_STRING")
    ?? "Host=localhost;Port=55432;Database=nublox_spike;Username=nublox;Password=nublox_spike_dev_only";
var key = Environment.GetEnvironmentVariable("NUBLOX_SPIKE_OPERABILITY_KEY")
    ?? "operability-restart-001";
var customer = new CustomerId(Guid.Parse("88888888-8888-8888-8888-888888888888"));
var sessions = new PostgresCustomerScopedTransactionalSessionFactory(connectionString);
var outbox = new OutboxModule();

if (args[0] == "seed")
{
    var handler = new CreateWorkWithExternalHandoffHandler(
        sessions, new SubjectModule(), new WorkModule(), outbox, new AuditModule());
    var result = await handler.HandleAsync(new CreateWorkWithExternalHandoffCommand(
        customer,
        "Operability restart subject",
        "Keep external intent during worker shutdown",
        key,
        "simulated-provider"));

    var provider = new BlockingProvider();
    var worker = new OutboxWorker(sessions, outbox, provider);
    using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    var delivery = worker.ProcessOneAsync(customer, "stopping-worker", TimeSpan.Zero, shutdown.Token);
    await provider.Entered.WaitAsync(TimeSpan.FromSeconds(10));
    shutdown.Cancel();

    try
    {
        await delivery;
        throw new InvalidOperationException("Delivery unexpectedly completed during shutdown.");
    }
    catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
    {
        // The claim committed before delivery; the next process must recover it.
    }

    var message = await ReadMessageAsync(result.OutboxMessageId);
    Ensure(message.State == "PROCESSING" && message.AttemptCount == 1,
        "Cancelled worker did not leave a recoverable, committed claim.");
    Console.WriteLine("PASS: shutdown left a committed PROCESSING intent for another process.");
    return;
}

var messageId = await FindMessageIdAsync();
var stranded = await ReadMessageAsync(messageId);
Ensure(stranded.State == "PROCESSING" && stranded.AttemptCount == 1,
    "A separate process did not find the stranded intent.");

await using (var session = await sessions.OpenAsync(customer))
{
    var premature = await outbox.RecoverAbandonedAsync(
        session, customer, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow);
    Ensure(premature == 0, "A fresh worker lease was recovered prematurely.");
    await session.CommitAsync();
}

// Advance this one fixture's lock clock instead of waiting for a real lease timeout.
await using (var session = await sessions.OpenAsync(customer))
{
    await using var command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = """
        UPDATE integration.outbox
        SET locked_at = clock_timestamp() - interval '10 minutes'
        WHERE customer_id = @customer_id AND id = @id AND state = 'PROCESSING';
        """;
    command.AddParameter("customer_id", customer.Value);
    command.AddParameter("id", messageId.Value);
    Ensure(await command.ExecuteNonQueryAsync() == 1, "Could not age the test lease.");
    await session.CommitAsync();
}

await using (var session = await sessions.OpenAsync(customer))
{
    var recovered = await outbox.RecoverAbandonedAsync(
        session, customer, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddSeconds(-1));
    Ensure(recovered == 1, "Expired worker lease was not recovered exactly once.");
    await session.CommitAsync();
}

var replacement = new OutboxWorker(sessions, outbox, new SuccessfulProvider());
Ensure(await replacement.ProcessOneAsync(customer, "replacement-worker", TimeSpan.Zero) == messageId,
    "Replacement worker did not claim the recovered intent.");
var completed = await ReadMessageAsync(messageId);
Ensure(completed.State == "SUCCEEDED" && completed.AttemptCount == 2,
    "Replacement worker did not durably complete the second attempt.");
Ensure(completed.ProviderRequestId == $"operability-{messageId.Value:N}",
    "Provider correlation was not retained on the outbox intent.");
Console.WriteLine("PASS: separate process recovered the aged lease and completed the intent.");

return;

async Task<OutboxMessageId> FindMessageIdAsync()
{
    await using var session = await sessions.OpenAsync(customer);
    await using var command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;
    command.CommandText = """
        SELECT id FROM integration.outbox
        WHERE customer_id = @customer_id AND idempotency_key = @key;
        """;
    command.AddParameter("customer_id", customer.Value);
    command.AddParameter("key", key);
    var found = await command.ExecuteScalarAsync();
    await session.RollbackAsync();
    return new OutboxMessageId(found is Guid id
        ? id
        : throw new InvalidOperationException("Seed process did not leave an outbox intent."));
}

async Task<OutboxMessage> ReadMessageAsync(OutboxMessageId id)
{
    await using var session = await sessions.OpenAsync(customer);
    var message = await outbox.GetAsync(session, customer, id);
    await session.RollbackAsync();
    return message ?? throw new InvalidOperationException("Outbox intent was not found.");
}

static void Ensure(bool condition, string explanation)
{
    if (!condition)
    {
        throw new InvalidOperationException($"SPIKE-009 VERIFICATION FAILED: {explanation}");
    }
}

sealed class BlockingProvider : IExternalDeliveryClient
{
    private readonly TaskCompletionSource<bool> _entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task Entered => _entered.Task;

    public async Task<DeliveryResult> DeliverAsync(
        OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _entered.SetResult(true);
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        throw new InvalidOperationException("The blocking provider should only stop on cancellation.");
    }
}

sealed class SuccessfulProvider : IExternalDeliveryClient
{
    public Task<DeliveryResult> DeliverAsync(
        OutboxMessage message, CancellationToken cancellationToken = default)
        => Task.FromResult(new DeliveryResult(
            DeliveryOutcome.Success, $"operability-{message.Id.Value:N}"));
}
