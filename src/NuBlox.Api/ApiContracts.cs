using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NuBlox.Identity;

namespace NuBlox.Api;

public static class NuBloxApiRoutes
{
    public const string V1Prefix = "/api/v1";
    public const string OpenApiV1 = "/openapi/v1.json";
    public const string Liveness = "/health/live";
    public const string Readiness = "/health/ready";
}

public static class NuBloxProblemTypes
{
    public const string Validation = "https://nublox.com/problems/validation";
    public const string Unauthenticated = "https://nublox.com/problems/unauthenticated";
    public const string Forbidden = "https://nublox.com/problems/forbidden";
    public const string NotFound = "https://nublox.com/problems/not-found";
    public const string Conflict = "https://nublox.com/problems/conflict";
    public const string RateLimitExceeded = "https://nublox.com/problems/rate-limit-exceeded";
    public const string DependencyUnavailable = "https://nublox.com/problems/dependency-unavailable";
    public const string UnexpectedFailure = "https://nublox.com/problems/unexpected-failure";

    public static void Apply(ProblemDetails problemDetails, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(problemDetails);

        (problemDetails.Type, problemDetails.Title) = statusCode switch
        {
            StatusCodes.Status400BadRequest => (Validation, "Request validation failed"),
            StatusCodes.Status401Unauthorized => (Unauthenticated, "Authentication is required"),
            StatusCodes.Status403Forbidden => (Forbidden, "Access is forbidden"),
            StatusCodes.Status404NotFound => (NotFound, "Resource not found"),
            StatusCodes.Status409Conflict => (Conflict, "Request conflicts with current state"),
            StatusCodes.Status429TooManyRequests => (RateLimitExceeded, "Request limit exceeded"),
            StatusCodes.Status503ServiceUnavailable => (DependencyUnavailable, "Required dependency unavailable"),
            _ when statusCode >= 500 => (UnexpectedFailure, "Unexpected server failure"),
            _ => (Validation, "Request could not be processed")
        };

        problemDetails.Status = statusCode;
        problemDetails.Detail = null;
        problemDetails.Instance = null;
    }
}

public static class NuBloxApiRouteBuilderExtensions
{
    public static RouteGroupBuilder MapNuBloxApiV1(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints
            .MapGroup(NuBloxApiRoutes.V1Prefix)
            .WithTags("NuBlox API v1");
    }
}

/// <summary>
/// Carries a context that has already been authenticated and tenant-authorised by the identity boundary.
/// Route values, headers and request bodies never populate this item directly.
/// </summary>
public static class NuBloxHttpContextExtensions
{
    private static readonly object VerifiedContextKey = new();

    public static void SetVerifiedNuBloxContext(
        this HttpContext httpContext,
        AuthenticatedRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(context);

        httpContext.Items[VerifiedContextKey] = context;
    }

    public static bool TryGetVerifiedNuBloxContext(
        this HttpContext httpContext,
        out AuthenticatedRequestContext? context)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        context = httpContext.Items.TryGetValue(VerifiedContextKey, out var value)
            ? value as AuthenticatedRequestContext
            : null;

        return context is not null;
    }
}
