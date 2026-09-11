using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.DataInventory;

public sealed class RetentionPolicy : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RetentionPeriodValue { get; set; }
    public RetentionPeriodUnit RetentionPeriodUnit { get; set; }
    public string? TriggerEvent { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
