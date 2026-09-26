using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Audit;
using NuBlox.Identity;
using NuBlox.Kernel;
using NuBlox.Observability;
using NuBlox.WorkProducts;

namespace NuBlox.Api.Tests;

[TestClass]
public sealed class WorkProductEvidenceOperationsTests
{
    [TestMethod]
    public async Task MaterialOperationsAppendMinimisedAuditEvidenceAndCorrelatedTelemetry()
    {
        var context = await CreateVerifiedContextAsync();
        var inner = new RecordingOperations();
        var audit = new RecordingAuditAppender();
        var logger = new TestLogger<AuditedObservedWorkProductHttpOperations>();
        var activities = new List<Activity>();
        using var listener = CreateActivityListener(activities);
        var operations = new AuditedObservedWorkProductHttpOperations(inner, audit, logger);
        var workProductId = new WorkProductId(Guid.NewGuid());

        var create = await operations.CreateAsync(
            context,
            new CreateWorkProductRequest("Sensitive design title", "governed-document"),
            CancellationToken.None);
        var submit = await operations.SubmitAsync(
            context,
            new WorkProductId(create.WorkProductId),
            new SubmitWorkProductRequest(Guid.NewGuid()),
            CancellationToken.None);
        var decideCorrelation = "decision-correlation-001";
        var decide = await operations.DecideAsync(
            context,
            workProductId,
            new DecideWorkProductRequest(
                submit.ReviewRequestId,
                ReviewDecisionOutcome.Approved,
                "Sensitive decision rationale that must not enter telemetry or generic audit payloads"),
            decideCorrelation,
            CancellationToken.None);
        var issueCorrelation = "issue-correlation-001";
        var issue = await operations.IssueAsync(
            context,
            workProductId,
            issueCorrelation,
            CancellationToken.None);

        Assert.AreEqual(4, audit.Evidence.Count);
        var expectedActionCodes = new[]
        {
            "work_product.create",
            "work_product.review.submit",
            "work_product.review.decide",
            "work_product.issue"
        };
        CollectionAssert.AreEqual(
            expectedActionCodes,
            audit.Evidence.Select(item => item.ActionCode).ToArray());

        foreach (var evidence in audit.Evidence)
        {
            Assert.AreEqual(context.Tenant.TenantId, evidence.TenantId);
            Assert.AreEqual(context.Principal.PrincipalId, evidence.ActorPrincipalId);
            Assert.AreEqual(AuditActorKind.HumanPrincipal, evidence.ActorKind);
            Assert.AreEqual(AuditOutcome.Succeeded, evidence.Outcome);
            Assert.AreEqual("nublox.work_products.http", evidence.Source);
            Assert.AreEqual("work_product", evidence.Subject.SubjectType);
            Assert.IsFalse(string.IsNullOrWhiteSpace(evidence.CorrelationId));
            Assert.IsNull(evidence.ReasonReference);
        }

        Assert.AreEqual(decideCorrelation, audit.Evidence[2].CorrelationId);
        Assert.AreEqual(issueCorrelation, audit.Evidence[3].CorrelationId);
        Assert.AreEqual(decide.DecisionEvidenceId.ToString("D"), audit.Evidence[2].EvidenceReferences.Single().ReferenceId);
        Assert.AreEqual(issue.IssueEvidenceId.ToString("D"), audit.Evidence[3].EvidenceReferences.Single().ReferenceId);

        var allAuditText = string.Join('|', audit.Evidence.SelectMany(FlattenAuditText));
        Assert.IsFalse(allAuditText.Contains("Sensitive design title", StringComparison.Ordinal));
        Assert.IsFalse(allAuditText.Contains("Sensitive decision rationale", StringComparison.Ordinal));

        Assert.AreEqual(4, activities.Count(activity => activity.OperationName != "work_product.read"));
        var decisionActivity = activities.Single(activity => activity.OperationName == "work_product.review.decide");
        Assert.AreEqual(decideCorrelation, decisionActivity.GetTagItem("nublox.correlation.id"));
        Assert.AreEqual(context.Tenant.TenantId.ToString(), decisionActivity.GetTagItem("nublox.tenant.id"));
        Assert.AreEqual(context.Principal.PrincipalId.ToString(), decisionActivity.GetTagItem("nublox.principal.id"));

        var activityText = string.Join('|', activities.SelectMany(activity =>
            activity.TagObjects.Select(tag => $"{tag.Key}={tag.Value}")));
        Assert.IsFalse(activityText.Contains("Sensitive design title", StringComparison.Ordinal));
        Assert.IsFalse(activityText.Contains("Sensitive decision rationale", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ReadProducesTelemetryWithoutAuthoritativeBusinessAudit()
    {
        var context = await CreateVerifiedContextAsync();
        var audit = new RecordingAuditAppender();
        var activities = new List<Activity>();
        using var listener = CreateActivityListener(activities);
        var operations = new AuditedObservedWorkProductHttpOperations(
            new RecordingOperations(),
            audit,
            new TestLogger<AuditedObservedWorkProductHttpOperations>());

        _ = await operations.FindAsync(context, new WorkProductId(Guid.NewGuid()), CancellationToken.None);

        Assert.AreEqual(0, audit.Evidence.Count);
        Assert.AreEqual(1, activities.Count(activity => activity.OperationName == "work_product.read"));
    }

    [TestMethod]
    public async Task FailedMaterialOperationDoesNotAppendSuccessfulAuditEvidence()
    {
        var context = await CreateVerifiedContextAsync();
        var audit = new RecordingAuditAppender();
        var inner = new RecordingOperations
        {
            Failure = new WorkProductStateConflictException("conflict")
        };
        var activities = new List<Activity>();
        using var listener = CreateActivityListener(activities);
        var operations = new AuditedObservedWorkProductHttpOperations(
            inner,
            audit,
            new TestLogger<AuditedObservedWorkProductHttpOperations>());

        await Assert.ThrowsAsync<WorkProductStateConflictException>(() => operations.SubmitAsync(
            context,
            new WorkProductId(Guid.NewGuid()),
            new SubmitWorkProductRequest(Guid.NewGuid()),
            CancellationToken.None));

        Assert.AreEqual(0, audit.Evidence.Count);
        Assert.AreEqual(1, activities.Count(activity => activity.OperationName == "work_product.review.submit"));
    }

    [TestMethod]
    public void AuditAndTelemetryRemainSeparateAssembliesAndContracts()
    {
        var auditReferences = typeof(AuditEvidence).Assembly.GetReferencedAssemblies();
        Assert.IsFalse(auditReferences.Any(reference =>
            reference.Name?.Contains("Observability", StringComparison.OrdinalIgnoreCase) == true
            || reference.Name?.Contains("OpenTelemetry", StringComparison.OrdinalIgnoreCase) == true));

        var telemetryReferences = typeof(NuBloxTelemetry).Assembly.GetReferencedAssemblies();
        Assert.IsFalse(telemetryReferences.Any(reference =>
            reference.Name?.Contains("NuBlox.Audit", StringComparison.OrdinalIgnoreCase) == true));
    }

    private static ActivityListener CreateActivityListener(List<Activity> stoppedActivities)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == NuBloxTelemetry.ActivitySourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => stoppedActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private static IEnumerable<string> FlattenAuditText(AuditEvidence evidence)
    {
        yield return evidence.ActionCode;
        yield return evidence.Source;
        yield return evidence.Subject.SubjectType;
        yield return evidence.Subject.SubjectId;
        yield return evidence.CorrelationId ?? string.Empty;
        yield return evidence.TraceId ?? string.Empty;
        yield return evidence.ReasonReference ?? string.Empty;
        foreach (var reference in evidence.EvidenceReferences)
        {
            yield return reference.ReferenceType;
            yield return reference.ReferenceId;
        }
    }

    private static async Task<AuthenticatedRequestContext> CreateVerifiedContextAsync()
    {
        var principal = AuthenticatedPrincipal.CreateHuman(
            PrincipalId.New(),
            new ExternalIdentity("https://identity.example.test", "audit-telemetry-subject"));
        var tenant = new TenantDirectoryEntry(TenantId.New(), new TenantRouteSlug("tenant-a"), true);
        var resolver = new IdentityContextResolver(
            new SingleTenantDirectory(tenant),
            new SingleTenantAccess(principal.PrincipalId, tenant.TenantId));
        return await resolver.ResolveAsync(principal, "tenant-a");
    }

    private sealed class RecordingAuditAppender : IAuditEvidenceAppender
    {
        public List<AuditEvidence> Evidence { get; } = [];

        public ValueTask AppendAsync(AuditEvidence evidence, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Evidence.Add(evidence);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingOperations : IWorkProductHttpOperations
    {
        public Exception? Failure { get; init; }

        public Task<WorkProductContract> CreateAsync(
            AuthenticatedRequestContext context,
            CreateWorkProductRequest request,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured();
            return Task.FromResult(new WorkProductContract(
                Guid.NewGuid(), request.Title, request.ProductType,
                context.Principal.PrincipalId.Value, 1, "Draft"));
        }

        public Task<WorkProductContract?> FindAsync(
            AuthenticatedRequestContext context,
            WorkProductId workProductId,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured();
            return Task.FromResult<WorkProductContract?>(new WorkProductContract(
                workProductId.Value, "Read title", "governed-document",
                context.Principal.PrincipalId.Value, 1, "Draft"));
        }

        public Task<ReviewSubmissionContract> SubmitAsync(
            AuthenticatedRequestContext context,
            WorkProductId workProductId,
            SubmitWorkProductRequest request,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured();
            return Task.FromResult(new ReviewSubmissionContract(
                workProductId.Value, Guid.NewGuid(), 1, "InReview", Guid.NewGuid(), request.ReviewerPrincipalId));
        }

        public Task<ReviewDecisionContract> DecideAsync(
            AuthenticatedRequestContext context,
            WorkProductId workProductId,
            DecideWorkProductRequest request,
            string correlationId,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured();
            return Task.FromResult(new ReviewDecisionContract(
                workProductId.Value, Guid.NewGuid(), "Approved", Guid.NewGuid(), request.Outcome.ToString()));
        }

        public Task<IssueWorkProductContract> IssueAsync(
            AuthenticatedRequestContext context,
            WorkProductId workProductId,
            string correlationId,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured();
            return Task.FromResult(new IssueWorkProductContract(
                workProductId.Value, Guid.NewGuid(), "Issued", Guid.NewGuid()));
        }

        private void ThrowIfConfigured()
        {
            if (Failure is not null) throw Failure;
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }

    private sealed class SingleTenantDirectory(TenantDirectoryEntry tenant) : ITenantDirectory
    {
        public ValueTask<TenantDirectoryEntry?> FindBySlugAsync(
            TenantRouteSlug routeSlug,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<TenantDirectoryEntry?>(routeSlug == tenant.CanonicalSlug ? tenant : null);
    }

    private sealed class SingleTenantAccess(PrincipalId principalId, TenantId tenantId) : IPrincipalTenantAccessEvaluator
    {
        public ValueTask<bool> HasAccessAsync(
            PrincipalId requestedPrincipalId,
            TenantId requestedTenantId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(requestedPrincipalId == principalId && requestedTenantId == tenantId);
    }
}
