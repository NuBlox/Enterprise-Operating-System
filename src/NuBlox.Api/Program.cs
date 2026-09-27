using NuBlox.Api;
using NuBlox.Audit;
using NuBlox.Kernel;
using NuBlox.Observability;
using NuBlox.Runtime.PostgreSql;
using NuBlox.WorkProducts.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        var statusCode = context.ProblemDetails.Status ?? context.HttpContext.Response.StatusCode;
        NuBloxProblemTypes.Apply(context.ProblemDetails, statusCode);
        context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddOpenApi("v1");

if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("PostgreSql")
        ?? builder.Configuration["NuBlox:PostgreSql:ConnectionString"]
        ?? throw new InvalidOperationException("NuBlox PostgreSQL runtime connection string is required outside the Testing environment.");

    var approvalPrincipals = builder.Configuration
        .GetSection("NuBlox:WorkProducts:ApprovalPrincipals")
        .GetChildren()
        .Select(static child => child.Value)
        .Where(static value => Guid.TryParse(value, out _))
        .Select(static value => new PrincipalId(Guid.Parse(value!)))
        .ToArray();

    var organisationAdministrators = builder.Configuration
        .GetSection("NuBlox:Enterprise:OrganisationAdministratorPrincipals")
        .GetChildren()
        .Select(static child => child.Value)
        .Where(static value => Guid.TryParse(value, out _))
        .Select(static value => new PrincipalId(Guid.Parse(value!)))
        .ToArray();

    var personAdministrators = builder.Configuration
        .GetSection("NuBlox:Enterprise:PersonAdministratorPrincipals")
        .GetChildren()
        .Select(static child => child.Value)
        .Where(static value => Guid.TryParse(value, out _))
        .Select(static value => new PrincipalId(Guid.Parse(value!)))
        .ToArray();

    builder.Services.AddNuBloxPostgresRuntime(
        connectionString,
        approvalPrincipals,
        organisationAdministrators,
        personAdministrators);
}

builder.Services.AddScoped<OrganisationHttpOperations>();
builder.Services.AddScoped<IOrganisationHttpOperations>(static services =>
    services.GetRequiredService<OrganisationHttpOperations>());

builder.Services.AddScoped<PersonHttpOperations>();
builder.Services.AddScoped<IPersonHttpOperations>(static services =>
    services.GetRequiredService<PersonHttpOperations>());

builder.Services.AddScoped<WorkProductHttpOperations>();
builder.Services.AddScoped<IWorkProductHttpOperations>(services =>
    new AuditedObservedWorkProductHttpOperations(
        services.GetRequiredService<WorkProductHttpOperations>(),
        services.GetRequiredService<IAuditEvidenceAppender>(),
        services.GetRequiredService<ILogger<AuditedObservedWorkProductHttpOperations>>()));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi("/openapi/{documentName}.json");

app.MapGet(NuBloxApiRoutes.Liveness, () => Results.Ok(new OperationalHealthContract("live")))
    .ExcludeFromDescription();

app.MapGet(NuBloxApiRoutes.Readiness, () =>
    {
        var snapshot = ServiceHealthSnapshot.Create(processIsLive: true);
        return snapshot.IsReady
            ? Results.Ok(new OperationalHealthContract("ready"))
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    })
    .ExcludeFromDescription();

var api = app.MapNuBloxApiV1();
_ = api.MapOrganisationEndpoints();
_ = api.MapPersonEndpoints();
_ = api.MapWorkProductEndpoints();

app.Run();

internal sealed record OperationalHealthContract(string Status);

public partial class Program
{
}
