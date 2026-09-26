using System.Text.RegularExpressions;

namespace NuBlox.Observability;

public enum DependencyHealthState
{
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3
}

public sealed record DependencyHealth
{
    public DependencyHealth(string dependencyCode, bool isCritical, DependencyHealthState state)
    {
        DependencyCode = TelemetryCode.Require(dependencyCode, nameof(dependencyCode));
        IsCritical = isCritical;
        State = state;
    }

    public string DependencyCode { get; }

    public bool IsCritical { get; }

    public DependencyHealthState State { get; }
}

/// <summary>
/// Host-neutral health result. DEV-108 maps these primitives to operational liveness/readiness endpoints;
/// they intentionally contain no secret, credential or topology detail.
/// </summary>
public sealed class ServiceHealthSnapshot
{
    private ServiceHealthSnapshot(
        bool isLive,
        bool isReady,
        IReadOnlyList<DependencyHealth> dependencies)
    {
        IsLive = isLive;
        IsReady = isReady;
        Dependencies = dependencies;
    }

    public bool IsLive { get; }

    public bool IsReady { get; }

    public IReadOnlyList<DependencyHealth> Dependencies { get; }

    public static ServiceHealthSnapshot Create(
        bool processIsLive,
        IReadOnlyCollection<DependencyHealth>? dependencies = null)
    {
        var snapshot = dependencies is null ? Array.Empty<DependencyHealth>() : [.. dependencies];
        var ready = processIsLive && snapshot.All(dependency =>
            !dependency.IsCritical || dependency.State != DependencyHealthState.Unhealthy);

        return new ServiceHealthSnapshot(processIsLive, ready, snapshot);
    }
}

internal static partial class TelemetryCode
{
    [GeneratedRegex("^[a-z][a-z0-9_.-]{1,79}$", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();

    public static string Require(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A governed telemetry code is required.", parameterName);
        }

        var normalised = value.Trim().ToLowerInvariant();
        if (!Pattern().IsMatch(normalised))
        {
            throw new ArgumentException("The value is not a valid governed telemetry code.", parameterName);
        }

        return normalised;
    }
}
