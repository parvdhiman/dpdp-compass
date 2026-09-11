namespace DPDP.Domain.Common;

/// <summary>
/// Adds the created/updated audit columns required by every table
/// per docs/DATABASE.md — never optional, never populated by triggers.
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
