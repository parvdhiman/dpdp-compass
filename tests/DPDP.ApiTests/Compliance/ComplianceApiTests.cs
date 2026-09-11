using System.Net;
using System.Net.Http.Json;
using DPDP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DPDP.ApiTests.Compliance;

/// <summary>
/// Module 4 quality-gate tests: framework versioning, control relationships,
/// control activation, tenant access and authorization — exercised at the
/// HTTP layer against the real seeded DPDP Act 2023 framework/control
/// library (see DpdpActSeedData). Reuses the Identity fixture's two
/// organisations and Super Administrator rather than seeding its own,
/// since the compliance library is global reference data, not tenant data.
///
/// Unlike tenant data, nothing here is scoped to an organisation the
/// Identity fixture cleans up, so any control/framework version this class
/// creates is hard-deleted in DisposeAsync — otherwise it leaks into the
/// shared dev database forever (see the Module 3 completion report for the
/// same lesson learned with orphaned BusinessUnits/Departments).
/// </summary>
[Collection("Identity")]
public sealed class ComplianceApiTests(DPDP.ApiTests.Identity.IdentityApiFixture fixture) : IAsyncLifetime
{
    private readonly List<Guid> _createdControlIds = [];
    private readonly List<Guid> _createdFrameworkVersionIds = [];

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_createdControlIds.Count == 0 && _createdFrameworkVersionIds.Count == 0)
        {
            return;
        }

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? throw new InvalidOperationException("Set ConnectionStrings__Default before running integration tests.");
        var options = new DbContextOptionsBuilder<DpdpDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var db = new DpdpDbContext(options, new DesignTimeCurrentUserContext());

        if (_createdControlIds.Count > 0)
        {
            await db.ControlMappings.Where(m => _createdControlIds.Contains(m.ControlId)).ExecuteDeleteAsync();
            await db.Controls.Where(c => _createdControlIds.Contains(c.Id)).ExecuteDeleteAsync();
        }

        if (_createdFrameworkVersionIds.Count > 0)
        {
            await db.FrameworkVersions.Where(v => _createdFrameworkVersionIds.Contains(v.Id)).ExecuteDeleteAsync();
        }
    }

    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Unauthenticated_request_is_rejected()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/v1/compliance/controls");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Seeded_DPDP_Act_framework_is_visible_to_any_authenticated_user()
    {
        var client = await AuthenticatedClientAsync(fixture.OrganisationAAdminEmail);

        var frameworks = await client.GetFromJsonAsync<List<FrameworkSummary>>("/api/v1/compliance/frameworks");

        Assert.NotNull(frameworks);
        Assert.Contains(frameworks!, f => f.Code == "DPDPA-2023");
    }

    [Fact]
    public async Task Organisation_admin_sees_only_ACTIVE_controls_by_default()
    {
        var client = await AuthenticatedClientAsync(fixture.OrganisationAAdminEmail);

        var response = await client.GetAsync("/api/v1/compliance/controls?pageSize=100");
        var body = await response.Content.ReadFromJsonAsync<PagedControls>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(body!.Items);
        Assert.All(body.Items, c => Assert.Equal("ACTIVE", c.Status));
    }

    [Fact]
    public async Task Seeded_control_exposes_its_mapped_requirement_and_questions_with_evidence()
    {
        var client = await AuthenticatedClientAsync(fixture.OrganisationAAdminEmail);

        var listResponse = await client.GetAsync("/api/v1/compliance/controls?search=DPDP-CTRL-001&pageSize=10");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedControls>();
        var summary = Assert.Single(list!.Items, c => c.ControlId == "DPDP-CTRL-001");

        var detailResponse = await client.GetAsync($"/api/v1/compliance/controls/{summary.Id}");
        var detail = await detailResponse.Content.ReadFromJsonAsync<ControlDetail>();

        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        Assert.NotEmpty(detail!.MappedRequirements);
        Assert.NotEmpty(detail.Questions);
        Assert.All(detail.MappedRequirements, r => Assert.False(string.IsNullOrWhiteSpace(r.LegalCitation)));
    }

    [Fact]
    public async Task Organisation_admin_cannot_create_or_modify_the_shared_control_library()
    {
        var client = await AuthenticatedClientAsync(fixture.OrganisationAAdminEmail);
        var categories = await client.GetFromJsonAsync<List<CategorySummary>>("/api/v1/compliance/control-categories");
        var categoryId = categories!.First().Id;

        var response = await client.PostAsJsonAsync("/api/v1/compliance/controls", new
        {
            controlId = $"TEST-{Guid.NewGuid():N}"[..20],
            name = "Intruder control",
            description = "Should never be created by a non-super-administrator.",
            objective = "n/a",
            controlCategoryId = categoryId,
            riskLevel = "LOW",
            applicableConditions = (string?)null,
            evidenceRequirementsSummary = (string?)null,
            guidance = (string?)null,
            sourceReference = "n/a",
            effectiveDate = (DateOnly?)null,
            reviewDate = (DateOnly?)null,
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Super_administrator_can_create_activate_and_retire_a_control_and_visibility_follows_status()
    {
        var superClient = await AuthenticatedClientAsync(fixture.SuperAdministratorEmail);
        var orgClient = await AuthenticatedClientAsync(fixture.OrganisationAAdminEmail);

        var categories = await superClient.GetFromJsonAsync<List<CategorySummary>>("/api/v1/compliance/control-categories");
        var categoryId = categories!.First().Id;
        var controlBusinessId = $"TEST-CTRL-{Guid.NewGuid():N}"[..24];

        var createResponse = await superClient.PostAsJsonAsync("/api/v1/compliance/controls", new
        {
            controlId = controlBusinessId,
            name = "Temporary test control",
            description = "Created by an automated test to verify the activation/retirement lifecycle.",
            objective = "Verify lifecycle transitions.",
            controlCategoryId = categoryId,
            riskLevel = "LOW",
            applicableConditions = (string?)null,
            evidenceRequirementsSummary = (string?)null,
            guidance = (string?)null,
            sourceReference = "Automated test fixture — not a real legal source.",
            effectiveDate = (DateOnly?)null,
            reviewDate = (DateOnly?)null,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ControlDetail>();
        Assert.Equal("DRAFT", created!.Status);
        _createdControlIds.Add(created.Id);

        // A new DRAFT control is invisible to a non-super-administrator, even as Super Admin sees it.
        var hiddenFromOrg = await orgClient.GetAsync($"/api/v1/compliance/controls/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenFromOrg.StatusCode);

        var activateResponse = await superClient.PostAsync($"/api/v1/compliance/controls/{created.Id}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        // Activating twice is rejected as a conflicting state transition.
        var reactivateResponse = await superClient.PostAsync($"/api/v1/compliance/controls/{created.Id}/activate", null);
        Assert.Equal(HttpStatusCode.Conflict, reactivateResponse.StatusCode);

        var visibleToOrg = await orgClient.GetAsync($"/api/v1/compliance/controls/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, visibleToOrg.StatusCode);

        var retireResponse = await superClient.PostAsync($"/api/v1/compliance/controls/{created.Id}/retire", null);
        Assert.Equal(HttpStatusCode.NoContent, retireResponse.StatusCode);

        var hiddenAgain = await orgClient.GetAsync($"/api/v1/compliance/controls/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenAgain.StatusCode);

        // Retired is still visible to a Super Administrator (never deleted).
        var stillVisibleToSuper = await superClient.GetAsync($"/api/v1/compliance/controls/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, stillVisibleToSuper.StatusCode);
    }

    [Fact]
    public async Task Super_administrator_can_version_a_framework_and_activation_moves_the_current_flag()
    {
        var superClient = await AuthenticatedClientAsync(fixture.SuperAdministratorEmail);

        var frameworks = await superClient.GetFromJsonAsync<List<FrameworkSummary>>("/api/v1/compliance/frameworks");
        var dpdpAct = frameworks!.Single(f => f.Code == "DPDPA-2023");
        var originalVersionId = dpdpAct.Versions.Single(v => v.IsCurrent).Id;

        var versionLabel = $"test-{Guid.NewGuid():N}"[..16];
        var createVersionResponse = await superClient.PostAsJsonAsync($"/api/v1/compliance/frameworks/{dpdpAct.Id}/versions", new
        {
            versionLabel,
            officialCitation = (string?)null,
            publicationDate = (DateOnly?)null,
            effectiveDate = (DateOnly?)null,
            sourceUrl = (string?)null,
            changeSummary = "Automated test version — not a real amendment.",
        });
        Assert.Equal(HttpStatusCode.Created, createVersionResponse.StatusCode);
        var newVersion = await createVersionResponse.Content.ReadFromJsonAsync<FrameworkVersionDetail>();
        Assert.False(newVersion!.IsCurrent);
        _createdFrameworkVersionIds.Add(newVersion.Id);

        var activateResponse = await superClient.PostAsync($"/api/v1/compliance/framework-versions/{newVersion.Id}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        var refreshedNewVersion = await superClient.GetFromJsonAsync<FrameworkVersionDetail>($"/api/v1/compliance/framework-versions/{newVersion.Id}");
        var refreshedOriginalVersion = await superClient.GetFromJsonAsync<FrameworkVersionDetail>($"/api/v1/compliance/framework-versions/{originalVersionId}");

        Assert.True(refreshedNewVersion!.IsCurrent);
        Assert.False(refreshedOriginalVersion!.IsCurrent);

        // Restore shared global state: leaving the seeded DPDP Act framework
        // without a current version (or with a since-deleted one as current)
        // would break every other test/user relying on it after this test's
        // row is hard-deleted in DisposeAsync.
        var restoreResponse = await superClient.PostAsync($"/api/v1/compliance/framework-versions/{originalVersionId}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);
    }

    private sealed record FrameworkSummary(Guid Id, string Name, string Code, List<FrameworkVersionSummary> Versions);
    private sealed record FrameworkVersionSummary(Guid Id, bool IsCurrent);
    private sealed record FrameworkVersionDetail(Guid Id, bool IsCurrent);
    private sealed record CategorySummary(Guid Id, string Name);
    private sealed record PagedControls(List<ControlSummary> Items);
    private sealed record ControlSummary(Guid Id, string ControlId, string Status);
    private sealed record ControlDetail(Guid Id, string Status, List<MappedRequirement> MappedRequirements, List<object> Questions);
    private sealed record MappedRequirement(string LegalCitation);
}
