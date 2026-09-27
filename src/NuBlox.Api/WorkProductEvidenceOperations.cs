using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NuBlox.Audit;
using NuBlox.Identity;
using NuBlox.Observability;
using NuBlox.WorkProducts;
using NuBlox.WorkProducts.Application;

namespace NuBlox.Api;

public sealed class AuditedObservedWorkProductHttpOperations : IWorkProductHttpOperations
{
    private const string Source = "nublox.work_products.http";

    private readonly IWorkProductHttpOperations _inner;
    private readonly IAuditEvidenceAppender _audit;
    private readonly ILogger<AuditedObservedWorkProductHttpOperations> _logger;

    public AuditedObservedWorkProductHttpOperations(
        IWorkProductHttpOperations inner,
        IAuditEvidenceAppender audit,
        ILogger<AuditedObservedWorkProductHttpOperations> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<WorkProductContract> CreateAsync(
        AuthenticatedRequestContext context,
        CreateWorkProductRequest request,
        CancellationToken cancellationToken) =>
        ExecuteMaterialAsync(
            context,
            "work_product.create",
            async () => await _inner.CreateAsync(context, request, cancellationToken).ConfigureAwait(false),
            result => result.WorkProductId,
            cancellationToken);

    public Task<WorkProductContract?> FindAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        CancellationToken cancellationToken) =>
        ExecuteReadAsync(
            context,
            "work_product.read",
            () => _inner.FindAsync(context, workProductId, cancellationToken));

    public Task<ReviewSubmissionContract> SubmitAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        SubmitWorkProductRequest request,
        CancellationToken cancellationToken) =>
        ExecuteMaterialAsync(
            context,
            "work_product.review.submit",
            async () => await _inner.SubmitAsync(context, workProductId, request, cancellationToken).ConfigureAwait(false),
            result => result.WorkProductId,
            cancellationToken,
            result => [new AuditEvidenceReference("review.request", result.ReviewRequestId.ToString("D"))]);

    public Task<ReviewDecisionContract> DecideAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        DecideWorkProductRequest request,
        string correlationId,
        CancellationToken cancellationToken) =>
        ExecuteMaterialAsync(
            context,
            "work_product.review.decide",
            async () => await _inner.DecideAsync(context, workProductId, request, correlationId, cancellationToken).ConfigureAwait(false),
            result => result.WorkProductId,
            cancellationToken,
            result => [new AuditEvidenceReference("decision.evidence", result.DecisionEvidenceId.ToString("D"))],
            correlationId);

    public Task<IssueWorkProductContract> IssueAsync(
        AuthenticatedRequestContext context,
        WorkProductId workProductId,
        string correlationId,
        CancellationToken cancellationToken) =>
        ExecuteMaterialAsync(
            context,
            "work_product.issue",
            async () => await _inner.IssueAsync(context, workProductId, correlationId, cancellationToken).ConfigureAwait(false),
            result => result.WorkProductId,
            cancellationToken,
            result => [new AuditEvidenceReference("issue.evidence", result.IssueEvidenceId.ToString("D"))],
            correlationId);

    private async Task<TResult> ExecuteMaterialAsync<TResult>(
        AuthenticatedRequestContext context,
        string operationCode,
        Func<Task<TResult>> action,
        Func<TResult, Guid> subjectId,
        CancellationToken cancellationToken,
        Func<TResult, IReadOnlyCollection<AuditEvidenceReference>>? evidenceReferences = null,
        string? correlationId = null)
    {
        var correlation = CreateCorrelation(context, correlationId);
        using var scope = NuBloxTelemetry.BeginCorrelationScope(_logger, correlation);
        using var activity = NuBloxTelemetry.StartOperation(operationCode, correlation);
        var started = Stopwatch.GetTimestamp();

        try
        {
            var result = await action().ConfigureAwait(false);
            var traceId = activity?.TraceId.ToString();
            var evidence = new AuditEvidence(
                AuditEventId.New(),
                context.Tenant.TenantId,
                context.Principal.PrincipalId,
                context.Principal.Kind == PrincipalKind.Human
                    ? AuditActorKind.HumanPrincipal
                    : AuditActorKind.ServicePrincipal,
                operationCode,
                new AuditSubject("work_product", subjectId(result).ToString("D")),
                DateTimeOffset.UtcNow,
                AuditOutcome.Succeeded,
                Source,
                correlation.CorrelationId,
                traceId,
                evidenceReferences: evidenceReferences?.Invoke(result));

            await _audit.AppendAsync(evidence, cancellationToken).ConfigureAwait(false);
            NuBloxTelemetry.RecordOperation(operationCode, TelemetryOutcome.Succeeded, Stopwatch.GetElapsedTime(started));
            return result;
        }
        catch (OperationCanceledException)
        {
            NuBloxTelemetry.RecordOperation(operationCode, TelemetryOutcome.Cancelled, Stopwatch.GetElapsedTime(started));
            throw;
        }
        catch
        {
            NuBloxTelemetry.RecordOperation(operationCode, TelemetryOutcome.Failed, Stopwatch.GetElapsedTime(started));
            throw;
        }
    }

    private async Task<TResult> ExecuteReadAsync<TResult>(
        AuthenticatedRequestContext context,
        string operationCode,
        Func<Task<TResult>> action)
    {
        var correlation = CreateCorrelation(context, null);
        using var scope = NuBloxTelemetry.BeginCorrelationScope(_logger, correlation);
        using var activity = NuBloxTelemetry.StartOperation(operationCode, correlation);
        var started = Stopwatch.GetTimestamp();

        try
        {
            var result = await action().ConfigureAwait(false);
            NuBloxTelemetry.RecordOperation(operationCode, TelemetryOutcome.Succeeded, Stopwatch.GetElapsedTime(started));
            return result;
        }
        catch (OperationCanceledException)
        {
            NuBloxTelemetry.RecordOperation(operationCode, TelemetryOutcome.Cancelled, Stopwatch.GetElapsedTime(started));
            throw;
        }
        catch
        {
            NuBloxTelemetry.RecordOperation(operationCode, TelemetryOutcome.Failed, Stopwatch.GetElapsedTime(started));
            throw;
        }
    }

    private static TelemetryCorrelation CreateCorrelation(
        AuthenticatedRequestContext context,
        string? correlationId)
    {
        var resolvedCorrelationId = !string.IsNullOrWhiteSpace(correlationId)
            ? correlationId
            : Activity.Current?.TraceId.ToString();

        if (string.IsNullOrWhiteSpace(resolvedCorrelationId))
        {
            resolvedCorrelationId = Guid.NewGuid().ToString("N");
        }

        return new TelemetryCorrelation(
            resolvedCorrelationId,
            context.Tenant.TenantId,
            context.Principal.PrincipalId);
    }
}
