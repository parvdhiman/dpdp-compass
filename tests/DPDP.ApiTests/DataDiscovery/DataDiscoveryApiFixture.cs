using System.Net.Http.Json;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using DPDP.Infrastructure.Persistence;
using DPDP.Infrastructure.Persistence.Configurations.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace DPDP.ApiTests.DataDiscovery;

/// <summary>
/// Seeds two isolated organisations. Org A gets an Operator (IT
/// Administrator — holds datasources.manage/discoveryjobs.manage), a
/// Reviewer (Compliance Officer — holds classification.review but NOT
/// datasources.manage, mirroring the separation Module 7 established
/// between upload and review), and a Read Only user. Org B gets its own
/// Operator for tenant-isolation tests. Plus a Super Administrator.
///
/// Also creates a dedicated Postgres schema in the SAME database this
/// test host already talks to, seeded with a couple of tables containing
/// obviously-personal-data-shaped columns, so PostgresDiscoveryConnector
/// can be exercised against a real Postgres server end-to-end without
/// standing up a second database — see docs/DATA_DISCOVERY.md and the
/// completion report for why MySQL/SqlServer connectors are unit-tested
/// only (no such server exists in this environment).
/// </summary>
public sealed class DataDiscoveryApiFixture : IAsyncLifetime
{
    public const string DefaultPassword = "Nb4#Compass!Vault7";

    private readonly DpdpApiFactory _factory = new();

    public Guid OrganisationAId { get; } = Guid.NewGuid();
    public Guid OrganisationBId { get; } = Guid.NewGuid();
    public string TestSchemaName { get; private set; } = string.Empty;

    public string SuperAdministratorEmail { get; private set; } = string.Empty;
    public string OperatorAEmail { get; private set; } = string.Empty;
    public string ReviewerAEmail { get; private set; } = string.Empty;
    public string ReadOnlyAEmail { get; private set; } = string.Empty;
    public string OperatorBEmail { get; private set; } = string.Empty;

    private readonly List<Guid> _createdUserIds = [];
    private NpgsqlConnectionStringBuilder _appConnectionStringBuilder = null!;

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task InitializeAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        SuperAdministratorEmail = $"test-disc-superadmin-{suffix}@test.local";
        OperatorAEmail = $"test-disc-operator-a-{suffix}@test.local";
        ReviewerAEmail = $"test-disc-reviewer-a-{suffix}@test.local";
        ReadOnlyAEmail = $"test-disc-ro-a-{suffix}@test.local";
        OperatorBEmail = $"test-disc-operator-b-{suffix}@test.local";
        TestSchemaName = $"discovery_test_{suffix}";

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        _appConnectionStringBuilder = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("Default"));

        var now = DateTimeOffset.UtcNow;
        var passwordHash = passwordHasher.HashPassword(DefaultPassword);

        db.Organisations.AddRange(
            new Organisation { Id = OrganisationAId, Name = $"DiscTest-Org-A-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Organisation { Id = OrganisationBId, Name = $"DiscTest-Org-B-{suffix}", Status = OrganisationStatus.Active, CreatedAt = now, UpdatedAt = now });

        var superAdmin = NewUser(null, SuperAdministratorEmail, "Test Super Administrator", passwordHash, now);
        var operatorA = NewUser(OrganisationAId, OperatorAEmail, "Test Operator A", passwordHash, now);
        var reviewerA = NewUser(OrganisationAId, ReviewerAEmail, "Test Reviewer A", passwordHash, now);
        var readOnlyA = NewUser(OrganisationAId, ReadOnlyAEmail, "Test Read Only A", passwordHash, now);
        var operatorB = NewUser(OrganisationBId, OperatorBEmail, "Test Operator B", passwordHash, now);

        superAdmin.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.SuperAdministrator), OrganisationId = null, AssignedAt = now });
        operatorA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ItAdministrator), OrganisationId = OrganisationAId, AssignedAt = now });
        reviewerA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ComplianceOfficer), OrganisationId = OrganisationAId, AssignedAt = now });
        readOnlyA.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ReadOnly), OrganisationId = OrganisationAId, AssignedAt = now });
        operatorB.UserRoles.Add(new UserRole { RoleId = RoleConfiguration.RoleId(RoleNames.ItAdministrator), OrganisationId = OrganisationBId, AssignedAt = now });

        db.Users.AddRange(superAdmin, operatorA, reviewerA, readOnlyA, operatorB);
        _createdUserIds.AddRange([superAdmin.Id, operatorA.Id, reviewerA.Id, readOnlyA.Id, operatorB.Id]);

        await db.SaveChangesAsync(CancellationToken.None);

        var dpdpDb = (DpdpDbContext)db;
        // TestSchemaName is a GUID-derived string generated above, never
        // user input — identifiers like schema/table names can't be sent
        // as SQL parameters anyway, so ExecuteSqlRawAsync is correct here.
#pragma warning disable EF1002
        await dpdpDb.Database.ExecuteSqlRawAsync($"""
            CREATE SCHEMA "{TestSchemaName}";
            CREATE TABLE "{TestSchemaName}".customers (
                id serial PRIMARY KEY,
                customer_email varchar(200) NOT NULL,
                national_id varchar(20),
                notes text
            );
            INSERT INTO "{TestSchemaName}".customers (customer_email, national_id, notes)
            VALUES ('jane.doe@example.com', '9876543210', 'a regular customer');
            CREATE INDEX ix_customers_email ON "{TestSchemaName}".customers (customer_email);
            """);
#pragma warning restore EF1002
    }

    public NpgsqlConnectionStringBuilder AppConnectionStringBuilder => _appConnectionStringBuilder;

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

#pragma warning disable EF1002 // TestSchemaName is our own GUID-derived string — see InitializeAsync.
        await db.Database.ExecuteSqlRawAsync($"""DROP SCHEMA IF EXISTS "{TestSchemaName}" CASCADE""");
#pragma warning restore EF1002

        await db.DataElements.IgnoreQueryFilters()
            .Where(e => e.OrganisationId == OrganisationAId || e.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.DiscoveryResults.IgnoreQueryFilters()
            .Where(r => r.OrganisationId == OrganisationAId || r.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.DataAssets.IgnoreQueryFilters()
            .Where(a => a.OrganisationId == OrganisationAId || a.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.DiscoveryJobs.IgnoreQueryFilters()
            .Where(j => j.OrganisationId == OrganisationAId || j.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.DataSources.IgnoreQueryFilters()
            .Where(s => s.OrganisationId == OrganisationAId || s.OrganisationId == OrganisationBId)
            .ExecuteDeleteAsync();
        await db.Users.IgnoreQueryFilters().Where(u => _createdUserIds.Contains(u.Id)).ExecuteDeleteAsync();
        await db.Organisations.IgnoreQueryFilters()
            .Where(o => o.Id == OrganisationAId || o.Id == OrganisationBId)
            .ExecuteDeleteAsync();

        await _factory.DisposeAsync();
    }
}
