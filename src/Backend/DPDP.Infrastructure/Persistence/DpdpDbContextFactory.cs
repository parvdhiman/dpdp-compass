using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DPDP.Infrastructure.Persistence;

/// <summary>
/// Enables `dotnet ef migrations` to construct DpdpDbContext without
/// spinning up the whole Api host. The connection string is read from the
/// ConnectionStrings__Default environment variable only — never a literal
/// here — so `dotnet ef` commands never require a secret to be committed.
/// See docs/SECURITY.md section 7.
/// </summary>
public sealed class DpdpDbContextFactory : IDesignTimeDbContextFactory<DpdpDbContext>
{
    public DpdpDbContext CreateDbContext(string[] args)
    {
        var connectionString = System.Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? throw new InvalidOperationException(
                "Set the ConnectionStrings__Default environment variable before running dotnet ef commands.");

        var optionsBuilder = new DbContextOptionsBuilder<DpdpDbContext>();
        optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

        return new DpdpDbContext(optionsBuilder.Options, new DesignTimeCurrentUserContext());
    }
}
