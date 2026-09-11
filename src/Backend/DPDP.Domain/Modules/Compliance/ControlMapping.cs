using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Compliance;

/// <summary>Many-to-many join between Control and Requirement — a control can satisfy several requirements and a requirement can be satisfied by several controls.</summary>
public sealed class ControlMapping : Entity
{
    public Guid ControlId { get; set; }
    public Control Control { get; set; } = null!;

    public Guid RequirementId { get; set; }
    public Requirement Requirement { get; set; } = null!;

    public string? MappingNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
