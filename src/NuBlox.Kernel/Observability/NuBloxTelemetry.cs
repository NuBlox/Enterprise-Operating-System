using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace NuBlox.Kernel.Observability;

public static class NuBloxTelemetry
{
    public const string ActivitySourceName = "NuBlox";
    public const string MeterName = "NuBlox";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly Meter Meter = new(MeterName);
}
