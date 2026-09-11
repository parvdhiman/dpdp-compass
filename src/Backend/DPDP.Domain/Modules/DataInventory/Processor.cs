using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.DataInventory;

/// <summary>
/// A lightweight third-party data processor record — ahead of the
/// dedicated Processor Management module (roadmap item 29, which depends
/// on Vendor Management, item 28, neither built yet). This is a real,
/// independently manageable entity (not a free-text placeholder) because
/// the brief's relationship diagram requires Processing Activities and
/// Data Inventory items to link to it structurally; when the dedicated
/// module ships, this table is the natural migration target, not a
/// throwaway.
/// </summary>
public sealed class Processor : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
