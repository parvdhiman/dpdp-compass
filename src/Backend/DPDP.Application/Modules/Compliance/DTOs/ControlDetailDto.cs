namespace DPDP.Application.Modules.Compliance.DTOs;

public sealed record ControlDetailDto(
    Guid Id,
    string ControlId,
    string Name,
    string Description,
    string Objective,
    Guid ControlCategoryId,
    string CategoryName,
    string RiskLevel,
    string? ApplicableConditions,
    string? EvidenceRequirementsSummary,
    string? Guidance,
    string SourceReference,
    DateOnly? EffectiveDate,
    DateOnly? ReviewDate,
    int Version,
    string Status,
    string ReviewStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<MappedRequirementDto> MappedRequirements,
    IReadOnlyList<AssessmentQuestionDto> Questions);

public sealed record MappedRequirementDto(Guid RequirementId, string RequirementCode, string RequirementTitle, string LegalCitation, string? MappingNotes);
