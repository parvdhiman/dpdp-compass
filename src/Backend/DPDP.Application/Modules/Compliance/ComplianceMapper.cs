using System.Text.Json;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;

namespace DPDP.Application.Modules.Compliance;

internal static class ComplianceMapper
{
    public static FrameworkVersionSummaryDto ToSummaryDto(FrameworkVersion version) => new(
        version.Id, version.VersionLabel, version.IsCurrent, version.ReviewStatus.ToString(),
        version.PublicationDate, version.EffectiveDate);

    public static FrameworkDto ToDto(Framework framework) => new(
        framework.Id, framework.Name, framework.Code, framework.Jurisdiction, framework.IssuingAuthority,
        framework.Description, framework.Versions.Select(ToSummaryDto).ToList());

    public static LegalReferenceDto ToDto(LegalReference legalReference) => new(
        legalReference.Id, legalReference.FrameworkVersionId, legalReference.Citation, legalReference.Title,
        legalReference.Chapter, legalReference.SummaryText, legalReference.SourceCitation,
        legalReference.ReviewStatus.ToString(), legalReference.Requirements.Count);

    public static FrameworkVersionDto ToDetailDto(FrameworkVersion version) => new(
        version.Id, version.FrameworkId, version.Framework.Name, version.VersionLabel, version.OfficialCitation,
        version.PublicationDate, version.EffectiveDate, version.SourceUrl, version.ReviewStatus.ToString(),
        version.IsCurrent, version.ChangeSummary, version.LegalReferences.Select(ToDto).ToList());

    public static ControlCategoryDto ToDto(ControlCategory category, int controlCount) => new(
        category.Id, category.Name, category.Description, category.SortOrder, controlCount);

    public static ControlSummaryDto ToSummaryDto(Control control) => new(
        control.Id, control.ControlId, control.Name, control.ControlCategory.Name, control.RiskLevel.ToString(),
        control.Status.ToString(), control.ReviewStatus.ToString(), control.Version, control.SourceReference,
        control.Questions.Count);

    public static EvidenceRequirementDto ToDto(EvidenceRequirement evidence) => new(
        evidence.Id, evidence.AssessmentQuestionId, evidence.AssessmentQuestion?.Code ?? string.Empty,
        evidence.Name, evidence.Description, evidence.IsMandatory, evidence.AcceptableFormats);

    public static AssessmentQuestionDto ToDto(AssessmentQuestion question) => new(
        question.Id, question.ControlId, question.Control?.ControlId ?? string.Empty, question.Control?.Name ?? string.Empty,
        question.Code, question.Text, question.HelpText, question.QuestionType.ToString(),
        question.OptionsJson is null ? null : JsonSerializer.Deserialize<List<string>>(question.OptionsJson),
        question.IsRequired, question.SortOrder,
        question.EvidenceRequirements.Select(ToDto).ToList());

    public static ControlDetailDto ToDetailDto(Control control) => new(
        control.Id, control.ControlId, control.Name, control.Description, control.Objective,
        control.ControlCategoryId, control.ControlCategory.Name, control.RiskLevel.ToString(),
        control.ApplicableConditions, control.EvidenceRequirementsSummary, control.Guidance,
        control.SourceReference, control.EffectiveDate, control.ReviewDate, control.Version,
        control.Status.ToString(), control.ReviewStatus.ToString(), control.CreatedAt, control.UpdatedAt,
        control.ControlMappings.Select(m => new MappedRequirementDto(
            m.RequirementId, m.Requirement.Code, m.Requirement.Title, m.Requirement.LegalReference.Citation, m.MappingNotes)).ToList(),
        control.Questions.Select(ToDto).ToList());
}
