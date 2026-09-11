using DPDP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DPDP.IntegrationTests.Persistence;

/// <summary>
/// Proves the EF Core + PostgreSQL + migrations pipeline actually works
/// end-to-end against a real database — see docs/PROJECT_PLAN.md Phase 1
/// quality gate. Requires ConnectionStrings__Default to point at a
/// reachable PostgreSQL instance (set by CI via a service container, or
/// locally via `dotnet user-secrets` / environment variable).
/// </summary>
public class DpdpDbContextTests
{
    private static DpdpDbContext CreateContext()
    {
        var connectionString = System.Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__Default before running integration tests. See docs/DEVELOPMENT.md.");

        var options = new DbContextOptionsBuilder<DpdpDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DpdpDbContext(options, new DesignTimeCurrentUserContext());
    }

    [Fact]
    public async Task Database_connection_succeeds()
    {
        await using var context = CreateContext();

        var canConnect = await context.Database.CanConnectAsync();

        Assert.True(canConnect);
    }

    [Fact]
    public async Task All_migrations_are_applied()
    {
        await using var context = CreateContext();

        var pending = await context.Database.GetPendingMigrationsAsync();

        Assert.Empty(pending);
    }
}
