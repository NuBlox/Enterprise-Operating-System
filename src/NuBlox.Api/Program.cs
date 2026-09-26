using NuBlox.Api;
using NuBlox.Observability;

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

_ = app.MapNuBloxApiV1();

app.Run();

internal sealed record OperationalHealthContract(string Status);

public partial class Program
{
}
