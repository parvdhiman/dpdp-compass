using System.Net;
using Xunit;

namespace DPDP.ApiTests;

public class HealthEndpointsTests(DpdpApiFactory factory) : IClassFixture<DpdpApiFactory>
{
    [Fact]
    public async Task Health_returns_200_without_checking_dependencies()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_ready_returns_200_when_postgresql_is_reachable()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
