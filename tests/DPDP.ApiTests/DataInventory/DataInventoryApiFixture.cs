using System.Net.Http.Json;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using DPDP.Infrastructure.Persistence;
using DPDP.Infrastructure.Persistence.Configurations.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DPDP.ApiTests.DataInventory;

/// <summary>
/// Seeds two isolated organisations. Org A gets a Preparer (Privacy
/// Officer — holds datainventory.manage/processingactivities.manage but
/// NOT processingactivities.review/approve, a deliberate
/// separation-of-duties control), a Reviewer (Compliance Officer — holds
/// processingactivities.review/approve but NOT manage), and a Read Only
/// user. Org B gets its own Preparer for tenant-isolation tests. Plus a
/// Super Administrator.
/// </summary>
public sealed class DataInventoryApiFixture : IAsyncLifetime
{
    public const string DefaultPassword = "Rk3!Compass#Ledger8";

    private readonly DpdpApiFactory _factory = new();

    public Guid OrganisationAId { get; } = Guid.NewGuid();
    public Guid OrganisationBId { get; } = Guid.NewGuid();

    public string SuperAdministratorEmail { get; private set; } = string.Empty;
    public string PreparerAEmail { get; private set; } = string.Empty;
    public string ReviewerAEmail { get; private set; } = string.Empty;
    public string ReadOnlyAEmail { get; private set; } = string.Empty;
    public string PreparerBEmail { get; private set; } = string.Empty;

    private readonly List<Guid> _createdUserIds = [];

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task InitializeAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        SuperAdministratorEmail = $"test-dinv-superadmin-{suffix}@test.local";
        PreparerAEmail = $"test-dinv-preparer-a-{suffix}@test.local";
        ReviewerAEmail = $"test-dinv-reviewer-a-{suffix}@test.local";
        ReadOnlyAEmail = $"test-dinv-ro-a-{suffix}@test.local";
        PreparerBEmail = $"test-dinv-preparer-b-{suffix}@test.local";

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var now = DateTimeOffset.UtcNow;
        var passwordHash = passwordHasher.HashPassword(DefaultPassword);

        db.Organisations.AddRange(
            new Organisation { Id = OrganisationAId, Name = $"DInvTest-Org-A-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Organisation { Id = OrganisationBId, Name = $"DInvTest-Org-B-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now });

        var superAdmin = NewUser(null, SuperAdministratorEmail, "Test Super Administrator", passwordHash, now);
        var preparerA = NewUser(OrganisationAId, PreparerAEmail, "Test Preparer A", passwordHash, now);
        var reviewerA = NewUser(OrganisationAId, ReviewerAEmail, "Test Reviewer A", passwordHash, now);
        var readOnlyA = NewUser(OrganisationAId, ReadOnlyAEmail, "Test Read Only A", passwordHash, now);
        var preparerB = NewUser(OrganisationBId, PreparerBEmail, "Test Preparer B", passwordHash, now);

        superAdmin.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.SuperAdministrator), OrganisationId = null, AssignedAt = now });
        preparerA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.PrivacyOfficer), OrganisationId = OrganisationAId, AssignedAt = now });
        reviewerA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ComplianceOfficer), OrganisationId = OrganisationAId, AssignedAt = now });
        readOnlyA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ReadOnly), OrganisationId = OrganisationAId, AssignedAt = now });
        preparerB.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.PrivacyOfficer), OrganisationId = OrganisationBId, AssignedAt = now });

        db.Users.AddRange(superAdmin, preparerA, reviewerA, readOnlyA, preparerB);
        _createdUserIds.AddRange([superAdmin.Id, preparerA.Id, reviewerA.Id, readOnlyA.Id, preparerB.Id]);

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

        await db.DataFlows.IgnoreQueryFilters().Where(f => f.OrganisationId == OrganisationAId || f.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM processing_activity_data_categories WHERE processing_activity_id IN (SELECT id FROM processing_activities WHERE organisation_id = {0} OR organisation_id = {1})",
            OrganisationAId, OrganisationBId);
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM processing_activity_it_systems WHERE processing_activity_id IN (SELECT id FROM processing_activities WHERE organisation_id = {0} OR organisation_id = {1})",
            OrganisationAId, OrganisationBId);
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM processing_activity_data_collection_sources WHERE processing_activity_id IN (SELECT id FROM processing_activities WHERE organisation_id = {0} OR organisation_id = {1})",
            OrganisationAId, OrganisationBId);
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM processing_activity_recipients WHERE processing_activity_id IN (SELECT id FROM processing_activities WHERE organisation_id = {0} OR organisation_id = {1})",
            OrganisationAId, OrganisationBId);
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM processing_activity_processors WHERE processing_activity_id IN (SELECT id FROM processing_activities WHERE organisation_id = {0} OR organisation_id = {1})",
            OrganisationAId, OrganisationBId);
        await db.ProcessingActivities.IgnoreQueryFilters().Where(a => a.OrganisationId == OrganisationAId || a.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.DataInventoryItems.IgnoreQueryFilters().Where(i => i.OrganisationId == OrganisationAId || i.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.RetentionPolicies.IgnoreQueryFilters().Where(p => p.OrganisationId == OrganisationAId || p.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.Recipients.IgnoreQueryFilters().Where(r => r.OrganisationId == OrganisationAId || r.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.Processors.IgnoreQueryFilters().Where(p => p.OrganisationId == OrganisationAId || p.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.DataCollectionSources.IgnoreQueryFilters().Where(s => s.OrganisationId == OrganisationAId || s.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.ItSystems.IgnoreQueryFilters().Where(s => s.OrganisationId == OrganisationAId || s.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.DataCategories.IgnoreQueryFilters().Where(c => c.OrganisationId == OrganisationAId || c.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.AuditLogs.IgnoreQueryFilters().Where(a => a.OrganisationId == OrganisationAId || a.OrganisationId == OrganisationBId).ExecuteDeleteAsync();
        await db.Users.IgnoreQueryFilters().Where(u => _createdUserIds.Contains(u.Id)).ExecuteDeleteAsync();
        await db.Organisations.IgnoreQueryFilters().Where(o => o.Id == OrganisationAId || o.Id == OrganisationBId).ExecuteDeleteAsync();

        await _factory.DisposeAsync();
    }
}
