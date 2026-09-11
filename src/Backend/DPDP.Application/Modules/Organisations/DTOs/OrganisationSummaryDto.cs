namespace DPDP.Application.Modules.Organisations.DTOs;

public sealed record OrganisationSummaryDto(
    Guid Id,
    string Name,
    string? LegalName,
    string Status,
    string? Industry,
    string? Size,
    string? Country,
    DateTimeOffset CreatedAt);
