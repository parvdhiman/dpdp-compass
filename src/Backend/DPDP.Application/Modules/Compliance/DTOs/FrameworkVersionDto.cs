namespace DPDP.Application.Modules.Compliance.DTOs;

public sealed record FrameworkVersionDto(
    Guid Id,
    Guid FrameworkId,
    string FrameworkName,
    string VersionLabel,
    string? OfficialCitation,
    DateOnly? PublicationDate,
    DateOnly? EffectiveDate,
    string? SourceUrl,
    string ReviewStatus,
    bool IsCurrent,
    string? ChangeSummary,
    IReadOnlyList<LegalReferenceDto> LegalReferences);
