using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DPDP.ApiTests.Identity;

[Collection("Identity")]
public sealed class AuthEndpointsTests(IdentityApiFixture fixture)
{
    [Fact]
    public async Task Login_with_correct_credentials_returns_tokens_and_never_the_password_hash()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = fixture.OrganisationAAdminEmail,
            password = IdentityApiFixture.DefaultPassword,
        });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("accessToken", body);
        Assert.Contains("refreshToken", body);
        Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(IdentityApiFixture.DefaultPassword, body);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401_with_a_generic_message()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = fixture.OrganisationAAdminEmail,
            password = "definitely-wrong",
        });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Invalid email or password", body);
    }

    [Fact]
    public async Task Login_for_an_unknown_email_returns_the_same_generic_message_as_a_wrong_password()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = $"no-such-user-{Guid.NewGuid():N}@test.local",
            password = "whatever-Pass1!",
        });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Invalid email or password", body);
    }

    [Fact]
    public async Task Refresh_rotates_the_token_and_rejects_reuse_of_the_old_one()
    {
        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = fixture.OrganisationBAdminEmail,
            password = IdentityApiFixture.DefaultPassword,
        });
        var tokens = await login.Content.ReadFromJsonAsync<TokenPair>();

        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = tokens!.RefreshToken });
        var newTokens = await refreshResponse.Content.ReadFromJsonAsync<TokenPair>();

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        Assert.NotEqual(tokens.RefreshToken, newTokens!.RefreshToken);

        var reuseResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = tokens.RefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = fixture.OrganisationAReadOnlyEmail,
            password = IdentityApiFixture.DefaultPassword,
        });
        var tokens = await login.Content.ReadFromJsonAsync<TokenPair>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens!.AccessToken);

        var logoutResponse = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = tokens.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshAfterLogout = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = tokens.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }

    [Fact]
    public async Task Forgot_password_returns_the_same_response_shape_whether_or_not_the_account_exists()
    {
        var client = fixture.CreateClient();

        var existing = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = fixture.OrganisationAAdminEmail });
        var nonExistent = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = $"nobody-{Guid.NewGuid():N}@test.local" });

        Assert.Equal(HttpStatusCode.OK, existing.StatusCode);
        Assert.Equal(HttpStatusCode.OK, nonExistent.StatusCode);
    }

    [Fact]
    public async Task A_role_lacking_the_permission_is_forbidden_from_creating_users()
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, fixture.OrganisationAReadOnlyEmail);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/users", new
        {
            organisationId = fixture.OrganisationAId,
            email = $"should-not-be-created-{Guid.NewGuid():N}@test.local",
            fullName = "Should Not Exist",
            password = IdentityApiFixture.DefaultPassword,
            roleIds = Array.Empty<Guid>(),
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record TokenPair(string AccessToken, string RefreshToken);
}
