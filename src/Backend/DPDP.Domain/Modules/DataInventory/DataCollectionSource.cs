using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.DataInventory;

/// <summary>
/// The brief's "Data Source" (where personal data is originally
/// collected from, e.g. a web form or a phone call) — renamed to avoid
/// colliding with Module 8's DataSource, which is an unrelated concept
/// (a technical connection to a customer database/filesystem for
/// discovery scans). See docs/ARCHITECTURE.md Module 9 section.
/// </summary>
public sealed class DataCollectionSource : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DataCollectionSourceType SourceType { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
