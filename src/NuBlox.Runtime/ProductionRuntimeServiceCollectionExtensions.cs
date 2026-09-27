using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NuBlox.Audit;
using NuBlox.Audit.Infrastructure.PostgreSql;
using NuBlox.Authority;
using NuBlox.Kernel;
using NuBlox.WorkProducts.Application;
using NuBlox.WorkProducts.Infrastructure.PostgreSql;

namespace NuBlox.Runtime;

public static class ProductionRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddNuBloxProductionRuntime(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton(_ => NpgsqlDataSource.Create(ResolvePostgresConnectionString(configuration)));

        services.AddScoped<IWorkProductRepository, PostgresGovernedWorkProductRepository>();
        services.AddScoped<IWorkProductIssueRepository, PostgresWorkProductIssueRepository>();
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddScoped<IWorkProductAccessEvaluator, FirstSliceWorkProductAccessEvaluator>();
        services.AddScoped<IWorkProductIssueAccessEvaluator, FirstSliceWorkProductIssueAccessEvaluator>();
        services.AddScoped<IBusinessAuthorityEvaluator>(_ => new ConfigurationBusinessAuthorityEvaluator(configuration));
        services.AddScoped<IAuditEvidenceAppender, PostgresAuditEvidenceAppender>();
        services.AddScoped<WorkProductApplicationService>();
        services.AddScoped<WorkProductIssueService>();

        return services;
    }

    private static string ResolvePostgresConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NuBloxPostgres")
            ?? configuration["NUBLOX_POSTGRES_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "NuBlox PostgreSQL is not configured. Set ConnectionStrings:NuBloxPostgres or NUBLOX_POSTGRES_CONNECTION_STRING.");
        }

        return connectionString;
    }
}

public sealed class FirstSliceWorkProductAccessEvaluator : IWorkProductAccessEvaluator
{
    public ValueTask<bool> CanSubmitForReviewAsync(
        WorkProductRecord record,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(record);
        return ValueTask.FromResult(record.WorkProduct.OwnerPrincipalId == actorPrincipalId);
    }

    public ValueTask<bool> CanDecideReviewAsync(
        ReviewDecisionContext context,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);
        return ValueTask.FromResult(context.RequestedPrincipalId == actorPrincipalId);
    }
}

public sealed class FirstSliceWorkProductIssueAccessEvaluator : IWorkProductIssueAccessEvaluator
{
    public ValueTask<bool> CanIssueAsync(
        WorkProductIssueContext context,
        PrincipalId actorPrincipalId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);
        return ValueTask.FromResult(context.Record.WorkProduct.OwnerPrincipalId == actorPrincipalId);
    }
}

public sealed class ConfigurationBusinessAuthorityEvaluator : IBusinessAuthorityEvaluator
{
    private readonly IReadOnlyList<ConfiguredAuthorityGrant> _grants;

    public ConfigurationBusinessAuthorityEvaluator(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _grants = configuration.GetSection("NuBlox:Authority:Grants")
            .GetChildren()
            .Select(ConfiguredAuthorityGrant.FromConfiguration)
            .ToArray();
    }

    public ValueTask<AuthorityEvaluationResult> EvaluateAsync(
        AuthorityEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        var grant = _grants.FirstOrDefault(candidate =>
            candidate.TenantId == context.TenantId.Value
            && candidate.PrincipalId == context.PrincipalId.Value
            && string.Equals(candidate.ActionCode, context.Requirement.ActionCode, StringComparison.Ordinal));

        return ValueTask.FromResult(grant is null
            ? AuthorityEvaluationResult.Denied("configured-authority-grant-not-found")
            : AuthorityEvaluationResult.Granted(grant.AuthorityReference));
    }

    private sealed record ConfiguredAuthorityGrant(
        Guid TenantId,
        Guid PrincipalId,
        string ActionCode,
        string AuthorityReference)
    {
        public static ConfiguredAuthorityGrant FromConfiguration(IConfigurationSection section)
        {
            if (!Guid.TryParse(section["TenantId"], out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException($"Authority grant '{section.Path}' has an invalid TenantId.");
            }

            if (!Guid.TryParse(section["PrincipalId"], out var principalId) || principalId == Guid.Empty)
            {
                throw new InvalidOperationException($"Authority grant '{section.Path}' has an invalid PrincipalId.");
            }

            var actionCode = section["ActionCode"];
            var authorityReference = section["AuthorityReference"];
            if (string.IsNullOrWhiteSpace(actionCode) || string.IsNullOrWhiteSpace(authorityReference))
            {
                throw new InvalidOperationException($"Authority grant '{section.Path}' requires ActionCode and AuthorityReference.");
            }

            return new ConfiguredAuthorityGrant(
                tenantId,
                principalId,
                actionCode.Trim(),
                authorityReference.Trim());
        }
    }
}
