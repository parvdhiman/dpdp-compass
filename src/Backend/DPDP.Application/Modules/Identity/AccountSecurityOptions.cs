namespace DPDP.Application.Modules.Identity;

/// <summary>Bound from configuration section "AccountSecurity" — see appsettings.json.</summary>
public sealed class AccountSecurityOptions
{
    public const string SectionName = "AccountSecurity";

    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
    public int PasswordResetTokenMinutes { get; set; } = 30;
}
