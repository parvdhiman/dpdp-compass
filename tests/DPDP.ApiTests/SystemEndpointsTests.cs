using System.Net;
using System.Text.Json;
using Xunit;

namespace DPDP.ApiTests;

public class SystemEndpointsTests(DpdpApiFactory factory) : IClassFixture<DpdpApiFactory>
{
    [Fact]
    public async Task GetSystemInfo_returns_version_and_environment()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/system/info");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(body);
        Assert.Equal("DPDP-COMPASS", json.RootElement.GetProperty("applicationName").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("version").GetString()));
        Assert.Equal("Development", json.RootElement.GetProperty("environment").GetString());
        Assert.True(json.RootElement.TryGetProperty("serverTimeUtc", out _));
    }

    [Fact]
    public async Task GetSystemInfo_never_exposes_secrets_or_connection_details()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/system/info");
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connectionstring", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Host=", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Every_response_carries_a_correlation_id()
    {
        var client = factory.CreateClient();
        var requestCorrelationId = Guid.NewGuid().ToString();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/system/info");
        request.Headers.Add("X-Correlation-Id", requestCorrelationId);

        var response = await client.SendAsync(request);

        Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var values));
        Assert.Equal(requestCorrelationId, values!.Single());
    }

    [Fact]
    public async Task Unknown_route_returns_problem_details_with_correlation_id()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/does-not-exist");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(body);
        Assert.True(json.RootElement.TryGetProperty("correlationId", out var correlationId));
        Assert.False(string.IsNullOrWhiteSpace(correlationId.GetString()));
    }
}
