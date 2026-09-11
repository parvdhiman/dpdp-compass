namespace DPDP.Application.Modules.Organisations.DTOs;

public sealed record OrganisationProfileDto(
    Guid Id,
    string Name,
    string? LegalName,
    string Status,
    string? Industry,
    string? Size,
    string? Country,
    string? Website,
    ContactInfoDto PrimaryContact,
    ContactInfoDto PrivacyContact,
    ContactInfoDto DpoContact,
    IReadOnlyList<OrganisationLocationDto> Locations,
    DateTimeOffset CreatedAt);
