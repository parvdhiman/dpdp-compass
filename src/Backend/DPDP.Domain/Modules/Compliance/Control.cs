using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Compliance;

/// <summary>
/// The full field list is per the Module 4 brief. ControlId is the
/// human-facing business key (e.g. "DPDP-CTRL-001"), distinct from Id.
/// Requirement linkage is many-to-many via ControlMapping, not a direct
/// FK — a control can satisfy more than one requirement and vice versa.
/// </summary>
public sealed class Control : AuditableEntity
{
    public string ControlId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;

    public Guid ControlCategoryId { get; set; }
    public ControlCategory ControlCategory { get; set; } = null!;

    public RiskLevel RiskLevel { get; set; }
    public string? ApplicableConditions { get; set; }
    public string? EvidenceRequirementsSummary { get; set; }
    public string? Guidance { get; set; }
    public string SourceReference { get; set; } = string.Empty;
    public DateOnly? EffectiveDate { get; set; }
    public DateOnly? ReviewDate { get; set; }
    public int Version { get; set; } = 1;
    public ControlStatus Status { get; set; } = ControlStatus.DRAFT;
    public ContentReviewStatus ReviewStatus { get; set; } = ContentReviewStatus.DRAFT;

    public ICollection<ControlMapping> ControlMappings { get; set; } = new List<ControlMapping>();
    public ICollection<AssessmentQuestion> Questions { get; set; } = new List<AssessmentQuestion>();
}
