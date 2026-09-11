using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Organisations;

public sealed class BusinessUnit : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContactInfo Head { get; set; } = new();
    public bool IsActive { get; set; } = true;

    public ICollection<Department> Departments { get; set; } = new List<Department>();

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
