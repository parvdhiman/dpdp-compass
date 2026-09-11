using System.Net.Http.Json;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using DPDP.Infrastructure.Persistence;
using DPDP.Infrastructure.Persistence.Configurations.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DPDP.ApiTests.Identity;

/// <summary>
/// Seeds two isolated test organisations, each with an Organisation
/// Administrator, plus a dedicated (uniquely named, disposable) Super
/// Administrator — all inserted directly against the real app's DbContext
/// rather than depending on whatever bootstrap state the shared dev
/// database happens to already have (see docs/DEVELOPMENT.md — tests run
/// against a real database, not a mock). Cleaned up in DisposeAsync.
/// </summary>
public sealed class IdentityApiFixture : IAsyncLifetime
{
    public const string DefaultPassword = "Xk9!Zephyr*Batt3ry";

    private readonly DpdpApiFactory _factory = new();

    public Guid OrganisationAId { get; } = Guid.NewGuid();
    public Guid OrganisationBId { get; } = Guid.NewGuid();

    public string SuperAdministratorEmail { get; private set; } = string.Empty;
    public string OrganisationAAdminEmail { get; private set; } = string.Empty;
    public string OrganisationBAdminEmail { get; private set; } = string.Empty;
    public string OrganisationAReadOnlyEmail { get; private set; } = string.Empty;
    public Guid OrganisationAAdminUserId { get; private set; }
    public Guid OrganisationBAdminUserId { get; private set; }
    public Guid OrganisationAReadOnlyUserId { get; private set; }

    private readonly List<Guid> _createdUserIds = [];

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task InitializeAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        SuperAdministratorEmail = $"test-superadmin-{suffix}@test.local";
        OrganisationAAdminEmail = $"test-admin-a-{suffix}@test.local";
        OrganisationBAdminEmail = $"test-admin-b-{suffix}@test.local";
        OrganisationAReadOnlyEmail = $"test-readonly-a-{suffix}@test.local";

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var now = DateTimeOffset.UtcNow;
        var passwordHash = passwordHasher.HashPassword(DefaultPassword);

        db.Organisations.AddRange(
            new Organisation { Id = OrganisationAId, Name = $"ApiTest-Org-A-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Organisation { Id = OrganisationBId, Name = $"ApiTest-Org-B-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now });

        var superAdmin = NewUser(null, SuperAdministratorEmail, "Test Super Administrator", passwordHash, now);
        var orgAAdmin = NewUser(OrganisationAId, OrganisationAAdminEmail, "Test Org A Admin", passwordHash, now);
        var orgBAdmin = NewUser(OrganisationBId, OrganisationBAdminEmail, "Test Org B Admin", passwordHash, now);
        var orgAReadOnly = NewUser(OrganisationAId, OrganisationAReadOnlyEmail, "Test Org A Read Only", passwordHash, now);

        OrganisationAAdminUserId = orgAAdmin.Id;
        OrganisationBAdminUserId = orgBAdmin.Id;
        OrganisationAReadOnlyUserId = orgAReadOnly.Id;

        superAdmin.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.SuperAdministrator), OrganisationId = null, AssignedAt = now });
        orgAAdmin.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.OrganisationAdministrator), OrganisationId = OrganisationAId, AssignedAt = now });
        orgBAdmin.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.OrganisationAdministrator), OrganisationId = OrganisationBId, AssignedAt = now });
        orgAReadOnly.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ReadOnly), OrganisationId = OrganisationAId, AssignedAt = now });

        db.Users.AddRange(superAdmin, orgAAdmin, orgBAdmin, orgAReadOnly);
        _createdUserIds.AddRange([superAdmin.Id, orgAAdmin.Id, orgBAdmin.Id, orgAReadOnly.Id]);

        await db.SaveChangesAsync(CancellationToken.None);
    }

    private static User NewUser(Guid? organisationId, string email, string fullName, string passwordHash, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        OrganisationId = organisationId,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        PasswordHash = passwordHash,
        FullName = fullName,
        IsActive = true,
        CreatedAt = now,
        UpdatedAt = now,
    };

    public async Task<string> LoginAsync(HttpClient client, string email, string password = DefaultPassword)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.AccessToken;
    }

    private sealed record LoginResponse(string AccessToken);

    public async Task DisposeAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = (DpdpDbContext)scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        // Dependency order matters: a *soft*-deleted BusinessUnit/Department
        // still physically exists (DeleteBehavior.Restrict to Organisation),
        // so any Module 3 test that creates org structure under these
        // organisations must be hard-deleted here first, or this
        // Organisations delete below fails with an FK violation — which
        // then leaks the whole fixture's data into the shared dev database
        // permanently, since the *next* run creates fresh GUIDs and never
        // sees the orphans again. See the Module 3 completion report.
        await db.Departments.IgnoreQueryFilters()
            .Where(d => d.OrganisationId == OrganisationAId || d.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.BusinessUnits.IgnoreQueryFilters()
            .Where(b => b.OrganisationId == OrganisationAId || b.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.OrganisationLocations.IgnoreQueryFilters()
            .Where(l => l.OrganisationId == OrganisationAId || l.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.Users.IgnoreQueryFilters().Where(u => _createdUserIds.Contains(u.Id)).ExecuteDeleteAsync();
        await db.Organisations.IgnoreQueryFilters()
            .Where(o => o.Id == OrganisationAId || o.Id == OrganisationBId)
            .ExecuteDeleteAsync();

        await _factory.DisposeAsync();
    }
}
