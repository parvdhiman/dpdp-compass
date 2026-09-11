namespace DPDP.Application.Modules.Compliance.DTOs;

public sealed record AssessmentQuestionDto(
    Guid Id,
    Guid ControlId,
    string ControlBusinessId,
    string ControlName,
    string Code,
    string Text,
    string? HelpText,
    string QuestionType,
    IReadOnlyList<string>? Options,
    bool IsRequired,
    int SortOrder,
    IReadOnlyList<EvidenceRequirementDto> EvidenceRequirements);
