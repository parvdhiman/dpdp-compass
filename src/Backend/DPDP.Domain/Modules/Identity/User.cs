using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Identity;

/// <summary>
/// OrganisationId is null only for Super Administrator accounts — every
/// other user belongs to exactly one organisation. See docs/DATABASE.md
/// section 3. Tenant isolation for this entity is enforced by a global
/// query filter in DpdpDbContext, not by a plain ITenantScoped marker,
/// precisely because OrganisationId is nullable here.
/// </summary>
public sealed class User : AuditableEntity, ISoftDeletable
{
    public Guid? OrganisationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Set true when an administrator creates or resets an account on the user's behalf.</summary>
    public bool MustChangePassword { get; set; }

    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }

    public int FailedLoginCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
