using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace NuBlox.Observability;

/// <summary>
/// Registers NuBlox standard diagnostics primitives with the OpenTelemetry SDK. Exporter/backend selection
/// remains a host/deployment concern; product instrumentation does not depend on a telemetry vendor.
/// </summary>
public static class OpenTelemetryRegistration
{
    public static TracerProviderBuilder AddNuBloxInstrumentation(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddSource(NuBloxTelemetry.ActivitySourceName);
    }

    public static MeterProviderBuilder AddNuBloxInstrumentation(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddMeter(NuBloxTelemetry.MeterName);
    }
}
