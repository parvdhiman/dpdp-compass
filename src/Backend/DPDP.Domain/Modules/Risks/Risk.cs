using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Risks;

/// <summary>
/// "Risk model: Likelihood, Impact, Data Sensitivity, Exposure" per the
/// Module 6 brief. DataSensitivity/Exposure/CalculatedRiskLevel reuse
/// Compliance.RiskLevel (LOW/MEDIUM/HIGH/CRITICAL) rather than three more
/// near-identical four-point enums — see docs/ARCHITECTURE.md Module 6
/// section. CalculatedRiskLevel/CalculatedRiskScore are recomputed by
/// IRiskScoringStrategy (docs/RISK_METHODOLOGY.md) every time any of the
/// four input dimensions changes, and persisted here — not computed lazily
/// on every read — the same "calculate on write, not on read" precedent
/// AssessmentControl.Status set in Module 5.
/// </summary>
public sealed class Risk : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public int SequenceNumber { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Likelihood Likelihood { get; set; }
    public Impact Impact { get; set; }
    public RiskLevel DataSensitivity { get; set; }
    public RiskLevel Exposure { get; set; }

    public RiskLevel CalculatedRiskLevel { get; set; }
    public double CalculatedRiskScore { get; set; }

    public Guid? OwnerUserId { get; set; }
    public User? Owner { get; set; }

    public RiskStatus Status { get; set; } = RiskStatus.OPEN;
    public string? TreatmentPlan { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<Findings.Finding> Findings { get; set; } = new List<Findings.Finding>();
}
