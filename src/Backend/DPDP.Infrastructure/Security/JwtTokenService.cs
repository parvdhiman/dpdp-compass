using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DPDP.Infrastructure.Security;

/// <summary>
/// Claim shape: sub, org_id (omitted if null), is_super_admin, one
/// "permission" claim per permission key, one "role" claim per role name
/// (display only — authorization must always check permission claims, per
/// docs/ARCHITECTURE.md section 5). Never logs the token value itself.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> jwtOptions, IDateTimeProvider dateTimeProvider)
    : IJwtTokenService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public AccessTokenResult GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> roleNames,
        IReadOnlyCollection<string> permissions,
        bool isSuperAdministrator)
    {
        var now = dateTimeProvider.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.FullName),
            new("is_super_admin", isSuperAdministrator ? "true" : "false"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (user.OrganisationId is { } organisationId)
        {
            claims.Add(new Claim("org_id", organisationId.ToString()));
        }

        claims.AddRange(roleNames.Select(name => new Claim(ClaimTypes.Role, name)));
        claims.AddRange(permissions.Select(key => new Claim("permission", key)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        return new AccessTokenResult(handler.WriteToken(token), expiresAt);
    }
}
