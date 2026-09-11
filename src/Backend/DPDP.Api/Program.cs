using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using DPDP.Api.ExceptionHandling;
using DPDP.Api.Middleware;
using DPDP.Api.Modules.Assessments;
using DPDP.Api.Modules.Auth;
using DPDP.Api.Modules.BusinessUnits;
using DPDP.Api.Modules.Compliance;
using DPDP.Api.Modules.ConsentPrivacy;
using DPDP.Api.Modules.DataDiscovery;
using DPDP.Api.Modules.DataInventory;
using DPDP.Api.Modules.Departments;
using DPDP.Api.Modules.Evidence;
using DPDP.Api.Modules.Findings;
using DPDP.Api.Modules.Health;
using DPDP.Api.Modules.Organisations;
using DPDP.Api.Modules.Permissions;
using DPDP.Api.Modules.Remediation;
using DPDP.Api.Modules.Risks;
using DPDP.Api.Modules.Roles;
using DPDP.Api.Modules.System;
using DPDP.Api.Modules.Users;
using DPDP.Api.OpenApi;
using DPDP.Application;
using DPDP.Infrastructure;
using DPDP.Infrastructure.Persistence.Seed;
using DPDP.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // --- Services -----------------------------------------------------

    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    // Matches EvidenceOptions.MaxUploadSizeBytes (25 MB) with headroom for
    // multipart form overhead — rejects oversized uploads at the transport
    // level before the request body is fully buffered.
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 30 * 1024 * 1024;
    });

    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            var correlationId = context.HttpContext.Response.Headers["X-Correlation-Id"].ToString();
            context.ProblemDetails.Extensions["correlationId"] = correlationId;
        };
    });
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
        ?? throw new InvalidOperationException(
            "Jwt:SigningKey is not configured. Set it via user-secrets/environment variable — see .env.example.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "dpdp-compass",
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "dpdp-compass",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
                ClockSkew = TimeSpan.FromSeconds(30),
            };
        });

    builder.Services.AddAuthorization();

    var authRateLimitPermits = builder.Configuration.GetValue("RateLimiting:Auth:PermitLimit", 10);
    var authRateLimitWindowSeconds = builder.Configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60);

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Partitioned per client IP — a named-but-unpartitioned limiter
        // would share one bucket across every caller, so one abusive
        // client could lock every legitimate user out of /auth/* at once.
        // Limit itself is configurable (appsettings.Development.json
        // relaxes it — every request from a WebApplicationFactory-hosted
        // test suite shares one loopback "IP", so the Production-strict
        // default would throttle the test run itself, not just abuse).
        options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authRateLimitPermits,
                Window = TimeSpan.FromSeconds(authRateLimitWindowSeconds),
                QueueLimit = 0,
            }));
    });

    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.TryAddEnumerable(
        ServiceDescriptor.Transient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>());
    // Note: Swagger UI has no built-in "Authorize" button configured in
    // Module 2 (Microsoft.OpenApi v2's security-scheme API shape wasn't
    // worth the added complexity for this). Test protected endpoints with
    // curl/a REST client, sending `Authorization: Bearer <accessToken>`.
    builder.Services.AddSwaggerGen();

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // Any origin, in Development only — lets the frontend be
                // reached from a LAN IP or tunnelled hostname without
                // pre-registering it. Never applies outside Development;
                // Staging/Production always require an explicit allow-list
                // below. See docs/SECURITY.md section 4.
                policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
            else if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        });
    });

    var app = builder.Build();

    await IdentityBootstrapper.RunAsync(app.Services);

    // --- Middleware pipeline -------------------------------------------

    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseSerilogRequestLogging();

    if (!app.Environment.IsDevelopment())
    {
        // HIGH finding, docs/ARCHITECTURE_REVIEW.md: docs/SECURITY.md §4
        // claimed HSTS was enabled but no code ever called UseHsts() —
        // added here to close that doc/reality gap, not a new feature.
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseCors("Frontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    var apiVersionDescriptions = app.DescribeApiVersions();

    if (!app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });
    }

    var versionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();

    app.MapSystemHealthEndpoints();
    app.MapSystemEndpoints(versionSet);
    app.MapAuthEndpoints(versionSet);
    app.MapUsersEndpoints(versionSet);
    app.MapRolesEndpoints(versionSet);
    app.MapPermissionsEndpoints(versionSet);
    app.MapOrganisationsEndpoints(versionSet);
    app.MapBusinessUnitsEndpoints(versionSet);
    app.MapDepartmentsEndpoints(versionSet);
    app.MapComplianceEndpoints(versionSet);
    app.MapAssessmentsEndpoints(versionSet);
    app.MapFindingsEndpoints(versionSet);
    app.MapRisksEndpoints(versionSet);
    app.MapRemediationEndpoints(versionSet);
    app.MapEvidenceEndpoints(versionSet);
    app.MapDataSourcesEndpoints(versionSet);
    app.MapDiscoveryJobsEndpoints(versionSet);
    app.MapDataAssetsEndpoints(versionSet);
    app.MapDataCategoriesEndpoints(versionSet);
    app.MapItSystemsEndpoints(versionSet);
    app.MapDataCollectionSourcesEndpoints(versionSet);
    app.MapProcessorsEndpoints(versionSet);
    app.MapRecipientsEndpoints(versionSet);
    app.MapRetentionPoliciesEndpoints(versionSet);
    app.MapDataInventoryItemsEndpoints(versionSet);
    app.MapProcessingActivitiesEndpoints(versionSet);
    app.MapDataFlowsEndpoints(versionSet);
    app.MapDataPrincipalsEndpoints(versionSet);
    app.MapConsentPurposesEndpoints(versionSet);
    app.MapSlaPoliciesEndpoints(versionSet);
    app.MapPrivacyNoticesEndpoints(versionSet);
    app.MapConsentRecordsEndpoints(versionSet);
    app.MapDataPrincipalRequestsEndpoints(versionSet);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "DPDP-COMPASS API terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}

namespace DPDP.Api
{
    /// <summary>Marker used by WebApplicationFactory&lt;Program&gt; in tests.</summary>
    public partial class Program;
}
