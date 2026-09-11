namespace DPDP.Application.Modules.Organisations.DTOs;

public sealed record BusinessUnitDto(
    Guid Id,
    Guid OrganisationId,
    string Name,
    string? Description,
    ContactInfoDto Head,
    bool IsActive,
    int DepartmentCount,
    DateTimeOffset CreatedAt);
