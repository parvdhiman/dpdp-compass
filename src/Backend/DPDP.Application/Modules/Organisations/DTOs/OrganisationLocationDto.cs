namespace DPDP.Application.Modules.Organisations.DTOs;

public sealed record OrganisationLocationDto(
    Guid Id,
    string Label,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    bool IsPrimary);
