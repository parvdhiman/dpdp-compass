using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Compliance;

/// <summary>A question used to assess whether an organisation satisfies a Control. OptionsJson holds a JSON string array for MULTIPLE_CHOICE/MULTI_SELECT.</summary>
public sealed class AssessmentQuestion : Entity, ISoftDeletable
{
    public Guid ControlId { get; set; }
    public Control Control { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? HelpText { get; set; }
    public QuestionType QuestionType { get; set; }
    public string? OptionsJson { get; set; }
    public bool IsRequired { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<EvidenceRequirement> EvidenceRequirements { get; set; } = new List<EvidenceRequirement>();
}
