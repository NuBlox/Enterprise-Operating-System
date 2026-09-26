using NuBlox.FoundationSpike.Configuration;
using NuBlox.FoundationSpike.Infrastructure;
using NuBlox.FoundationSpike.Shared;

var connectionString = Environment.GetEnvironmentVariable("NUBLOX_SPIKE_CONNECTION_STRING")
    ?? "Host=localhost;Port=55432;Database=nublox_spike;Username=nublox;Password=nublox_spike_dev_only";

var customerFactory = new PostgresCustomerScopedTransactionalSessionFactory(connectionString);
var policies = new WorkPolicyModule();

var customerA = new CustomerId(Guid.Parse("55555555-5555-5555-5555-555555555555"));
var customerB = new CustomerId(Guid.Parse("66666666-6666-6666-6666-666666666666"));
var initialEffective = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
var changeEffective = new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero);

Console.WriteLine("SPIKE-006: verifying typed customer configuration and effective history...");

await using (var session = await customerFactory.OpenAsync(customerA))
{
    await policies.ActivateAsync(
        session,
        customerA,
        approvalRequired: true,
        maxOpenWork: 50,
        externalCollaborationEnabled: false,
        effectiveFrom: initialEffective,
        changeReason: "Initial customer work policy");

    await policies.ActivateAsync(
        session,
        customerA,
        approvalRequired: false,
        maxOpenWork: 100,
        externalCollaborationEnabled: true,
        effectiveFrom: changeEffective,
        changeReason: "Approved future policy change");

    await session.CommitAsync();
}

var marchPolicy = await GetPolicyAsync(
    customerFactory,
    policies,
    customerA,
    new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero))
    ?? throw new InvalidOperationException("SPIKE-006 FAILED: March policy was not found.");

var augustPolicy = await GetPolicyAsync(
    customerFactory,
    policies,
    customerA,
    new DateTimeOffset(2025, 8, 1, 0, 0, 0, TimeSpan.Zero))
    ?? throw new InvalidOperationException("SPIKE-006 FAILED: August policy was not found.");

Ensure(marchPolicy.ApprovalRequired, "Initial policy did not require approval.");
Ensure(marchPolicy.MaxOpenWork == 50, "Initial max-open-work value was incorrect.");
Ensure(!marchPolicy.ExternalCollaborationEnabled, "Initial external collaboration value was incorrect.");
Ensure(marchPolicy.AuditRequired, "Mandatory audit invariant was not true in initial configuration.");
Ensure(marchPolicy.EffectiveTo == changeEffective, "Initial version was not closed at future policy effective date.");

Ensure(!augustPolicy.ApprovalRequired, "Future policy did not apply changed approval behaviour.");
Ensure(augustPolicy.MaxOpenWork == 100, "Future max-open-work value was incorrect.");
Ensure(augustPolicy.ExternalCollaborationEnabled, "Future external collaboration value was incorrect.");
Ensure(augustPolicy.AuditRequired, "Mandatory audit invariant was not preserved after configuration change.");
Ensure(augustPolicy.EffectiveFrom == changeEffective, "Future configuration effective date was not preserved.");
Ensure(augustPolicy.RecordedAt > changeEffective, "Configuration recording time was not distinct from business-effective time.");

await using (var session = await customerFactory.OpenAsync(customerA))
{
    var versions = await policies.GetAllVersionsAsync(session, customerA);
    Ensure(versions.Count == 2, $"Expected two Customer A policy versions; found {versions.Count}.");
    await session.RollbackAsync();
}

Console.WriteLine("PASS: typed customer policy supports controlled future-effective variation with history.");

Console.WriteLine("SPIKE-006: verifying independent customer variation and RLS isolation...");

await using (var session = await customerFactory.OpenAsync(customerB))
{
    await policies.ActivateAsync(
        session,
        customerB,
        approvalRequired: true,
        maxOpenWork: 10,
        externalCollaborationEnabled: true,
        effectiveFrom: initialEffective,
        changeReason: "Customer B independent policy");
    await session.CommitAsync();
}

