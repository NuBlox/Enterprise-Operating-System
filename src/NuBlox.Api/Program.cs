using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NuBlox.Kernel;
using NuBlox.Kernel.Observability;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi("v1");
builder.Services.AddHealthChecks();

OpenTelemetry.OpenTelemetryBuilder telemetry = builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(ProductIdentity.ProductCode))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource(NuBloxTelemetry.ActivitySourceName))
    .WithMetrics(metrics => metrics
        .AddMeter(NuBloxTelemetry.MeterName));

if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
{
    telemetry.UseOtlpExporter();
}

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi("/openapi/{documentName}.json");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready");

RouteGroupBuilder api = app.MapGroup("/api/v1");

api.MapGet("/meta", () => Results.Ok(new
{
    product = ProductIdentity.Name,
    productCode = ProductIdentity.ProductCode,
    apiVersion = "v1"
}));

app.Run();

public partial class Program;
