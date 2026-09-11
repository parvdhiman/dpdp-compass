namespace DPDP.Application.Modules.Organisations.DTOs;

public sealed record OrganisationDashboardDto(
    Guid OrganisationId,
    string Name,
    string? Industry,
    string? Size,
    int BusinessUnitCount,
    int DepartmentCount,
    int ActiveUserCount,
    OrganisationLocationDto? PrimaryLocation,
    bool HasDpoConfigured);