var customerBPolicy = await GetPolicyAsync(
    customerFactory,
    policies,
    customerB,
    new DateTimeOffset(2025, 8, 1, 0, 0, 0, TimeSpan.Zero))
    ?? throw new InvalidOperationException("SPIKE-006 FAILED: Customer B policy was not found.");
Ensure(customerBPolicy.MaxOpenWork == 10, "Customer B policy did not remain independent from Customer A.");

await using (var customerASession = await customerFactory.OpenAsync(customerA))
{
    var customerBFromAContext = await policies.GetAsOfAsync(
        customerASession,
        customerB,
        new DateTimeOffset(2025, 8, 1, 0, 0, 0, TimeSpan.Zero));
    Ensure(customerBFromAContext is null, "Customer A context could read Customer B configuration.");
    await customerASession.RollbackAsync();
}

Console.WriteLine("PASS: customers can vary typed policy independently while RLS hides other-customer configuration.");

Console.WriteLine("SPIKE-006: verifying invalid typed values are rejected before persistence...");

var invalidRangeRejected = false;
await using (var session = await customerFactory.OpenAsync(customerA))
{
    try
    {
        await policies.ActivateAsync(
            session,
            customerA,
            approvalRequired: true,
            maxOpenWork: 0,
            externalCollaborationEnabled: false,
            effectiveFrom: new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero),
            changeReason: "Invalid configuration test");
    }
    catch (ArgumentOutOfRangeException)
    {
        invalidRangeRejected = true;
    }

    await session.RollbackAsync();
}
Ensure(invalidRangeRejected, "Typed configuration accepted maxOpenWork = 0.");

Console.WriteLine("PASS: invalid typed configuration was rejected by the module boundary.");

Console.WriteLine("SPIKE-006: verifying mandatory audit invariant cannot be configured off...");

var auditDisableRejected = false;
await using (var session = await customerFactory.OpenAsync(customerA))
{
    try
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO configuration.work_policy_versions
                (
                    customer_id,
                    version_id,
                    effective_from,
                    effective_to,
                    approval_required,
                    max_open_work,
                    external_collaboration_enabled,
                    audit_required,
                    change_reason
                )
            VALUES
                (
                    @customer_id,
                    @version_id,
                    @effective_from,
                    @effective_to,
                    true,
                    25,
                    false,
                    false,
                    'Attempt to disable mandatory audit'
                );
            """;
        command.AddParameter("customer_id", customerA.Value);
        command.AddParameter("version_id", Guid.NewGuid());
        command.AddParameter("effective_from", new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        command.AddParameter("effective_to", new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero));

        await command.ExecuteNonQueryAsync();
    }
    catch (Exception exception)
    {
        auditDisableRejected = true;
        Console.WriteLine($"Expected mandatory-invariant rejection: {exception.GetType().Name}");
    }

    await session.RollbackAsync();
}

Ensure(auditDisableRejected, "Database accepted configuration with audit_required = false.");
Console.WriteLine("PASS: mandatory audit invariant cannot be disabled even by a direct application-role insert.");

Console.WriteLine();
Console.WriteLine("CONFIGURATION SPIKE VERIFICATION PASSED (SPIKE-006). ");

return;

static void Ensure(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException($"SPIKE-006 VERIFICATION FAILED: {message}");
    }
}

static async Task<WorkPolicyVersion?> GetPolicyAsync(
    ICustomerScopedTransactionalSessionFactory factory,
    WorkPolicyModule policies,
    CustomerId customerId,
    DateTimeOffset effectiveAt)
{
    await using var session = await factory.OpenAsync(customerId);
    var result = await policies.GetAsOfAsync(session, customerId, effectiveAt);
    await session.RollbackAsync();
    return result;
}
