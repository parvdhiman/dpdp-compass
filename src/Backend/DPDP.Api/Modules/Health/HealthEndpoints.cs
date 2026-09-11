using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DPDP.Api.Modules.Health;

public static class HealthEndpoints
{
    /// <summary>
    /// GET /health — liveness, no dependency checks (is the process up?).
    /// GET /health/ready — readiness, checks dependencies tagged "ready"
    /// (currently PostgreSQL). See docs/API.md section 2.
    /// </summary>
    public static IEndpointRouteBuilder MapSystemHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false,
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
        });

        return app;
    }
}
