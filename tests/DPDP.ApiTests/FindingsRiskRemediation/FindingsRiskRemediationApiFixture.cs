using System.Net.Http.Json;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using DPDP.Infrastructure.Persistence;
using DPDP.Infrastructure.Persistence.Configurations.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DPDP.ApiTests.FindingsRiskRemediation;

/// <summary>
/// Seeds two isolated organisations, each with a Compliance Officer (holds
/// findings.create/assign/close, risks.manage, remediation.manage — every
/// Module 6 permission from one actor) and a Read Only user, plus a Super
/// Administrator — the same shape as AssessmentApiFixture. Deleting
/// Findings first is enough to cascade every RemediationTask/
/// RemediationComment beneath it (see docs/DATABASE.md Module 6 section);
/// Risks are independent rows, deleted separately.
/// </summary>
public sealed class FindingsRiskRemediationApiFixture : IAsyncLifetime
{
    public const string DefaultPassword = "Xk9!Zephyr*Batt3ry";

    private readonly DpdpApiFactory _factory = new();

    public Guid OrganisationAId { get; } = Guid.NewGuid();
    public Guid OrganisationBId { get; } = Guid.NewGuid();
    public Guid FrameworkVersionId { get; private set; }

    public string SuperAdministratorEmail { get; private set; } = string.Empty;
    public string ComplianceOfficerAEmail { get; private set; } = string.Empty;
    public string ComplianceOfficerBEmail { get; private set; } = string.Empty;
    public string ReadOnlyAEmail { get; private set; } = string.Empty;
    public Guid ComplianceOfficerAUserId { get; private set; }

    private readonly List<Guid> _createdUserIds = [];

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task InitializeAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        SuperAdministratorEmail = $"test-frr-superadmin-{suffix}@test.local";
        ComplianceOfficerAEmail = $"test-frr-co-a-{suffix}@test.local";
        ComplianceOfficerBEmail = $"test-frr-co-b-{suffix}@test.local";
        ReadOnlyAEmail = $"test-frr-ro-a-{suffix}@test.local";

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var currentVersion = await db.FrameworkVersions
            .Include(v => v.Framework)
            .FirstAsync(v => v.Framework.Code == "DPDPA-2023" && v.IsCurrent);
        FrameworkVersionId = currentVersion.Id;

        var now = DateTimeOffset.UtcNow;
        var passwordHash = passwordHasher.HashPassword(DefaultPassword);

        db.Organisations.AddRange(
            new Organisation { Id = OrganisationAId, Name = $"FrrTest-Org-A-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Organisation { Id = OrganisationBId, Name = $"FrrTest-Org-B-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now });

        var superAdmin = NewUser(null, SuperAdministratorEmail, "Test Super Administrator", passwordHash, now);
        var complianceOfficerA = NewUser(OrganisationAId, ComplianceOfficerAEmail, "Test Compliance Officer A", passwordHash, now);
        var complianceOfficerB = NewUser(OrganisationBId, ComplianceOfficerBEmail, "Test Compliance Officer B", passwordHash, now);
        var readOnlyA = NewUser(OrganisationAId, ReadOnlyAEmail, "Test Read Only A", passwordHash, now);

        ComplianceOfficerAUserId = complianceOfficerA.Id;

        superAdmin.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.SuperAdministrator), OrganisationId = null, AssignedAt = now });
        complianceOfficerA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ComplianceOfficer), OrganisationId = OrganisationAId, AssignedAt = now });
        complianceOfficerB.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ComplianceOfficer), OrganisationId = OrganisationBId, AssignedAt = now });
        readOnlyA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ReadOnly), OrganisationId = OrganisationAId, AssignedAt = now });

        db.Users.AddRange(superAdmin, complianceOfficerA, complianceOfficerB, readOnlyA);
        _createdUserIds.AddRange([superAdmin.Id, complianceOfficerA.Id, complianceOfficerB.Id, readOnlyA.Id]);

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

        // Findings reference Assessments/AssessmentControls only via
        // SetNull FKs, so Findings must go first, then the Assessments
        // some tests create (which have no FK to Organisations at all —
        // see docs/ARCHITECTURE.md Module 5 section — so they would
        // otherwise be orphaned, not cascaded, by deleting Organisations).
        await db.Findings.IgnoreQueryFilters()
            .Where(f => f.OrganisationId == OrganisationAId || f.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.Risks.IgnoreQueryFilters()
            .Where(r => r.OrganisationId == OrganisationAId || r.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.Assessments.IgnoreQueryFilters()
            .Where(a => a.OrganisationId == OrganisationAId || a.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.Users.IgnoreQueryFilters().Where(u => _createdUserIds.Contains(u.Id)).ExecuteDeleteAsync();
        await db.Organisations.IgnoreQueryFilters()
            .Where(o => o.Id == OrganisationAId || o.Id == OrganisationBId)
            .ExecuteDeleteAsync();

        await _factory.DisposeAsync();
    }
}
