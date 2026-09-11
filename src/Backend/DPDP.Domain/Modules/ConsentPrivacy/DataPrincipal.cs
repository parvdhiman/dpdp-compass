using DPDP.Domain.Common;
using DPDP.Domain.Modules.DataInventory;

namespace DPDP.Domain.Modules.ConsentPrivacy;

/// <summary>
/// A pointer to a real person, not a duplicate personal-data record. Per
/// the brief's explicit "never store unnecessary raw personal data" and
/// "support external identity/reference IDs" — this entity deliberately
/// has no name/email/phone field. ExternalReferenceId is the
/// organisation's own identifier for this person (a customer ID, employee
/// ID, or similar token from a system this platform does not replace);
/// looking up their actual contact details is that system's job, not
/// this platform's. See docs/CONSENT_PRIVACY_OPERATIONS.md section 1.
/// </summary>
public sealed class DataPrincipal : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public string ExternalReferenceId { get; set; } = string.Empty;
    public DataSubjectCategory? ReferenceCategory { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
