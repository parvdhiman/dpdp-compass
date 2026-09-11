namespace DPDP.Infrastructure.Security;

/// <summary>Bound from configuration section "Jwt". SigningKey comes from user-secrets/env — never appsettings.json.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "dpdp-compass";
    public string Audience { get; set; } = "dpdp-compass";
    public int AccessTokenMinutes { get; set; } = 15;
}
