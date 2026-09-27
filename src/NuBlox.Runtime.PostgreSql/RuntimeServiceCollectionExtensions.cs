using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NuBlox.Audit;
using NuBlox.Authority;
using NuBlox.Enterprise.Application;
using NuBlox.Enterprise.Infrastructure.PostgreSql;
using NuBlox.Kernel;
using NuBlox.WorkProducts.Application;
using NuBlox.WorkProducts.Infrastructure.PostgreSql;

namespace NuBlox.Runtime.PostgreSql;

public static class RuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddNuBloxPostgresRuntime(
        this IServiceCollection services,
        string connectionString,
        IEnumerable<PrincipalId>? workProductApprovalPrincipals = null,
        IEnumerable<PrincipalId>? organisationAdministratorPrincipals = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var approvalPrincipals = workProductApprovalPrincipals?.ToArray() ?? [];
        var organisationAdministrators = organisationAdministratorPrincipals?.ToArray() ?? [];

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));

        services.AddSingleton<WorkProductOwnershipAccessEvaluator>();
        services.AddSingleton<IWorkProductAccessEvaluator>(static provider =>
            provider.GetRequiredService<WorkProductOwnershipAccessEvaluator>());
        services.AddSingleton<IWorkProductIssueAccessEvaluator>(static provider =>
            provider.GetRequiredService<WorkProductOwnershipAccessEvaluator>());

        services.AddSingleton<ConfiguredWorkProductAuthorityEvaluator>(_ =>
            new ConfiguredWorkProductAuthorityEvaluator(approvalPrincipals));
        services.AddSingleton<IBusinessAuthorityEvaluator>(static provider =>
            provider.GetRequiredService<ConfiguredWorkProductAuthorityEvaluator>());

        services.AddSingleton<IEnterpriseClock, SystemEnterpriseClock>();
        services.AddSingleton<ConfiguredOrganisationAccessEvaluator>(_ =>
            new ConfiguredOrganisationAccessEvaluator(organisationAdministrators));
        services.AddSingleton<IOrganisationAccessEvaluator>(static provider =>
            provider.GetRequiredService<ConfiguredOrganisationAccessEvaluator>());
        services.AddScoped<IOrganisationRepository, PostgresOrganisationRepository>();
        services.AddScoped<OrganisationApplicationService>();

        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddScoped<IWorkProductRepository, PostgresGovernedWorkProductRepository>();
        services.AddScoped<IWorkProductIssueRepository, PostgresWorkProductIssueRepository>();
        services.AddScoped<WorkProductApplicationService>();
        services.AddScoped<WorkProductIssueService>();
        services.AddScoped<IAuditEvidenceAppender, PostgresAuditEvidenceAppender>();

        return services;
    }
}
