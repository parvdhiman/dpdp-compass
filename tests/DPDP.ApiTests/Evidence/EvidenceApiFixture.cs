using System.Net.Http.Json;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using DPDP.Infrastructure.Persistence;
using DPDP.Infrastructure.Persistence.Configurations.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DPDP.ApiTests.Evidence;

/// <summary>
/// Seeds two isolated organisations. Org A gets an Uploader (Privacy
/// Officer — holds evidence.upload but NOT evidence.review, a deliberate
/// separation-of-duties control, see docs/EVIDENCE_STORAGE.md), a Reviewer
/// (Compliance Officer — holds evidence.review but NOT evidence.upload),
/// and a Read Only user. Org B gets its own Uploader for tenant-isolation
/// tests. Plus a Super Administrator, same shape as the other module
/// fixtures.
/// </summary>
public sealed class EvidenceApiFixture : IAsyncLifetime
{
    public const string DefaultPassword = "Qw7#Falcon!Ridge9";

    private readonly DpdpApiFactory _factory = new();

    public Guid OrganisationAId { get; } = Guid.NewGuid();
    public Guid OrganisationBId { get; } = Guid.NewGuid();

    public string SuperAdministratorEmail { get; private set; } = string.Empty;
    public string UploaderAEmail { get; private set; } = string.Empty;
    public string ReviewerAEmail { get; private set; } = string.Empty;
    public string ReadOnlyAEmail { get; private set; } = string.Empty;
    public string UploaderBEmail { get; private set; } = string.Empty;
    public Guid ReviewerAUserId { get; private set; }

    private readonly List<Guid> _createdUserIds = [];

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task InitializeAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        SuperAdministratorEmail = $"test-evid-superadmin-{suffix}@test.local";
        UploaderAEmail = $"test-evid-uploader-a-{suffix}@test.local";
        ReviewerAEmail = $"test-evid-reviewer-a-{suffix}@test.local";
        ReadOnlyAEmail = $"test-evid-ro-a-{suffix}@test.local";
        UploaderBEmail = $"test-evid-uploader-b-{suffix}@test.local";

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var now = DateTimeOffset.UtcNow;
        var passwordHash = passwordHasher.HashPassword(DefaultPassword);

        db.Organisations.AddRange(
            new Organisation { Id = OrganisationAId, Name = $"EvidTest-Org-A-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Organisation { Id = OrganisationBId, Name = $"EvidTest-Org-B-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now });

        var superAdmin = NewUser(null, SuperAdministratorEmail, "Test Super Administrator", passwordHash, now);
        var uploaderA = NewUser(OrganisationAId, UploaderAEmail, "Test Uploader A", passwordHash, now);
        var reviewerA = NewUser(OrganisationAId, ReviewerAEmail, "Test Reviewer A", passwordHash, now);
        var readOnlyA = NewUser(OrganisationAId, ReadOnlyAEmail, "Test Read Only A", passwordHash, now);
        var uploaderB = NewUser(OrganisationBId, UploaderBEmail, "Test Uploader B", passwordHash, now);

        ReviewerAUserId = reviewerA.Id;

        superAdmin.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.SuperAdministrator), OrganisationId = null, AssignedAt = now });
        uploaderA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.PrivacyOfficer), OrganisationId = OrganisationAId, AssignedAt = now });
        reviewerA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ComplianceOfficer), OrganisationId = OrganisationAId, AssignedAt = now });
        readOnlyA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ReadOnly), OrganisationId = OrganisationAId, AssignedAt = now });
        uploaderB.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.PrivacyOfficer), OrganisationId = OrganisationBId, AssignedAt = now });

        db.Users.AddRange(superAdmin, uploaderA, reviewerA, readOnlyA, uploaderB);
        _createdUserIds.AddRange([superAdmin.Id, uploaderA.Id, reviewerA.Id, readOnlyA.Id, uploaderB.Id]);

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

        await db.EvidenceReviewRecords.IgnoreQueryFilters()
            .Where(r => r.OrganisationId == OrganisationAId || r.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.EvidenceVersions.IgnoreQueryFilters()
            .Where(v => v.OrganisationId == OrganisationAId || v.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.EvidenceItems.IgnoreQueryFilters()
            .Where(e => e.OrganisationId == OrganisationAId || e.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.Users.IgnoreQueryFilters().Where(u => _createdUserIds.Contains(u.Id)).ExecuteDeleteAsync();
        await db.Organisations.IgnoreQueryFilters()
            .Where(o => o.Id == OrganisationAId || o.Id == OrganisationBId)
            .ExecuteDeleteAsync();

        await _factory.DisposeAsync();
    }
}
