using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DPDP.ApiTests.Identity;

/// <summary>
/// The Module 2 brief's explicit requirement, exercised at the HTTP layer
/// (defense in depth alongside DPDP.IntegrationTests.Identity.TenantIsolationTests,
/// which proves the same thing at the query-filter layer directly).
/// </summary>
[Collection("Identity")]
public sealed class UsersTenantIsolationApiTests(IdentityApiFixture fixture)
{
    [Fact]
    public async Task Organisation_A_admin_cannot_read_organisation_Bs_user_list()
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, fixture.OrganisationAAdminEmail);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.GetAsync("/api/v1/users");
        var body = await response.Content.ReadFromJsonAsync<PagedUsers>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(body!.Items, u => u.Id == fixture.OrganisationBAdminUserId);
        Assert.Contains(body.Items, u => u.Id == fixture.OrganisationAAdminUserId);
    }

    [Fact]
    public async Task Organisation_A_admin_cannot_fetch_organisation_Bs_user_by_id()
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, fixture.OrganisationAAdminEmail);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.GetAsync($"/api/v1/users/{fixture.OrganisationBAdminUserId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Organisation_A_admin_cannot_create_a_user_in_organisation_B()
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, fixture.OrganisationAAdminEmail);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/users", new
        {
            organisationId = fixture.OrganisationBId,
            email = $"intruder-{Guid.NewGuid():N}@test.local",
            fullName = "Intruder",
            password = IdentityApiFixture.DefaultPassword,
            roleIds = Array.Empty<Guid>(),
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_request_is_rejected()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_super_administrator_can_read_users_from_both_organisations()
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, fixture.SuperAdministratorEmail);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var responseA = await client.GetAsync($"/api/v1/users/{fixture.OrganisationAAdminUserId}");
        var responseB = await client.GetAsync($"/api/v1/users/{fixture.OrganisationBAdminUserId}");

        Assert.Equal(HttpStatusCode.OK, responseA.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responseB.StatusCode);
    }

    private sealed record PagedUsers(List<UserSummary> Items);
    private sealed record UserSummary(Guid Id);
}
