namespace DPDP.Application.Modules.Compliance.DTOs;

public sealed record LegalReferenceDto(
    Guid Id,
    Guid FrameworkVersionId,
    string Citation,
    string Title,
    string? Chapter,
    string? SummaryText,
    string SourceCitation,
    string ReviewStatus,
    int RequirementCount);
