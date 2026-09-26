using System.Data.Common;
using NuBlox.FoundationSpike.Application;
using NuBlox.FoundationSpike.Audit;
using NuBlox.FoundationSpike.Infrastructure;
using NuBlox.FoundationSpike.Integration;
using NuBlox.FoundationSpike.Shared;
using NuBlox.FoundationSpike.Subjects;
using NuBlox.FoundationSpike.Work;

var connectionString = Environment.GetEnvironmentVariable("NUBLOX_SPIKE_CONNECTION_STRING")
    ?? "Host=localhost;Port=55432;Database=nublox_spike;Username=nublox;Password=nublox_spike_dev_only";

var adminFactory = new PostgresTransactionalSessionFactory(connectionString);
var customerFactory = new PostgresCustomerScopedTransactionalSessionFactory(connectionString);
var subjects = new SubjectModule();
var work = new WorkModule();
var outbox = new OutboxModule();
var audit = new AuditModule();
var handler = new CreateWorkWithExternalHandoffHandler(
    customerFactory,
    subjects,
    work,
    outbox,
    audit);

var provider = new IdempotentFakeProvider();
var worker = new OutboxWorker(customerFactory, outbox, provider);

var customerA = new CustomerId(Guid.Parse("33333333-3333-3333-3333-333333333333"));
var customerB = new CustomerId(Guid.Parse("44444444-4444-4444-4444-444444444444"));

Console.WriteLine("SPIKE-005: verifying business state and outbound intent commit atomically...");

var atomic = await handler.HandleAsync(
    new CreateWorkWithExternalHandoffCommand(
        customerA,
        "Outbox atomic subject",
        "Create local work and durable external intent",
        "atomic-handoff-001",
        "external-system-A"));

Ensure(
    await ExistsAsync(adminFactory, "subjects.business_subjects", customerA.Value, atomic.SubjectId.Value),
    "Committed subject was not found.");
Ensure(
    await ExistsAsync(adminFactory, "work.work_requests", customerA.Value, atomic.WorkId.Value),
    "Committed work record was not found.");

var atomicMessage = await GetMessageAsync(customerFactory, outbox, customerA, atomic.OutboxMessageId)
    ?? throw new InvalidOperationException("SPIKE VERIFICATION FAILED: committed outbox intent was not found.");
Ensure(atomicMessage.State == "PENDING", "New outbox intent was not PENDING after local commit.");
Ensure(atomicMessage.AttemptCount == 0, "New outbox intent unexpectedly has delivery attempts.");

Console.WriteLine("PASS: committed business state includes durable PENDING external intent.");

var processedAtomic = await worker.ProcessOneAsync(customerA, "worker-A", TimeSpan.Zero);
Ensure(processedAtomic == atomic.OutboxMessageId, "Atomic-case message was not the message claimed for delivery.");
var atomicAfterDelivery = await GetMessageAsync(customerFactory, outbox, customerA, atomic.OutboxMessageId)
    ?? throw new InvalidOperationException("Atomic outbox message disappeared after delivery.");
Ensure(atomicAfterDelivery.State == "SUCCEEDED", "Atomic-case message did not complete before later scenarios.");
Ensure(provider.GetBusinessEffectCount("atomic-handoff-001") == 1, "Atomic-case delivery did not produce exactly one external effect.");

Console.WriteLine("PASS: initial atomic case was completed before isolated failure/retry scenarios.");

Console.WriteLine("SPIKE-005: verifying rollback removes local state and outbound intent together...");

SubjectId rollbackSubjectId;
WorkId rollbackWorkId;
OutboxMessageId rollbackOutboxId;

await using (var session = await customerFactory.OpenAsync(customerA))
{
    rollbackSubjectId = await subjects.CreateAsync(
        session,
        customerA,
        "Outbox rollback subject");
    rollbackWorkId = await work.CreateAsync(
        session,
        customerA,
        rollbackSubjectId,
        "This work must roll back with its intent");
    rollbackOutboxId = await outbox.EnqueueAsync(
        session,
        customerA,
        "rollback-handoff-001",
        "EXTERNAL_WORK_HANDOFF",
        "work_request",
        rollbackWorkId.Value,
        new { workId = rollbackWorkId.Value, destinationReference = "external-system-A" });

    await session.RollbackAsync();
}

Ensure(
    !await ExistsAsync(adminFactory, "subjects.business_subjects", customerA.Value, rollbackSubjectId.Value),
    "Rolled-back subject still exists.");
Ensure(
    !await ExistsAsync(adminFactory, "work.work_requests", customerA.Value, rollbackWorkId.Value),
    "Rolled-back work still exists.");
Ensure(
    await GetMessageAsync(customerFactory, outbox, customerA, rollbackOutboxId) is null,
    "Rolled-back outbox intent still exists.");

Console.WriteLine("PASS: rollback removed business state and outbound intent atomically.");

Console.WriteLine("SPIKE-005: verifying transient provider failure is retried safely...");

