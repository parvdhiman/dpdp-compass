namespace DPDP.Application.Modules.Compliance.DTOs;

public sealed record RequirementDto(
    Guid Id,
    Guid LegalReferenceId,
    string LegalReferenceCitation,
    string SourceCitation,
    string Code,
    string Title,
    string Description,
    string ReviewStatus,
    IReadOnlyList<ControlSummaryDto> MappedControls);
