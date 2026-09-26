using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NuBlox.Identity;
using NuBlox.Kernel;
using NuBlox.WorkProducts;
using NuBlox.WorkProducts.Application;

namespace NuBlox.Api;

public sealed record CreateWorkProductRequest(
    string Title,
    string ProductType,
    Guid? OwnerPrincipalId = null,
    Guid? TenantId = null,
    Guid? ActorPrincipalId = null);

public sealed record SubmitWorkProductRequest(
    Guid ReviewerPrincipalId,
    Guid? TenantId = null,
    Guid? ActorPrincipalId = null);

public sealed record DecideWorkProductRequest(
    Guid ReviewRequestId,
    ReviewDecisionOutcome Outcome,
    string Rationale,
    Guid? TenantId = null,
    Guid? ActorPrincipalId = null);

public sealed record WorkProductContract(
    Guid WorkProductId,
    string Title,
    string ProductType,
    Guid OwnerPrincipalId,
    int RevisionNumber,
    string RevisionState);

public sealed record ReviewSubmissionContract(
    Guid WorkProductId,
    Guid RevisionId,
    int RevisionNumber,
    string RevisionState,
    Guid ReviewRequestId,
    Guid ReviewerPrincipalId);

public sealed record ReviewDecisionContract(
    Guid WorkProductId,
    Guid RevisionId,
    string RevisionState,
    Guid DecisionEvidenceId,
    string Outcome);

public sealed record IssueWorkProductContract(
    Guid WorkProductId,
    Guid RevisionId,
    string RevisionState,
    Guid IssueEvidenceId);

public interface IWorkProductHttpOperations
{
    Task<WorkProductContract> CreateAsync(
        AuthenticatedRequestContext context,
        CreateWorkProductRequest request,
        CancellationToken cancellationToken);

    Task<WorkProductContract?> FindAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        CancellationToken cancellationToken);

    Task<ReviewSubmissionContract> SubmitAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        SubmitWorkProductRequest request,
        CancellationToken cancellationToken);

    Task<ReviewDecisionContract> DecideAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        DecideWorkProductRequest request,
        string correlationId,
        CancellationToken cancellationToken);

    Task<IssueWorkProductContract> IssueAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        string correlationId,
        CancellationToken cancellationToken);
}

public sealed class WorkProductHttpOperations : IWorkProductHttpOperations
{
    private readonly WorkProductApplicationService _application;
    private readonly WorkProductIssueService _issueService;

    public WorkProductHttpOperations(
        WorkProductApplicationService application,
        WorkProductIssueService issueService)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        _issueService = issueService ?? throw new ArgumentNullException(nameof(issueService));
    }

    public async Task<WorkProductContract> CreateAsync(
        AuthenticatedRequestContext context,
        CreateWorkProductRequest request,
        CancellationToken cancellationToken)
    {
        var owner = request.OwnerPrincipalId is { } ownerId
            ? new PrincipalId(ownerId)
            : context.Principal.PrincipalId;

        var record = await _application.CreateAsync(
            new CreateWorkProductCommand(
                context.Tenant.TenantId,
                context.Principal.PrincipalId,
                request.Title,
                request.ProductType,
                owner),
            cancellationToken).ConfigureAwait(false);

        return ToContract(record);
    }

    public async Task<WorkProductContract?> FindAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        CancellationToken cancellationToken)
    {
        var record = await _application.FindAsync(
            context.Tenant.TenantId,
            workProductId,
            cancellationToken).ConfigureAwait(false);
        return record is null ? null : ToContract(record);
    }

    public async Task<ReviewSubmissionContract> SubmitAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        SubmitWorkProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _application.SubmitForReviewAsync(
            new SubmitWorkProductForReviewCommand(
                context.Tenant.TenantId,
                workProductId,
                context.Principal.PrincipalId,
                new PrincipalId(request.ReviewerPrincipalId)),
            cancellationToken).ConfigureAwait(false);

        return new ReviewSubmissionContract(
            result.Revision.WorkProductId.Value,
            result.Revision.Id.Value,
            result.Revision.RevisionNumber,
            result.Revision.State.ToString(),
            result.ReviewRequest.Id.Value,
            result.ReviewRequest.RequestedPrincipalId.Value);
    }

    public async Task<ReviewDecisionContract> DecideAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        DecideWorkProductRequest request,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var result = await _application.DecideReviewAsync(
            new DecideWorkProductReviewCommand(
                context.Tenant.TenantId,
                workProductId,
                new ReviewRequestId(request.ReviewRequestId),
                context.Principal.PrincipalId,
                request.Outcome,
                request.Rationale,
                correlationId),
            cancellationToken).ConfigureAwait(false);

        return new ReviewDecisionContract(
            result.Revision.WorkProductId.Value,
            result.Revision.Id.Value,
            result.Revision.State.ToString(),
            result.Decision.Id.Value,
            result.Decision.Outcome.ToString());
    }

    public async Task<IssueWorkProductContract> IssueAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var result = await _issueService.IssueAsync(
            new IssueWorkProductRevisionCommand(
                context.Tenant.TenantId,
                workProductId,
                context.Principal.PrincipalId,
                correlationId),
            cancellationToken).ConfigureAwait(false);

        return new IssueWorkProductContract(
            result.Revision.WorkProductId.Value,
            result.Revision.Id.Value,
            result.Revision.State.ToString(),
            result.Evidence.Id.Value);
    }

    private static WorkProductContract ToContract(WorkProductRecord record) =>
        new(
            record.WorkProduct.Id.Value,
            record.WorkProduct.Title,
            record.WorkProduct.ProductType,
            record.WorkProduct.OwnerPrincipalId.Value,
            record.CurrentRevision.RevisionNumber,
            record.CurrentRevision.State.ToString());
}

