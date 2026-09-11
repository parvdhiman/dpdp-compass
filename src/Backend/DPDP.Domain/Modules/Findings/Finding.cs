using DPDP.Domain.Common;
using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Risks;

namespace DPDP.Domain.Modules.Findings;

/// <summary>
/// "Finding ID" (the brief's field) is a human-facing display value —
/// FIND-{SequenceNumber:D5} — computed from SequenceNumber, a
/// database-generated identity column (atomic, no race condition under
/// concurrent creation), never stored as a formatted string. See
/// docs/ARCHITECTURE.md Module 6 section.
///
/// "Asset" (the brief's field) has no real FK yet — Asset Inventory is a
/// later, not-yet-built module (roadmap item 16) — so AssetReference is a
/// free-text placeholder until that module ships a real Asset entity to
/// point at.
/// </summary>
public sealed class Finding : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public int SequenceNumber { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FindingSource Source { get; set; } = FindingSource.MANUAL;

    public Guid? AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    public Guid? AssessmentControlId { get; set; }
    public AssessmentControl? AssessmentControl { get; set; }

    public Guid? ControlId { get; set; }
    public Control? Control { get; set; }

    public string? AssetReference { get; set; }

    public Guid? RiskId { get; set; }
    public Risk? Risk { get; set; }

    public FindingSeverity Severity { get; set; }

    public Guid? OwnerUserId { get; set; }
    public User? Owner { get; set; }

    public DateOnly? DueDate { get; set; }
    public FindingStatus Status { get; set; } = FindingStatus.OPEN;
    public string? Recommendation { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<Remediation.RemediationTask> RemediationTasks { get; set; } = new List<Remediation.RemediationTask>();
}
