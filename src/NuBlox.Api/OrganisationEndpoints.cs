using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NuBlox.Audit;
using NuBlox.Enterprise;
using NuBlox.Enterprise.Application;
using NuBlox.Identity;

namespace NuBlox.Api;

public sealed record CreateOrganisationRequest(
    string DisplayName,
    Guid? TenantId = null,
    Guid? ActorPrincipalId = null);

public sealed record OrganisationContract(
    Guid PartyId,
    string PartyKind,
    string DisplayName,
    DateTimeOffset CreatedAtUtc);

public interface IOrganisationHttpOperations
{
    Task<OrganisationContract> CreateAsync(
        AuthenticatedRequestContext context,
        CreateOrganisationRequest request,
        string correlationId,
        CancellationToken cancellationToken);

    Task<OrganisationContract?> FindAsync(
        AuthenticatedRequestContext context,
        PartyId partyId,
        CancellationToken cancellationToken);
}

public sealed class OrganisationHttpOperations : IOrganisationHttpOperations
{
    private readonly OrganisationApplicationService _application;
    private readonly IAuditEvidenceAppender _auditAppender;

    public OrganisationHttpOperations(
        OrganisationApplicationService application,
        IAuditEvidenceAppender auditAppender)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        _auditAppender = auditAppender ?? throw new ArgumentNullException(nameof(auditAppender));
    }

    public async Task<OrganisationContract> CreateAsync(
        AuthenticatedRequestContext context,
        CreateOrganisationRequest request,
        string correlationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(request);

        var record = await _application.CreateAsync(
            new CreateOrganisationCommand(
                context.Tenant.TenantId,
                context.Principal.PrincipalId,
                request.DisplayName),
            cancellationToken).ConfigureAwait(false);

        var organisation = record.Organisation;
        await _auditAppender.AppendAsync(
            new AuditEvidence(
                AuditEventId.New(),
                organisation.TenantId,
                context.Principal.PrincipalId,
                context.Principal.Kind == PrincipalKind.Service
                    ? AuditActorKind.ServicePrincipal
                    : AuditActorKind.HumanPrincipal,
                "enterprise.organisation.create",
                new AuditSubject("organisation", organisation.Id.ToString()),
                DateTimeOffset.UtcNow,
                AuditOutcome.Succeeded,
                "api",
                correlationId: correlationId),
            cancellationToken).ConfigureAwait(false);

        return ToContract(organisation);
    }

    public async Task<OrganisationContract?> FindAsync(
        AuthenticatedRequestContext context,
        PartyId partyId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var record = await _application.FindAsync(
            context.Tenant.TenantId,
            partyId,
            context.Principal.PrincipalId,
            cancellationToken).ConfigureAwait(false);
        return record is null ? null : ToContract(record.Organisation);
    }

    private static OrganisationContract ToContract(Organisation organisation) =>
        new(
            organisation.Id.Value,
            organisation.Kind.ToString(),
            organisation.DisplayName,
            organisation.CreatedAtUtc);
}

public static class OrganisationEndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapOrganisationEndpoints(this RouteGroupBuilder api)
    {
        ArgumentNullException.ThrowIfNull(api);

        var group = api.MapGroup("/{tenantSlug}/organisations").WithTags("Organisations");
        group.MapPost("/", CreateAsync);
        group.MapGet("/{partyId:guid}", FindAsync);
        return group;
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        string tenantSlug,
        CreateOrganisationRequest request,
        IOrganisationHttpOperations operations,
        CancellationToken cancellationToken)
    {
        if (!TryGetVerifiedContext(httpContext, tenantSlug, out var context, out var failure))
        {
            return failure;
        }

        try
        {
            var result = await operations.CreateAsync(
                context!,
                request,
                httpContext.TraceIdentifier,
                cancellationToken).ConfigureAwait(false);
            return Results.Created($"{httpContext.Request.Path}/{result.PartyId:D}", result);
        }
        catch (Exception exception)
        {
            return MapException(exception);
        }
    }

    private static async Task<IResult> FindAsync(
        HttpContext httpContext,
        string tenantSlug,
        Guid partyId,
        IOrganisationHttpOperations operations,
        CancellationToken cancellationToken)
    {
        if (!TryGetVerifiedContext(httpContext, tenantSlug, out var context, out var failure))
        {
            return failure;
        }

        try
        {
            var result = await operations.FindAsync(
                context!,
                new PartyId(partyId),
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
        OrganisationAccessDeniedException => Problem(StatusCodes.Status403Forbidden),
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
