using NuBlox.Kernel;

namespace NuBlox.Authority;

public sealed record AuthorityRequirement
{
    public AuthorityRequirement(string actionCode, string subjectType, string subjectId)
    {
        ActionCode = Require(actionCode, nameof(actionCode), 96);
        SubjectType = Require(subjectType, nameof(subjectType), 96);
        SubjectId = Require(subjectId, nameof(subjectId), 256);
    }

    public string ActionCode { get; }
    public string SubjectType { get; }
    public string SubjectId { get; }

    private static string Require(string value, string parameterName, int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var canonical = value.Trim();
        if (canonical.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Value cannot exceed {maximumLength} characters.");
        }

        return canonical;
    }
}

public sealed record AuthorityEvaluationContext(
    TenantId TenantId,
    PrincipalId PrincipalId,
    AuthorityRequirement Requirement);

public sealed record AuthorityEvaluationResult
{
    private AuthorityEvaluationResult(bool isGranted, string? authorityReference, string? denialReasonCode)
    {
        IsGranted = isGranted;
        AuthorityReference = authorityReference;
        DenialReasonCode = denialReasonCode;
    }

    public bool IsGranted { get; }
    public string? AuthorityReference { get; }
    public string? DenialReasonCode { get; }

    public static AuthorityEvaluationResult Granted(string authorityReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorityReference);
        return new AuthorityEvaluationResult(true, authorityReference.Trim(), null);
    }

    public static AuthorityEvaluationResult Denied(string denialReasonCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(denialReasonCode);
        return new AuthorityEvaluationResult(false, null, denialReasonCode.Trim());
    }
}

public interface IBusinessAuthorityEvaluator
{
    ValueTask<AuthorityEvaluationResult> EvaluateAsync(
        AuthorityEvaluationContext context,
        CancellationToken cancellationToken = default);
}