const string transientKey = "transient-handoff-001";
var transient = await handler.HandleAsync(
    new CreateWorkWithExternalHandoffCommand(
        customerA,
        "Transient provider subject",
        "Retry a transient external failure",
        transientKey,
        "external-system-transient"));
provider.FailTransientlyOnce(transientKey);

await worker.ProcessOneAsync(customerA, "worker-A", TimeSpan.Zero);
var afterTransientFailure = await GetMessageAsync(customerFactory, outbox, customerA, transient.OutboxMessageId)
    ?? throw new InvalidOperationException("Transient outbox message disappeared after retryable failure.");
Ensure(afterTransientFailure.State == "RETRY_WAIT", "Transient failure did not move to RETRY_WAIT.");
Ensure(afterTransientFailure.AttemptCount == 1, "Transient failure did not record first attempt.");
Ensure(provider.GetBusinessEffectCount(transientKey) == 0, "Transient failure produced an external business effect.");

await worker.ProcessOneAsync(customerA, "worker-A", TimeSpan.Zero);
var afterTransientSuccess = await GetMessageAsync(customerFactory, outbox, customerA, transient.OutboxMessageId)
    ?? throw new InvalidOperationException("Transient outbox message disappeared after successful retry.");
Ensure(afterTransientSuccess.State == "SUCCEEDED", "Successful retry did not mark SUCCEEDED.");
Ensure(afterTransientSuccess.AttemptCount == 2, "Successful retry did not preserve attempt count.");
Ensure(provider.GetBusinessEffectCount(transientKey) == 1, "Successful retry did not create exactly one external business effect.");

Console.WriteLine("PASS: transient failure moved to RETRY_WAIT and later succeeded exactly once externally.");

Console.WriteLine("SPIKE-005: verifying permanent provider rejection becomes actionable failure state...");

const string permanentKey = "permanent-handoff-001";
var permanent = await handler.HandleAsync(
    new CreateWorkWithExternalHandoffCommand(
        customerA,
        "Permanent rejection subject",
        "Record permanent external rejection",
        permanentKey,
        "external-system-reject"));
provider.FailPermanently(permanentKey);

await worker.ProcessOneAsync(customerA, "worker-A", TimeSpan.Zero);
var afterPermanentFailure = await GetMessageAsync(customerFactory, outbox, customerA, permanent.OutboxMessageId)
    ?? throw new InvalidOperationException("Permanent outbox message disappeared.");
Ensure(
    afterPermanentFailure.State == "FAILED_ACTION_REQUIRED",
    "Permanent rejection did not move to FAILED_ACTION_REQUIRED.");
Ensure(afterPermanentFailure.AttemptCount == 1, "Permanent rejection did not record its attempt.");
Ensure(provider.GetBusinessEffectCount(permanentKey) == 0, "Permanent rejection produced an external business effect.");

Console.WriteLine("PASS: permanent rejection is retained as actionable failed state.");

Console.WriteLine("SPIKE-005: verifying crash after provider success can recover without duplicate external effect...");

const string crashKey = "crash-after-provider-001";
var crashCase = await handler.HandleAsync(
    new CreateWorkWithExternalHandoffCommand(
        customerA,
        "Crash recovery subject",
        "Recover after provider success before local acknowledgement",
        crashKey,
        "external-system-crash"));

OutboxMessage claimed;
await using (var claimSession = await customerFactory.OpenAsync(customerA))
{
    claimed = await outbox.ClaimNextAsync(
        claimSession,
        customerA,
        "crashing-worker")
        ?? throw new InvalidOperationException("Expected crash-case message to be claimable.");
    await claimSession.CommitAsync();
}

var providerSuccessBeforeCrash = await provider.DeliverAsync(claimed);
Ensure(providerSuccessBeforeCrash.Outcome == DeliveryOutcome.Success, "Crash-case provider call did not succeed.");
Ensure(provider.GetBusinessEffectCount(crashKey) == 1, "Crash-case provider did not record exactly one effect before simulated crash.");

// Simulate process termination here: intentionally do NOT mark the claimed row succeeded.
await using (var recoverySession = await customerFactory.OpenAsync(customerA))
{
    var recovered = await outbox.RecoverAbandonedAsync(
        recoverySession,
        customerA,
        lockedBefore: DateTimeOffset.UtcNow.AddMinutes(1),
        retryAt: DateTimeOffset.UtcNow.AddSeconds(-1));
    Ensure(recovered == 1, $"Expected one abandoned message to recover; recovered {recovered}.");
    await recoverySession.CommitAsync();
}

await worker.ProcessOneAsync(customerA, "replacement-worker", TimeSpan.Zero);
var afterCrashRecovery = await GetMessageAsync(customerFactory, outbox, customerA, crashCase.OutboxMessageId)
    ?? throw new InvalidOperationException("Crash-recovered outbox message disappeared.");
