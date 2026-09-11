using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Organisations;

/// <summary>
/// The tenant. Every tenant-scoped entity elsewhere in the domain carries
/// an OrganisationId referencing this table — see docs/ARCHITECTURE.md
/// section 4 and docs/DATABASE.md section 3.
/// </summary>
public sealed class Organisation : AuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public OrganisationStatus Status { get; set; } = OrganisationStatus.Active;

    public string? Industry { get; set; }
    public OrganisationSize? Size { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }

    /// <summary>General/primary point of contact for the organisation.</summary>
    public ContactInfo PrimaryContact { get; set; } = new();

    /// <summary>Contact for data-protection/privacy inquiries — may or may not be the DPO.</summary>
    public ContactInfo PrivacyContact { get; set; } = new();

    /// <summary>Data Protection Officer / Privacy Officer, where the organisation is required to (or chooses to) appoint one.</summary>
    public ContactInfo DpoContact { get; set; } = new();

    public ICollection<OrganisationLocation> Locations { get; set; } = new List<OrganisationLocation>();

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
