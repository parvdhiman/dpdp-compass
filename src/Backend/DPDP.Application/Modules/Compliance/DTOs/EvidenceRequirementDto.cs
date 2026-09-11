namespace DPDP.Application.Modules.Compliance.DTOs;

public sealed record EvidenceRequirementDto(
    Guid Id,
    Guid AssessmentQuestionId,
    string QuestionCode,
    string Name,
    string? Description,
    bool IsMandatory,
    string? AcceptableFormats);