public static class WorkProductEndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapWorkProductEndpoints(this RouteGroupBuilder api)
    {
        ArgumentNullException.ThrowIfNull(api);

        var group = api.MapGroup("/{tenantSlug}/work-products").WithTags("Work Products");

        group.MapPost("/", CreateAsync);
        group.MapGet("/{workProductId:guid}", FindAsync);
        group.MapPost("/{workProductId:guid}/submit", SubmitAsync);
        group.MapPost("/{workProductId:guid}/decisions", DecideAsync);
        group.MapPost("/{workProductId:guid}/issue", IssueAsync);

        return group;
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        string tenantSlug,
        CreateWorkProductRequest request,
        IWorkProductHttpOperations operations,
        CancellationToken cancellationToken)
    {
        if (!TryGetVerifiedContext(httpContext, tenantSlug, out var context, out var failure)) return failure;

        try
        {
            var result = await operations.CreateAsync(context!, request, cancellationToken).ConfigureAwait(false);
            return Results.Created($"{httpContext.Request.Path}/{result.WorkProductId:D}", result);
        }
        catch (Exception exception)
        {
            return MapException(exception);
        }
    }

    private static async Task<IResult> FindAsync(
        HttpContext httpContext,
        string tenantSlug,
        Guid workProductId,
        IWorkProductHttpOperations operations,
        CancellationToken cancellationToken)
    {
        if (!TryGetVerifiedContext(httpContext, tenantSlug, out var context, out var failure)) return failure;

        try
        {
            var result = await operations.FindAsync(
                context!,
                new WorkProductId(workProductId),
                cancellationToken).ConfigureAwait(false);
            return result is null
                ? Problem(StatusCodes.Status404NotFound)
                : Results.Ok(result);
        }
        catch (Exception exception)
        {
            return MapException(exception);
        }
    }

    private static async Task<IResult> SubmitAsync(
        HttpContext httpContext,
        string tenantSlug,
        Guid workProductId,
        SubmitWorkProductRequest request,
        IWorkProductHttpOperations operations,
        CancellationToken cancellationToken)
    {
        if (!TryGetVerifiedContext(httpContext, tenantSlug, out var context, out var failure)) return failure;

        try
        {
            return Results.Ok(await operations.SubmitAsync(
                context!,
                new WorkProductId(workProductId),
                request,
                cancellationToken).ConfigureAwait(false));
        }
        catch (Exception exception)
        {
            return MapException(exception);
        }
    }

    private static async Task<IResult> DecideAsync(
        HttpContext httpContext,
        string tenantSlug,
        Guid workProductId,
        DecideWorkProductRequest request,
        IWorkProductHttpOperations operations,
        CancellationToken cancellationToken)
    {
        if (!TryGetVerifiedContext(httpContext, tenantSlug, out var context, out var failure)) return failure;

        try
        {
            return Results.Ok(await operations.DecideAsync(
                context!,
                new WorkProductId(workProductId),
                request,
                httpContext.TraceIdentifier,
                cancellationToken).ConfigureAwait(false));
        }
        catch (Exception exception)
        {
            return MapException(exception);
        }
    }

    private static async Task<IResult> IssueAsync(
        HttpContext httpContext,
        string tenantSlug,
        Guid workProductId,
        IWorkProductHttpOperations operations,
        CancellationToken cancellationToken)
    {
        if (!TryGetVerifiedContext(httpContext, tenantSlug, out var context, out var failure)) return failure;

        try
        {
            return Results.Ok(await operations.IssueAsync(
                context!,
                new WorkProductId(workProductId),
                httpContext.TraceIdentifier,
                cancellationToken).ConfigureAwait(false));
        }
        catch (Exception exception)
        {
            return MapException(exception);
        }
    }

    private static bool TryGetVerifiedContext(
        HttpContext httpContext,
        string tenantSlug,
        out AuthenticatedRequestContext? context,
        out IResult failure)
    {
        if (!httpContext.TryGetVerifiedNuBloxContext(out context) || context is null)
        {
            failure = Problem(StatusCodes.Status401Unauthorized);
            return false;
        }

        if (!string.Equals(
            context.Tenant.CanonicalSlug.Value,
            tenantSlug,
            StringComparison.OrdinalIgnoreCase))
        {
            context = null;
            failure = Problem(StatusCodes.Status403Forbidden);
            return false;
        }

        failure = Results.Empty;
        return true;
    }

    private static IResult MapException(Exception exception) => exception switch
    {
        ArgumentException => Problem(StatusCodes.Status400BadRequest),
        KeyNotFoundException => Problem(StatusCodes.Status404NotFound),
        WorkProductAccessDeniedException => Problem(StatusCodes.Status403Forbidden),
        WorkProductDecisionAccessDeniedException => Problem(StatusCodes.Status403Forbidden),
        WorkProductAuthorityDeniedException => Problem(StatusCodes.Status403Forbidden),
        WorkProductIssueAccessDeniedException => Problem(StatusCodes.Status403Forbidden),
        WorkProductStateConflictException => Problem(StatusCodes.Status409Conflict),
        _ => Problem(StatusCodes.Status500InternalServerError)
    };

    private static IResult Problem(int statusCode)
    {
        var problem = new ProblemDetails();
        NuBloxProblemTypes.Apply(problem, statusCode);
        return Results.Problem(
            statusCode: problem.Status,
            title: problem.Title,
            type: problem.Type);
    }
}
