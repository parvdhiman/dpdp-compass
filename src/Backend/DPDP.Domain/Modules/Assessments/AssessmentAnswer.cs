using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Assessments;

/// <summary>
/// The mutable response to one AssessmentControlQuestion — created
/// (Status = NOT_ASSESSED, empty) alongside its question at assessment
/// creation time, then updated in place by SaveAssessmentAnswerCommand as
/// the assessor makes progress ("save progress"), never replaced with a
/// new row (no answer history beyond what AuditLog already captures).
/// Field list matches MASTER_PROMPT section 10 exactly: comment, evidence,
/// reviewer, review date, confidence, risk, remediation — all optional.
/// EvidenceJson mirrors the OptionsJson pattern Module 4 already
/// established for AssessmentQuestion.Options.
/// </summary>
public sealed class AssessmentAnswer : AuditableEntity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid AssessmentControlQuestionId { get; set; }
    public AssessmentControlQuestion AssessmentControlQuestion { get; set; } = null!;

    public AnswerStatus Status { get; set; } = AnswerStatus.NOT_ASSESSED;

    /// <summary>Scalar answer value (YES_NO/TEXT/NUMBER/DATE/URL/FILE) — a plain string, interpreted per the question's QuestionType.</summary>
    public string? AnswerValue { get; set; }

    /// <summary>JSON string array — used only when the question's QuestionType is MULTI_SELECT.</summary>
    public string? AnswerValuesJson { get; set; }

    public string? Comment { get; set; }

    /// <summary>JSON array of evidence references: [{ requirementId, description, url }]. No file upload/storage yet — see docs/ARCHITECTURE.md Module 5 section; real Evidence Management is a later module.</summary>
    public string? EvidenceJson { get; set; }

    public Guid? ReviewerId { get; set; }
    public User? Reviewer { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewComment { get; set; }

    public ConfidenceLevel? Confidence { get; set; }
    public RiskLevel? AssessedRiskLevel { get; set; }
    public string? RemediationNotes { get; set; }
}