Ensure(afterCrashRecovery.State == "SUCCEEDED", "Recovered message did not reach SUCCEEDED.");
Ensure(afterCrashRecovery.AttemptCount == 2, "Recovered message did not record the second claim attempt.");
Ensure(provider.GetCallCount(crashKey) == 2, "Crash-case provider was not called twice as expected after recovery.");
Ensure(provider.GetBusinessEffectCount(crashKey) == 1, "Idempotent retry created a duplicate external business effect.");
Ensure(
    afterCrashRecovery.ProviderRequestId == provider.GetProviderRequestId(crashKey),
    "Recovered retry did not retain the provider's stable idempotent request result.");

Console.WriteLine("PASS: abandoned PROCESSING message recovered and redelivery remained externally idempotent.");

Console.WriteLine("SPIKE-005: verifying background processing preserves customer context...");

var customerBPending = await handler.HandleAsync(
    new CreateWorkWithExternalHandoffCommand(
        customerB,
        "Customer B outbox subject",
        "Remain invisible to Customer A worker",
        "customer-b-pending-001",
        "external-system-B"));

var noCustomerAMessage = await worker.ProcessOneAsync(customerA, "worker-A", TimeSpan.Zero);
Ensure(noCustomerAMessage is null, "Customer A worker claimed work after all Customer A messages were terminal.");

var customerBMessage = await GetMessageAsync(customerFactory, outbox, customerB, customerBPending.OutboxMessageId)
    ?? throw new InvalidOperationException("Customer B pending outbox message was not found in Customer B context.");
Ensure(customerBMessage.State == "PENDING", "Customer A worker changed Customer B pending outbox message.");

await using (var customerASession = await customerFactory.OpenAsync(customerA))
{
    var invisible = await outbox.GetAsync(customerASession, customerA, customerBPending.OutboxMessageId);
    Ensure(invisible is null, "Customer A context could see Customer B outbox message.");
    await customerASession.RollbackAsync();
}

Console.WriteLine("PASS: background worker and outbox reads remain customer scoped.");

Console.WriteLine();
Console.WriteLine("INTEGRATION SPIKE VERIFICATION PASSED (SPIKE-005). ");

return;

static void Ensure(bool condition, string failureMessage)
{
    if (!condition)
    {
        throw new InvalidOperationException($"SPIKE-005 VERIFICATION FAILED: {failureMessage}");
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

static async Task<OutboxMessage?> GetMessageAsync(
    ICustomerScopedTransactionalSessionFactory factory,
    OutboxModule outbox,
    CustomerId customerId,
    OutboxMessageId messageId)
{
    await using var session = await factory.OpenAsync(customerId);
    var message = await outbox.GetAsync(session, customerId, messageId);
    await session.RollbackAsync();
    return message;
}

sealed class IdempotentFakeProvider : IExternalDeliveryClient
{
    private readonly HashSet<string> _failTransientlyOnce = new(StringComparer.Ordinal);
    private readonly HashSet<string> _failPermanently = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _successfulRequests = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _callCounts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _businessEffects = new(StringComparer.Ordinal);
    private int _providerSequence;

    public void FailTransientlyOnce(string idempotencyKey)
        => _failTransientlyOnce.Add(idempotencyKey);

    public void FailPermanently(string idempotencyKey)
        => _failPermanently.Add(idempotencyKey);

    public int GetCallCount(string idempotencyKey)
        => _callCounts.TryGetValue(idempotencyKey, out var count) ? count : 0;

    public int GetBusinessEffectCount(string idempotencyKey)
        => _businessEffects.TryGetValue(idempotencyKey, out var count) ? count : 0;

    public string? GetProviderRequestId(string idempotencyKey)
        => _successfulRequests.TryGetValue(idempotencyKey, out var value) ? value : null;

    public Task<DeliveryResult> DeliverAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _callCounts[message.IdempotencyKey] = GetCallCount(message.IdempotencyKey) + 1;

        if (_failPermanently.Contains(message.IdempotencyKey))
        {
            return Task.FromResult(new DeliveryResult(
                DeliveryOutcome.PermanentFailure,
                ProviderRequestId: $"rejection-{message.IdempotencyKey}",
                Error: "Provider rejected the operation permanently."));
        }

        if (_failTransientlyOnce.Remove(message.IdempotencyKey))
        {
            return Task.FromResult(new DeliveryResult(
                DeliveryOutcome.TransientFailure,
                Error: "Simulated transient provider timeout."));
        }

        if (_successfulRequests.TryGetValue(message.IdempotencyKey, out var existingRequestId))
        {
            return Task.FromResult(new DeliveryResult(
                DeliveryOutcome.Success,
                ProviderRequestId: existingRequestId));
        }

        var providerRequestId = $"provider-{Interlocked.Increment(ref _providerSequence):D4}";
        _successfulRequests.Add(message.IdempotencyKey, providerRequestId);
        _businessEffects[message.IdempotencyKey] = GetBusinessEffectCount(message.IdempotencyKey) + 1;

        return Task.FromResult(new DeliveryResult(
            DeliveryOutcome.Success,
            ProviderRequestId: providerRequestId));
    }
}
