using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Identity;

/// <summary>
/// Assigns a role to a user, scoped to an organisation. OrganisationId is
/// null only for a Super Administrator's global role assignment — a
/// surrogate Id is used (rather than a composite key including
/// OrganisationId) because PostgreSQL primary key columns cannot be
/// nullable; uniqueness is instead enforced by two partial unique indexes
/// in UserRoleConfiguration (one for organisation-scoped rows, one for the
/// null-organisation/global case).
/// </summary>
public sealed class UserRole : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid? OrganisationId { get; set; }

    public DateTimeOffset AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; }
}
