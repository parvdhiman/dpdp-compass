using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Compliance;

public sealed class EvidenceRequirement : Entity, ISoftDeletable
{
    public Guid AssessmentQuestionId { get; set; }
    public AssessmentQuestion AssessmentQuestion { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; } = true;
    public string? AcceptableFormats { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
