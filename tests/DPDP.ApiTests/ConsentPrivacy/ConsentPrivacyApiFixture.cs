using System.Net.Http.Json;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using DPDP.Infrastructure.Persistence;
using DPDP.Infrastructure.Persistence.Configurations.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DPDP.ApiTests.ConsentPrivacy;

/// <summary>
/// Seeds two isolated organisations. Org A gets a Privacy Officer (holds
/// privacynotices/consentpurposes/consent/dataprincipals/datarequests
/// .manage — the module's day-to-day operator), a Compliance Officer
/// (holds privacynotices.approve but NOT .manage or datarequests.manage —
/// a deliberate separation-of-duties control mirroring Module 9's
/// Reviewer), and a Read Only user. Org B gets its own Privacy Officer
/// for tenant-isolation tests. Plus a Super Administrator.
/// </summary>
public sealed class ConsentPrivacyApiFixture : IAsyncLifetime
{
    public const string DefaultPassword = "Rk3!Compass#Ledger8";

    private readonly DpdpApiFactory _factory = new();

    public Guid OrganisationAId { get; } = Guid.NewGuid();
    public Guid OrganisationBId { get; } = Guid.NewGuid();

    public string SuperAdministratorEmail { get; private set; } = string.Empty;
    public string PrivacyOfficerAEmail { get; private set; } = string.Empty;
    public string ComplianceOfficerAEmail { get; private set; } = string.Empty;
    public string ReadOnlyAEmail { get; private set; } = string.Empty;
    public string PrivacyOfficerBEmail { get; private set; } = string.Empty;

    public Guid PrivacyOfficerAUserId { get; private set; }

    private readonly List<Guid> _createdUserIds = [];

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task InitializeAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        SuperAdministratorEmail = $"test-cp-superadmin-{suffix}@test.local";
        PrivacyOfficerAEmail = $"test-cp-privacy-a-{suffix}@test.local";
        ComplianceOfficerAEmail = $"test-cp-compliance-a-{suffix}@test.local";
        ReadOnlyAEmail = $"test-cp-ro-a-{suffix}@test.local";
        PrivacyOfficerBEmail = $"test-cp-privacy-b-{suffix}@test.local";

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var now = DateTimeOffset.UtcNow;
        var passwordHash = passwordHasher.HashPassword(DefaultPassword);

        db.Organisations.AddRange(
            new Organisation { Id = OrganisationAId, Name = $"CPTest-Org-A-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Organisation { Id = OrganisationBId, Name = $"CPTest-Org-B-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now });

        var superAdmin = NewUser(null, SuperAdministratorEmail, "Test Super Administrator", passwordHash, now);
        var privacyOfficerA = NewUser(OrganisationAId, PrivacyOfficerAEmail, "Test Privacy Officer A", passwordHash, now);
        var complianceOfficerA = NewUser(OrganisationAId, ComplianceOfficerAEmail, "Test Compliance Officer A", passwordHash, now);
        var readOnlyA = NewUser(OrganisationAId, ReadOnlyAEmail, "Test Read Only A", passwordHash, now);
        var privacyOfficerB = NewUser(OrganisationBId, PrivacyOfficerBEmail, "Test Privacy Officer B", passwordHash, now);

        superAdmin.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.SuperAdministrator), OrganisationId = null, AssignedAt = now });
        privacyOfficerA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.PrivacyOfficer), OrganisationId = OrganisationAId, AssignedAt = now });
        complianceOfficerA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ComplianceOfficer), OrganisationId = OrganisationAId, AssignedAt = now });
        readOnlyA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ReadOnly), OrganisationId = OrganisationAId, AssignedAt = now });
        privacyOfficerB.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.PrivacyOfficer), OrganisationId = OrganisationBId, AssignedAt = now });

        db.Users.AddRange(superAdmin, privacyOfficerA, complianceOfficerA, readOnlyA, privacyOfficerB);
        _createdUserIds.AddRange([superAdmin.Id, privacyOfficerA.Id, complianceOfficerA.Id, readOnlyA.Id, privacyOfficerB.Id]);
        PrivacyOfficerAUserId = privacyOfficerA.Id;

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

    public async Task<int> CountAuditLogsAsync(string action, string entityId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = (DpdpDbContext)scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        return await db.AuditLogs.IgnoreQueryFilters().CountAsync(a => a.Action == action && a.EntityId == entityId);
    }

    public async Task DisposeAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = (DpdpDbContext)scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM privacy_notice_data_categories WHERE privacy_notice_id IN (SELECT id FROM privacy_notices WHERE organisation_id = {0} OR organisation_id = {1})",
            OrganisationAId, OrganisationBId);
        await db.DataPrincipalRequests.IgnoreQueryFilters().Where(r => r.OrganisationId == OrganisationAId || r.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.ConsentRecords.IgnoreQueryFilters().Where(c => c.OrganisationId == OrganisationAId || c.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.SlaPolicies.IgnoreQueryFilters().Where(p => p.OrganisationId == OrganisationAId || p.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.PrivacyNotices.IgnoreQueryFilters().Where(n => n.OrganisationId == OrganisationAId || n.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.ConsentPurposes.IgnoreQueryFilters().Where(p => p.OrganisationId == OrganisationAId || p.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.DataPrincipals.IgnoreQueryFilters().Where(p => p.OrganisationId == OrganisationAId || p.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.AuditLogs.IgnoreQueryFilters().Where(a => a.OrganisationId == OrganisationAId || a.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.Users.IgnoreQueryFilters().Where(u => _createdUserIds.Contains(u.Id)).ExecuteDeleteAsync();
        await db.Organisations.IgnoreQueryFilters().Where(o => o.Id == OrganisationAId || o.Id == OrganisationBId).ExecuteDeleteAsync();

        await _factory.DisposeAsync();
    }
}
