using NuBlox.FoundationSpike.Authority;
using NuBlox.FoundationSpike.Infrastructure;
using NuBlox.FoundationSpike.Shared;

var connectionString = Environment.GetEnvironmentVariable("NUBLOX_SPIKE_CONNECTION_STRING")
    ?? "Host=localhost;Port=55432;Database=nublox_spike;Username=nublox;Password=nublox_spike_dev_only";

var sessions = new PostgresCustomerScopedTransactionalSessionFactory(connectionString);
var authority = new AuthorityModule();
var customer = new CustomerId(Guid.Parse("33333333-3333-3333-3333-333333333333"));
var evaluationAt = new DateTimeOffset(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);

Console.WriteLine("SPIKE-004: verifying permission does not itself grant business authority...");

var permissionOnlyPrincipal = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
await using (var session = await sessions.OpenAsync(customer))
{
    await authority.GrantApprovePermissionAsync(
        session,
        customer,
        permissionOnlyPrincipal,
        evaluationAt.AddDays(-30));
    await session.CommitAsync();
}

AuthorityEvaluationResult permissionOnlyResult;
await using (var session = await sessions.OpenAsync(customer))
{
    permissionOnlyResult = await authority.EvaluateApprovalAsync(
        session,
        customer,
        permissionOnlyPrincipal,
        "COMMERCIAL_COMMITMENT",
        1_000m,
        evaluationAt);
    await session.CommitAsync();
}

Ensure(!permissionOnlyResult.Allowed, "Permission-only principal was incorrectly authorised.");
Ensure(
    permissionOnlyResult.Reason == "BUSINESS_AUTHORITY_MISSING_OR_THRESHOLD_EXCEEDED",
    "Permission-only denial reason was not explicit.");
Console.WriteLine("PASS: technical permission alone does not grant approval authority.");

Console.WriteLine("SPIKE-004: verifying authority does not itself grant technical permission...");

var authorityOnlyPrincipal = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
await using (var session = await sessions.OpenAsync(customer))
{
    await authority.GrantDecisionAuthorityAsync(
        session,
        customer,
        authorityOnlyPrincipal,
        "COMMERCIAL_COMMITMENT",
        10_000m,
        evaluationAt.AddDays(-30),
        null,
        "Authority-only test grant");
    await session.CommitAsync();
}

AuthorityEvaluationResult authorityOnlyResult;
await using (var session = await sessions.OpenAsync(customer))
{
    authorityOnlyResult = await authority.EvaluateApprovalAsync(
        session,
        customer,
        authorityOnlyPrincipal,
        "COMMERCIAL_COMMITMENT",
        1_000m,
        evaluationAt);
    await session.CommitAsync();
}

Ensure(!authorityOnlyResult.Allowed, "Authority-only principal was incorrectly allowed without operation permission.");
Ensure(
    authorityOnlyResult.Reason == "OPERATION_PERMISSION_MISSING",
    "Authority-only denial reason was not explicit.");
Console.WriteLine("PASS: business authority does not bypass technical operation permission.");

Console.WriteLine("SPIKE-004: verifying valid permission + in-scope authority is allowed...");

var authorisedPrincipal = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
await using (var session = await sessions.OpenAsync(customer))
{
    await authority.GrantApprovePermissionAsync(
        session,
        customer,
        authorisedPrincipal,
        evaluationAt.AddDays(-30));

    await authority.GrantDecisionAuthorityAsync(
        session,
        customer,
        authorisedPrincipal,
        "COMMERCIAL_COMMITMENT",
        10_000m,
        evaluationAt.AddDays(-30),
        evaluationAt.AddDays(30),
        "Delegated commercial approval authority",
        Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"));

    await session.CommitAsync();
}

AuthorityEvaluationResult inScopeResult;
await using (var session = await sessions.OpenAsync(customer))
{
    inScopeResult = await authority.EvaluateApprovalAsync(
        session,
        customer,
        authorisedPrincipal,
        "COMMERCIAL_COMMITMENT",
        5_000m,
        evaluationAt);
    await session.CommitAsync();
}

Ensure(inScopeResult.Allowed, "Principal with permission and in-scope authority was denied.");
Ensure(inScopeResult.Reason == "ALLOWED", "Allowed authority result did not carry an explicit reason.");
Console.WriteLine("PASS: valid permission + effective authority within threshold is allowed.");

Console.WriteLine("SPIKE-004: verifying authority threshold is enforced...");

AuthorityEvaluationResult overThresholdResult;
await using (var session = await sessions.OpenAsync(customer))
{
    overThresholdResult = await authority.EvaluateApprovalAsync(
        session,
        customer,
        authorisedPrincipal,
        "COMMERCIAL_COMMITMENT",
        15_000m,
        evaluationAt);
    await session.CommitAsync();
}

Ensure(!overThresholdResult.Allowed, "Principal was allowed above the configured authority threshold.");
Ensure(
    overThresholdResult.Reason == "BUSINESS_AUTHORITY_MISSING_OR_THRESHOLD_EXCEEDED",
    "Over-threshold denial reason was not explicit.");
Console.WriteLine("PASS: approval above the authority threshold is denied.");

Console.WriteLine("SPIKE-004: verifying effective-dated authority expiry...");

var expiredPrincipal = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
await using (var session = await sessions.OpenAsync(customer))
{
    await authority.GrantApprovePermissionAsync(
        session,
        customer,
        expiredPrincipal,
        new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    await authority.GrantDecisionAuthorityAsync(
        session,
        customer,
        expiredPrincipal,
        "COMMERCIAL_COMMITMENT",
        50_000m,
        new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
        "Expired authority test grant");

    await session.CommitAsync();
}

AuthorityEvaluationResult expiredResult;
await using (var session = await sessions.OpenAsync(customer))
{
    expiredResult = await authority.EvaluateApprovalAsync(
        session,
        customer,
        expiredPrincipal,
        "COMMERCIAL_COMMITMENT",
        1_000m,
        evaluationAt);
    await session.CommitAsync();
}

Ensure(!expiredResult.Allowed, "Expired business authority was incorrectly accepted.");
Console.WriteLine("PASS: expired authority is denied even when technical permission remains active.");

Console.WriteLine();
Console.WriteLine("AUTHORITY SPIKE VERIFICATION PASSED (SPIKE-004).");

return;

static void Ensure(bool condition, string failureMessage)
{
    if (!condition)
    {
        throw new InvalidOperationException($"AUTHORITY SPIKE VERIFICATION FAILED: {failureMessage}");
    }
}
