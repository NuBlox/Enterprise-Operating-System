using NuBlox.FoundationSpike.Application;
using NuBlox.FoundationSpike.Audit;
using NuBlox.FoundationSpike.Decisions;
using NuBlox.FoundationSpike.Infrastructure;
using NuBlox.FoundationSpike.Shared;
using NuBlox.FoundationSpike.Subjects;
using NuBlox.FoundationSpike.Work;

var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/health", () => Results.Ok(new { status = "ok", spike = true }));

app.MapPost(
    "/spike/governed-work",
    async (
        CreateGovernedWorkRequest request,
        CreateGovernedWorkHandler handler,
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
