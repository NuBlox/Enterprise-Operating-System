namespace NuBlox.Kernel.Identity;

/// <summary>
/// Trusted external authentication identity keyed by issuer + subject.
/// </summary>
public sealed record ExternalIdentityBinding
{
    public ExternalIdentityBinding(string issuer, string subject)
    {
        Issuer = RequireValue(issuer, nameof(issuer));
        Subject = RequireValue(subject, nameof(subject));
    }

    public string Issuer { get; }

    public string Subject { get; }

    private static string RequireValue(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}
