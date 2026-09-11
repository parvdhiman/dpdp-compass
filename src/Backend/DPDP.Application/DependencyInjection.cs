using System.Reflection;
using DPDP.Application.Common.Behaviors;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.Scoring;
using DPDP.Application.Modules.DataDiscovery;
using DPDP.Application.Modules.DataDiscovery.Classification;
using DPDP.Application.Modules.DataDiscovery.Connectors;
using DPDP.Application.Modules.Evidence;
using DPDP.Application.Modules.Identity;
using DPDP.Application.Modules.Risks.Scoring;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DPDP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
        services.Configure<AccountSecurityOptions>(configuration.GetSection(AccountSecurityOptions.SectionName));

        services.AddSingleton<IComplianceScoringStrategy, DefaultComplianceScoringStrategy>();
        services.Configure<ScoringOptions>(configuration.GetSection(ScoringOptions.SectionName));

        services.AddSingleton<IRiskScoringStrategy, DefaultRiskScoringStrategy>();
        services.Configure<RiskScoringOptions>(configuration.GetSection(RiskScoringOptions.SectionName));

        services.Configure<EvidenceOptions>(configuration.GetSection(EvidenceOptions.SectionName));

        services.AddSingleton<IDataClassificationStrategy, DefaultDataClassificationStrategy>();
        services.Configure<ClassificationOptions>(configuration.GetSection(ClassificationOptions.SectionName));
        services.Configure<DiscoveryOptions>(configuration.GetSection(DiscoveryOptions.SectionName));
        services.AddScoped<IDiscoveryConnectorResolver, DefaultDiscoveryConnectorResolver>();
        services.AddScoped<IDiscoveryJobProcessor, DiscoveryJobProcessor>();

        return services;
    }
}
