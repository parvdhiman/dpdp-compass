using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.Connectors;
using DPDP.Infrastructure.DataDiscovery;
using DPDP.Infrastructure.DataDiscovery.Connectors;
using DPDP.Infrastructure.Persistence;
using DPDP.Infrastructure.Persistence.Interceptors;
using DPDP.Infrastructure.Security;
using DPDP.Infrastructure.Services;
using DPDP.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DPDP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is not configured. Set ConnectionStrings__Default " +
                "via environment variable or user-secrets — see .env.example.");

        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddDbContext<DpdpDbContext>((sp, options) => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>())
            // User's global query filter vs. its required RefreshToken/
            // PasswordResetToken/UserRole navigations: EF warns this could
            // silently drop rows. We intentionally bypass the filter with
            // .IgnoreQueryFilters() at every pre-authentication query site
            // that loads User through these navigations (Login, Refresh,
            // ResetPassword) — see docs/ARCHITECTURE.md section 11.
            .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<DpdpDbContext>());

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IApplicationInfo, ApplicationInfoProvider>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddScoped<IRequestContext, HttpRequestContext>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<INotificationService, LoggingNotificationService>();

        services.Configure<ObjectStorageOptions>(configuration.GetSection(ObjectStorageOptions.SectionName));
        services.AddSingleton<IObjectStorageService, FileSystemObjectStorageService>();
        services.AddSingleton<IMalwareScanner, NoOpMalwareScanner>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Data Discovery (Module 8). Key persistence: without this,
        // DataSource.EncryptedSecret becomes undecryptable after every API
        // process restart — see docs/DATA_DISCOVERY.md section 2. Same
        // relative-path-under-working-directory convention as
        // ObjectStorageOptions.RootPath; production deployments running
        // more than one instance must point this at a shared location.
        var dataProtectionKeysPath = configuration["DataProtection:KeysPath"] ?? "dataprotection-keys";
        services.AddDataProtection()
            .SetApplicationName("DPDP-Compass")
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
        services.AddSingleton<IConnectionSecretProtector, ConnectionSecretProtector>();

        services.AddSingleton<IDiscoveryJobQueue, DiscoveryJobQueue>();
        services.AddSingleton<IDiscoveryCancellationRegistry, InMemoryDiscoveryCancellationRegistry>();
        services.AddHostedService<DiscoveryJobBackgroundService>();

        services.AddScoped<IDiscoveryConnector, PostgresDiscoveryConnector>();
        services.AddScoped<IDiscoveryConnector, MySqlDiscoveryConnector>();
        services.AddScoped<IDiscoveryConnector, SqlServerDiscoveryConnector>();
        services.AddScoped<IDiscoveryConnector, FileSystemDiscoveryConnector>();

        services.AddHealthChecks()
            .AddNpgSql(
                connectionString,
                name: "postgresql",
                tags: ["ready"]);

        return services;
    }
}
