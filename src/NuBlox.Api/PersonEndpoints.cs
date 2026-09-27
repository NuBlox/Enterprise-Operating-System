using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NuBlox.Audit;
using NuBlox.Enterprise;
using NuBlox.Enterprise.Application;
using NuBlox.Identity;

namespace NuBlox.Api;

public sealed record CreatePersonRequest(
    string DisplayName,
    Guid? TenantId = null,
    Guid? ActorPrincipalId = null);

public sealed record PersonContract(
    Guid PartyId,
    string PartyKind,
    string DisplayName,
    DateTimeOffset CreatedAtUtc);

public interface IPersonHttpOperations
{
    Task<PersonContract> CreateAsync(
        AuthenticatedRequestContext context,
        CreatePersonRequest request,
        string correlationId,
        CancellationToken cancellationToken);

    Task<PersonContract?> FindAsync(
        AuthenticatedRequestContext context,
        PartyId partyId,
        CancellationToken cancellationToken);
}

public sealed class PersonHttpOperations : IPersonHttpOperations
{
    private readonly PersonApplicationService _application;
    private readonly IAuditEvidenceAppender _auditAppender;

    public PersonHttpOperations(
        PersonApplicationService application,
        IAuditEvidenceAppender auditAppender)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        _auditAppender = auditAppender ?? throw new ArgumentNullException(nameof(auditAppender));
    }

    public async Task<PersonContract> CreateAsync(
        AuthenticatedRequestContext context,
        CreatePersonRequest request,
        string correlationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(request);

        var record = await _application.CreateAsync(
            new CreatePersonCommand(
                context.Tenant.TenantId,
                context.Principal.PrincipalId,
                request.DisplayName),
            cancellationToken).ConfigureAwait(false);

        var person = record.Person;
        await _auditAppender.AppendAsync(
            new AuditEvidence(
                AuditEventId.New(),
                person.TenantId,
                context.Principal.PrincipalId,
                context.Principal.Kind == PrincipalKind.Service
                    ? AuditActorKind.ServicePrincipal
                    : AuditActorKind.HumanPrincipal,
                "enterprise.person.create",
                new AuditSubject("person", person.Id.ToString()),
                DateTimeOffset.UtcNow,
                AuditOutcome.Succeeded,
                "api",
                correlationId: correlationId),
            cancellationToken).ConfigureAwait(false);

        return ToContract(person);
    }

    public async Task<PersonContract?> FindAsync(
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
        return record is null ? null : ToContract(record.Person);
    }

    private static PersonContract ToContract(Person person) =>
        new(
            person.Id.Value,
            person.Kind.ToString(),
            person.DisplayName,
            person.CreatedAtUtc);
}

public static class PersonEndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapPersonEndpoints(this RouteGroupBuilder api)
    {
        ArgumentNullException.ThrowIfNull(api);

        var group = api.MapGroup("/{tenantSlug}/people").WithTags("People");
        group.MapPost("/", CreateAsync);
        group.MapGet("/{partyId:guid}", FindAsync);
        return group;
    }

    private static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        string tenantSlug,
        CreatePersonRequest request,
        IPersonHttpOperations operations,
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
        IPersonHttpOperations operations,
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
        PersonAccessDeniedException => Problem(StatusCodes.Status403Forbidden),
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
