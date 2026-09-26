using NuBlox.FoundationSpike.Application;
using NuBlox.FoundationSpike.Audit;
using NuBlox.FoundationSpike.Decisions;
using NuBlox.FoundationSpike.Infrastructure;
using NuBlox.FoundationSpike.Shared;
using NuBlox.FoundationSpike.Subjects;
using NuBlox.FoundationSpike.Work;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Database is required for the foundation architecture spike.");

builder.Services.AddSingleton<ICustomerScopedTransactionalSessionFactory>(
    _ => new PostgresCustomerScopedTransactionalSessionFactory(connectionString));
builder.Services.AddSingleton<SubjectModule>();
builder.Services.AddSingleton<WorkModule>();
builder.Services.AddSingleton<DecisionModule>();
builder.Services.AddSingleton<AuditModule>();
builder.Services.AddSingleton<CreateGovernedWorkHandler>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var supplied = context.Request.Headers["X-Correlation-ID"].ToString();
    var correlationId = Guid.TryParse(supplied, out var parsed) && parsed != Guid.Empty
        ? parsed
        : Guid.NewGuid();

    context.Response.Headers["X-Correlation-ID"] = correlationId.ToString("D");
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("NuBlox.FoundationSpike.Request");
    using (logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId.ToString("D")
    }))
    {
        await next(context);
    }
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", spike = true }));

app.MapPost(
    "/spike/governed-work",
    async (
        CreateGovernedWorkRequest request,
        CreateGovernedWorkHandler handler,
        ILogger<Program> logger,
        CancellationToken cancellationToken) =>
    {
        if (request.CustomerId == Guid.Empty)
        {
            return Results.BadRequest(new { error = "customerId must be a non-empty UUID." });
        }

        try
        {
            var result = await handler.HandleAsync(
                new CreateGovernedWorkCommand(
                    new CustomerId(request.CustomerId),
                    request.SubjectName,
                    request.WorkSummary,
                    request.DecisionOutcome),
                cancellationToken);

            logger.LogInformation(
                "Governed work created for customer {CustomerId}, work {WorkId}",
                request.CustomerId,
                result.WorkId.Value);

            return Results.Created(
                $"/spike/governed-work/{result.WorkId.Value}",
                new
                {
                    customerId = request.CustomerId,
                    subjectId = result.SubjectId.Value,
                    workId = result.WorkId.Value,
                    decisionId = result.DecisionId.Value,
                    auditEventId = result.AuditEventId.Value
                });
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    });

app.Run("http://localhost:5080");

public sealed record CreateGovernedWorkRequest(
    Guid CustomerId,
    string SubjectName,
    string WorkSummary,
    string DecisionOutcome);
