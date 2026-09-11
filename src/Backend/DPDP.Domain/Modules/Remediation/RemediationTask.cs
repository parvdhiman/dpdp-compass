using DPDP.Domain.Common;
using DPDP.Domain.Modules.Findings;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Remediation;

/// <summary>
/// EvidenceJson mirrors the same lightweight JSON-reference pattern
/// Module 5 established for AssessmentAnswer.EvidenceJson — no file
/// upload/storage yet (Evidence Management is a later module).
/// </summary>
public sealed class RemediationTask : AuditableEntity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid FindingId { get; set; }
    public Finding Finding { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? OwnerUserId { get; set; }
    public User? Owner { get; set; }

    public DateOnly? DueDate { get; set; }
    public RemediationStatus Status { get; set; } = RemediationStatus.OPEN;

    public string? EvidenceJson { get; set; }

    public Guid? VerifiedByUserId { get; set; }
    public User? VerifiedByUser { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }

    public ICollection<RemediationComment> Comments { get; set; } = new List<RemediationComment>();
}
