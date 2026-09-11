using System.Net;
using System.Net.Http.Json;
using DPDP.ApiTests.Identity;
using Xunit;

namespace DPDP.ApiTests.Organisations;

[Collection("Identity")]
public sealed class OrganisationManagementApiTests(IdentityApiFixture fixture)
{
    [Fact]
    public async Task Organisation_A_admin_can_read_and_update_their_own_profile()
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, fixture.OrganisationAAdminEmail);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var getResponse = await client.GetAsync($"/api/v1/organisations/{fixture.OrganisationAId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/organisations/{fixture.OrganisationAId}", new
        {
            name = "Updated Org A Name",
            industry = "Software",
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var body = await updateResponse.Content.ReadAsStringAsync();
        Assert.Contains("Updated Org A Name", body);
        Assert.Contains("Software", body);
    }

    [Fact]
    public async Task Organisation_A_admin_cannot_read_organisation_Bs_profile()
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, fixture.OrganisationAAdminEmail);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.GetAsync($"/api/v1/organisations/{fixture.OrganisationBId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Only_a_super_administrator_can_list_every_organisation()
    {
        var client = fixture.CreateClient();
        var orgAdminToken = await fixture.LoginAsync(client, fixture.OrganisationAAdminEmail);
        client.DefaultRequestHeaders.Authorization = new("Bearer", orgAdminToken);

        var response = await client.GetAsync("/api/v1/organisations");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Business_unit_and_department_crud_works_and_is_tenant_isolated()
    {
        var clientA = fixture.CreateClient();
        var tokenA = await fixture.LoginAsync(clientA, fixture.OrganisationAAdminEmail);
        clientA.DefaultRequestHeaders.Authorization = new("Bearer", tokenA);

        // Create a business unit in Org A.
        var createBuResponse = await clientA.PostAsJsonAsync("/api/v1/business-units", new
        {
            organisationId = fixture.OrganisationAId,
            name = $"Engineering-{Guid.NewGuid():N}",
        });
        Assert.Equal(HttpStatusCode.Created, createBuResponse.StatusCode);
        var businessUnit = await createBuResponse.Content.ReadFromJsonAsync<BusinessUnitResponse>();

        // Create a department under it.
        var createDeptResponse = await clientA.PostAsJsonAsync("/api/v1/departments", new
        {
            businessUnitId = businessUnit!.Id,
            name = $"Backend-{Guid.NewGuid():N}",
        });
        Assert.Equal(HttpStatusCode.Created, createDeptResponse.StatusCode);
        var department = await createDeptResponse.Content.ReadFromJsonAsync<DepartmentResponse>();

        // Org B's admin cannot see either.
        var clientB = fixture.CreateClient();
        var tokenB = await fixture.LoginAsync(clientB, fixture.OrganisationBAdminEmail);
        clientB.DefaultRequestHeaders.Authorization = new("Bearer", tokenB);

        var buFromB = await clientB.GetAsync($"/api/v1/business-units/{businessUnit.Id}");
        Assert.Equal(HttpStatusCode.NotFound, buFromB.StatusCode);

        var deptFromB = await clientB.GetAsync($"/api/v1/departments/{department!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deptFromB.StatusCode);

        // Org B's admin cannot create a business unit in Org A.
        var crossTenantCreate = await clientB.PostAsJsonAsync("/api/v1/business-units", new
        {
            organisationId = fixture.OrganisationAId,
            name = "Intruder BU",
        });
        Assert.Equal(HttpStatusCode.Forbidden, crossTenantCreate.StatusCode);

        // Org B's admin cannot attach a department to Org A's business unit
        // (the business unit lookup itself is tenant-filtered, so this is 404).
        var crossTenantDept = await clientB.PostAsJsonAsync("/api/v1/departments", new
        {
            businessUnitId = businessUnit.Id,
            name = "Intruder Dept",
        });
        Assert.Equal(HttpStatusCode.NotFound, crossTenantDept.StatusCode);

        // Cannot delete a business unit that still has a department.
        var deleteBuWithDept = await clientA.DeleteAsync($"/api/v1/business-units/{businessUnit.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteBuWithDept.StatusCode);

        // Clean up in dependency order so the fixture's own teardown doesn't conflict.
        await clientA.DeleteAsync($"/api/v1/departments/{department.Id}");
        var deleteBuNowEmpty = await clientA.DeleteAsync($"/api/v1/business-units/{businessUnit.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteBuNowEmpty.StatusCode);
    }

    private sealed record BusinessUnitResponse(Guid Id);
    private sealed record DepartmentResponse(Guid Id);
}
