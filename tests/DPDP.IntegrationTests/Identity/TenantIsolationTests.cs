using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using DPDP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DPDP.IntegrationTests.Identity;

/// <summary>
/// Proves tenant isolation at the layer docs/ARCHITECTURE.md section 4
/// identifies as the real backstop — the EF Core global query filter on
/// User — independent of any API-layer permission check. This is the
/// explicit "Organisation A cannot access Organisation B users" test
/// required by the Module 2 brief. See also
/// DPDP.ApiTests for the same guarantee exercised over real HTTP.
/// </summary>
public sealed class TenantIsolationTests : IAsyncLifetime
{
    private readonly Guid _organisationAId = Guid.NewGuid();
    private readonly Guid _organisationBId = Guid.NewGuid();
    private readonly Guid _userAId = Guid.NewGuid();
    private readonly Guid _userBId = Guid.NewGuid();

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ConnectionStrings__Default")
        ?? throw new InvalidOperationException("Set ConnectionStrings__Default before running integration tests.");

    private static DpdpDbContext CreateContext(DPDP.Application.Common.Interfaces.ICurrentUserContext currentUser)
    {
        var options = new DbContextOptionsBuilder<DpdpDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DpdpDbContext(options, currentUser);
    }

    public async Task InitializeAsync()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(null, isSuperAdministrator: true));

        var now = DateTimeOffset.UtcNow;

        db.Organisations.AddRange(
            new Organisation { Id = _organisationAId, Name = $"Tenant-Isolation-Test-A-{_organisationAId:N}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Organisation { Id = _organisationBId, Name = $"Tenant-Isolation-Test-B-{_organisationBId:N}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now });

        db.Users.AddRange(
            new User
            {
                Id = _userAId, OrganisationId = _organisationAId,
                Email = $"user-a-{_userAId:N}@test.local", NormalizedEmail = $"USER-A-{_userAId:N}@TEST.LOCAL",
                PasswordHash = "not-a-real-hash", FullName = "Tenant A User",
                CreatedAt = now, UpdatedAt = now,
            },
            new User
            {
                Id = _userBId, OrganisationId = _organisationBId,
                Email = $"user-b-{_userBId:N}@test.local", NormalizedEmail = $"USER-B-{_userBId:N}@TEST.LOCAL",
                PasswordHash = "not-a-real-hash", FullName = "Tenant B User",
                CreatedAt = now, UpdatedAt = now,
            });

        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(null, isSuperAdministrator: true));

        await db.Users.IgnoreQueryFilters().Where(u => u.Id == _userAId || u.Id == _userBId).ExecuteDeleteAsync();
        await db.Organisations.IgnoreQueryFilters().Where(o => o.Id == _organisationAId || o.Id == _organisationBId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task A_caller_in_organisation_A_sees_only_organisation_As_users()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(_organisationAId));

        var visibleUserIds = await db.Users.Select(u => u.Id).ToListAsync();

        Assert.Contains(_userAId, visibleUserIds);
        Assert.DoesNotContain(_userBId, visibleUserIds);
    }

    [Fact]
    public async Task A_caller_in_organisation_B_sees_only_organisation_Bs_users()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(_organisationBId));

        var visibleUserIds = await db.Users.Select(u => u.Id).ToListAsync();

        Assert.Contains(_userBId, visibleUserIds);
        Assert.DoesNotContain(_userAId, visibleUserIds);
    }

    [Fact]
    public async Task A_caller_in_organisation_A_cannot_fetch_organisation_Bs_user_by_id()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(_organisationAId));

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == _userBId);

        Assert.Null(user);
    }

    [Fact]
    public async Task A_super_administrator_sees_users_from_every_organisation()
    {
        await using var db = CreateContext(new FakeCurrentUserContext(null, isSuperAdministrator: true));

        var visibleUserIds = await db.Users.Select(u => u.Id).ToListAsync();

        Assert.Contains(_userAId, visibleUserIds);
        Assert.Contains(_userBId, visibleUserIds);
    }
}
