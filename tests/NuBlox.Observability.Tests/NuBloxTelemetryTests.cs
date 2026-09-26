using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Kernel;
using NuBlox.Observability;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace NuBlox.Observability.Tests;

[TestClass]
public sealed class NuBloxTelemetryTests
{
    private static readonly string[] MetricTagNames = ["operation.name", "operation.outcome"];
    private static readonly string[] CorrelationScopeNames = ["CorrelationId", "TraceId", "SpanId", "TenantId", "PrincipalId"];

    [TestMethod]
    public void OpenTelemetrySdkReceivesNuBloxActivitiesWithApprovedCorrelation()
    {
        var processor = new CollectingActivityProcessor();
        using var provider = Sdk.CreateTracerProviderBuilder()
            .AddNuBloxInstrumentation()
            .SetSampler(new AlwaysOnSampler())
            .AddProcessor(processor)
            .Build();

        var tenantId = TenantId.New();
        var principalId = PrincipalId.New();
        var correlation = new TelemetryCorrelation("support-123", tenantId, principalId);

        using (NuBloxTelemetry.StartOperation("platform.audit.append", correlation))
        {
        }

        Assert.AreEqual(1, processor.Completed.Count);
        var activity = processor.Completed[0];
        Assert.AreEqual("platform.audit.append", activity.OperationName);
        Assert.AreEqual("support-123", activity.GetTagItem("nublox.correlation.id"));
        Assert.AreEqual(tenantId.ToString(), activity.GetTagItem("nublox.tenant.id"));
        Assert.AreEqual(principalId.ToString(), activity.GetTagItem("nublox.principal.id"));
    }

    [TestMethod]
    public void OperationMetricsUseOnlyBoundedOperationAndOutcomeLabels()
    {
        var measurements = new List<IReadOnlyDictionary<string, object?>>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, currentListener) =>
        {
            if (instrument.Meter.Name == NuBloxTelemetry.MeterName)
            {
                currentListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, _, tags, _) => measurements.Add(CopyTags(tags)));
        listener.SetMeasurementEventCallback<double>((_, _, tags, _) => measurements.Add(CopyTags(tags)));
        listener.Start();

        NuBloxTelemetry.RecordOperation("platform.audit.append", TelemetryOutcome.Succeeded, TimeSpan.FromMilliseconds(12));

        Assert.IsTrue(measurements.Count >= 2);
        foreach (var tags in measurements)
        {
            CollectionAssert.AreEquivalent(MetricTagNames, tags.Keys.ToArray());
            Assert.AreEqual("platform.audit.append", tags["operation.name"]);
            Assert.AreEqual("succeeded", tags["operation.outcome"]);
            Assert.IsFalse(tags.Keys.Any(key =>
                key.Contains("tenant", StringComparison.OrdinalIgnoreCase)
                || key.Contains("principal", StringComparison.OrdinalIgnoreCase)
                || key.Contains("object", StringComparison.OrdinalIgnoreCase)));
        }
    }

    [TestMethod]
    public void LogCorrelationStateContainsOnlyApprovedDiagnosticIdentifiers()
    {
        var state = NuBloxTelemetry.CreateCorrelationScopeState(
            new TelemetryCorrelation("case-42", TenantId.New(), PrincipalId.New()));

        CollectionAssert.AreEquivalent(CorrelationScopeNames, state.Keys.ToArray());
        Assert.IsFalse(state.Keys.Any(key =>
            key.Contains("token", StringComparison.OrdinalIgnoreCase)
            || key.Contains("authorization", StringComparison.OrdinalIgnoreCase)
            || key.Contains("password", StringComparison.OrdinalIgnoreCase)
            || key.Contains("payload", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void ReadinessFailsOnlyWhenProcessOrCriticalDependencyIsUnhealthy()
    {
        var healthy = ServiceHealthSnapshot.Create(
            true,
            [new DependencyHealth("database.primary", true, DependencyHealthState.Healthy)]);
        var degradedNonCritical = ServiceHealthSnapshot.Create(
            true,
            [new DependencyHealth("telemetry.export", false, DependencyHealthState.Unhealthy)]);
        var criticalFailure = ServiceHealthSnapshot.Create(
            true,
            [new DependencyHealth("database.primary", true, DependencyHealthState.Unhealthy)]);
        var deadProcess = ServiceHealthSnapshot.Create(false);

        Assert.IsTrue(healthy.IsLive);
        Assert.IsTrue(healthy.IsReady);
        Assert.IsTrue(degradedNonCritical.IsReady, "A telemetry-export failure must not make required business processing unavailable by definition.");
        Assert.IsFalse(criticalFailure.IsReady);
        Assert.IsFalse(deadProcess.IsLive);
        Assert.IsFalse(deadProcess.IsReady);
    }

    [TestMethod]
    public void ObservabilityAssemblyDoesNotReferenceAuditOrDatabaseProviderAssemblies()
    {
        var references = typeof(NuBloxTelemetry).Assembly.GetReferencedAssemblies();

        Assert.IsFalse(references.Any(reference =>
            reference.Name?.Contains("NuBlox.Audit", StringComparison.OrdinalIgnoreCase) == true
            || reference.Name?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true
            || reference.Name?.Contains("FoundationSpike", StringComparison.OrdinalIgnoreCase) == true));
    }

    [TestMethod]
    public void GovernedOperationCodesRejectFreeFormMetricDimensions()
    {
        AssertThrows<ArgumentException>(() =>
            NuBloxTelemetry.RecordOperation(
                "tenant 123 did something different",
                TelemetryOutcome.Succeeded,
                TimeSpan.Zero));
    }

    private static Dictionary<string, object?> CopyTags(
        ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var tag in tags)
        {
            result[tag.Key] = tag.Value;
        }

        return result;
    }

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

    private sealed class CollectingActivityProcessor : BaseProcessor<Activity>
    {
        public List<Activity> Completed { get; } = [];

        public override void OnEnd(Activity data)
        {
            Completed.Add(data);
        }
    }
}
