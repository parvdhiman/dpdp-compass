using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.ConsentPrivacy;

/// <summary>
/// The only source of a Data Principal Request's due date. Per the
/// brief's explicit "do not hard-code legal deadlines without verified
/// legal source/configuration" — no default policy is seeded, and no
/// literal day-count exists anywhere in code; an organisation must
/// configure its own policy, sourced from its own legal counsel's advice
/// on the applicable statutory or contractual deadline (once one is
/// authoritatively set — see docs/COMPLIANCE_CONTENT_GOVERNANCE.md's
/// precedent for the DPDP Rules, 2025 shell). RequestType null means
/// "applies to any request type with no more specific policy configured".
/// </summary>
public sealed class SlaPolicy : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DataPrincipalRequestType? RequestType { get; set; }
    public int ResponseDueDays { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
