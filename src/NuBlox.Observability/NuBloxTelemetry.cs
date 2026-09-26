using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using NuBlox.Kernel;

namespace NuBlox.Observability;

public enum TelemetryOutcome
{
    Succeeded = 1,
    Failed = 2,
    Cancelled = 3
}

public sealed record TelemetryCorrelation
{
    public TelemetryCorrelation(
        string correlationId,
        TenantId? tenantId = null,
        PrincipalId? principalId = null)
    {
        if (string.IsNullOrWhiteSpace(correlationId) || correlationId.Trim().Length > 128)
        {
            throw new ArgumentException("Correlation identifier must contain between 1 and 128 characters.", nameof(correlationId));
        }

        CorrelationId = correlationId.Trim();
        TenantId = tenantId;
        PrincipalId = principalId;
    }

    public string CorrelationId { get; }

    public TenantId? TenantId { get; }

    public PrincipalId? PrincipalId { get; }
}

/// <summary>
/// Standard NuBlox instrumentation primitives. Business identifiers are attached only to trace/log correlation;
/// metrics intentionally use bounded operation/outcome labels and never tenant, principal or object identifiers.
/// </summary>
public static class NuBloxTelemetry
{
    public const string ActivitySourceName = "NuBlox.EnterpriseOperatingSystem";
    public const string MeterName = "NuBlox.EnterpriseOperatingSystem";
    public const string InstrumentationVersion = "0.1.0";

    private static readonly ActivitySource ActivitySourceInstance = new(ActivitySourceName, InstrumentationVersion);
    private static readonly Meter MeterInstance = new(MeterName, InstrumentationVersion);
    private static readonly Counter<long> OperationCount = MeterInstance.CreateCounter<long>(
        "nublox.operation.count",
        unit: "{operation}",
        description: "Count of governed NuBlox platform operations by bounded operation code and outcome.");
    private static readonly Histogram<double> OperationDuration = MeterInstance.CreateHistogram<double>(
        "nublox.operation.duration",
        unit: "ms",
        description: "Duration of governed NuBlox platform operations.");

    public static Activity? StartOperation(
        string operationCode,
        TelemetryCorrelation? correlation = null,
        ActivityKind kind = ActivityKind.Internal)
    {
        var code = TelemetryCode.Require(operationCode, nameof(operationCode));
        var activity = ActivitySourceInstance.StartActivity(code, kind);
        if (activity is not null && correlation is not null)
        {
            activity.SetTag("nublox.correlation.id", correlation.CorrelationId);
            if (correlation.TenantId is { } tenantId)
            {
                activity.SetTag("nublox.tenant.id", tenantId.ToString());
            }

            if (correlation.PrincipalId is { } principalId)
            {
                activity.SetTag("nublox.principal.id", principalId.ToString());
            }
        }

        return activity;
    }

    public static void RecordOperation(
        string operationCode,
        TelemetryOutcome outcome,
        TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        var code = TelemetryCode.Require(operationCode, nameof(operationCode));
        var outcomeCode = outcome.ToString().ToLowerInvariant();
        var tags = new TagList
        {
            { "operation.name", code },
            { "operation.outcome", outcomeCode }
        };

        OperationCount.Add(1, tags);
        OperationDuration.Record(duration.TotalMilliseconds, tags);
    }

    public static IReadOnlyDictionary<string, object?> CreateCorrelationScopeState(
        TelemetryCorrelation correlation)
    {
        ArgumentNullException.ThrowIfNull(correlation);

        var activity = Activity.Current;
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["CorrelationId"] = correlation.CorrelationId,
            ["TraceId"] = activity?.TraceId.ToString(),
            ["SpanId"] = activity?.SpanId.ToString(),
            ["TenantId"] = correlation.TenantId?.ToString(),
            ["PrincipalId"] = correlation.PrincipalId?.ToString()
        };
    }

    public static IDisposable? BeginCorrelationScope(
        ILogger logger,
        TelemetryCorrelation correlation)
    {
        ArgumentNullException.ThrowIfNull(logger);
        return logger.BeginScope(CreateCorrelationScopeState(correlation));
    }
}
