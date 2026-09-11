using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Identity;

/// <summary>
/// One row per login attempt, success or failure — backs failed-login
/// tracking/lockout and the audit trail. UserId/OrganisationId are null
/// when the attempted email doesn't match any account (never reveal that
/// distinction to the caller — see docs/SECURITY.md).
/// </summary>
public sealed class LoginHistory : Entity
{
    public Guid? UserId { get; set; }
    public Guid? OrganisationId { get; set; }
    public string AttemptedEmail { get; set; } = string.Empty;
    public bool Succeeded { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
