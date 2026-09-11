using DPDP.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DPDP.ApiTests;

/// <summary>
/// Boots the real Api host for HTTP-level tests. Requires
/// ConnectionStrings__Default in the environment (same variable used by
/// `dotnet run` and `dotnet ef`) — see docs/DEVELOPMENT.md.
/// </summary>
public sealed class DpdpApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
