namespace DPDP.Application.Modules.Compliance.DTOs;

public sealed record FrameworkDto(
    Guid Id,
    string Name,
    string Code,
    string Jurisdiction,
    string IssuingAuthority,
    string? Description,
    IReadOnlyList<FrameworkVersionSummaryDto> Versions);

public sealed record FrameworkVersionSummaryDto(
    Guid Id,
    string VersionLabel,
    bool IsCurrent,
    string ReviewStatus,
    DateOnly? PublicationDate,
    DateOnly? EffectiveDate);
