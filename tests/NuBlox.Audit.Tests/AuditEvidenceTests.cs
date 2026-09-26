using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Audit;
using NuBlox.Kernel;

namespace NuBlox.Audit.Tests;

[TestClass]
public sealed class AuditEvidenceTests
{
    [TestMethod]
    public void AppendContractDoesNotExposeMutationOperations()
    {
        var methods = typeof(IAuditEvidenceAppender).GetMethods();

        Assert.AreEqual(1, methods.Length);
        Assert.AreEqual(nameof(IAuditEvidenceAppender.AppendAsync), methods[0].Name);
        Assert.IsFalse(methods.Any(method =>
            method.Name.Contains("Update", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Delete", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void EvidenceModelDoesNotExposeArbitraryPayloadOrSecretFields()
    {
        var forbiddenFragments = new[] { "payload", "password", "secret", "token", "authorization" };
        var properties = typeof(AuditEvidence).GetProperties();

        Assert.IsFalse(properties.Any(property => forbiddenFragments.Any(fragment =>
            property.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase))));
    }

    [TestMethod]
    public void AuditEvidenceRequiresUtcRecordedTime()
    {
        var nonUtc = new DateTimeOffset(2026, 9, 26, 20, 0, 0, TimeSpan.FromHours(1));

        AssertThrows<ArgumentException>(() => CreateEvidence(recordedAtUtc: nonUtc));
    }

    [TestMethod]
    public void CorrectionUsesNewEventAndReferencesPriorEvidence()
    {
        var original = CreateEvidence();
        var correction = CreateEvidence(
            eventId: AuditEventId.New(),
            outcome: AuditOutcome.Corrected,
            correctsEventId: original.EventId);

        Assert.AreNotEqual(original.EventId, correction.EventId);
        Assert.AreEqual(original.EventId, correction.CorrectsEventId);
    }

    [TestMethod]
    public void EventCannotCorrectItself()
    {
        var eventId = AuditEventId.New();

        AssertThrows<ArgumentException>(() => CreateEvidence(eventId: eventId, correctsEventId: eventId));
    }

    [TestMethod]
    public void ServiceAndHumanActorsRemainDistinct()
    {
        var human = CreateEvidence(actorKind: AuditActorKind.HumanPrincipal);
        var service = CreateEvidence(actorKind: AuditActorKind.ServicePrincipal);

        Assert.AreNotEqual(human.ActorKind, service.ActorKind);
    }

    [TestMethod]
    public void GovernedCodesRejectFreeFormValues()
    {
        AssertThrows<ArgumentException>(() =>
        {
            var subject = new AuditSubject("Bad subject type!", "123");
            GC.KeepAlive(subject);
        });

        AssertThrows<ArgumentException>(() => CreateEvidence(actionCode: "Made a change because I felt like it"));
    }

    [TestMethod]
    public void AuditAssemblyDoesNotReferenceTelemetryOrProviderAssemblies()
    {
        var references = typeof(AuditEvidence).Assembly.GetReferencedAssemblies();

        Assert.IsFalse(references.Any(reference =>
            reference.Name?.Contains("OpenTelemetry", StringComparison.OrdinalIgnoreCase) == true
            || reference.Name?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true
            || reference.Name?.Contains("FoundationSpike", StringComparison.OrdinalIgnoreCase) == true));
    }

    private static AuditEvidence CreateEvidence(
        AuditEventId? eventId = null,
        DateTimeOffset? recordedAtUtc = null,
        AuditOutcome outcome = AuditOutcome.Succeeded,
        AuditActorKind actorKind = AuditActorKind.HumanPrincipal,
        string actionCode = "record.created",
        AuditEventId? correctsEventId = null) =>
        new(
            eventId ?? AuditEventId.New(),
            TenantId.New(),
            PrincipalId.New(),
            actorKind,
            actionCode,
            new AuditSubject("platform.record", Guid.NewGuid().ToString("D")),
            recordedAtUtc ?? DateTimeOffset.UtcNow,
            outcome,
            "nublox.platform",
            correlationId: Guid.NewGuid().ToString("N"),
            traceId: "0123456789abcdef0123456789abcdef",
            evidenceReferences: [new AuditEvidenceReference("document.version", "doc-001:v3")],
            correctsEventId: correctsEventId);

    private static TException AssertThrows<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        Assert.Fail($"Expected {typeof(TException).Name}.");
        throw new InvalidOperationException("Assert.Fail did not throw as expected.");
    }
}
