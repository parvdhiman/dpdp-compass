using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Organisations;

/// <summary>
/// OrganisationId is denormalized from BusinessUnit (rather than derived
/// through the BusinessUnitId join every time) specifically so this entity
/// can implement ITenantScoped directly and get the same one-line global
/// query filter as BusinessUnit — see DpdpDbContext.OnModelCreating.
/// Application handlers must always set it from the loaded BusinessUnit,
/// never from client input, to keep the two in sync.
/// </summary>
public sealed class Department : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }

    public Guid BusinessUnitId { get; set; }
    public BusinessUnit BusinessUnit { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContactInfo Head { get; set; } = new();
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
