namespace DPDP.Application.Modules.Assessments.DTOs;

public sealed record AssessmentScopeDto(
    Guid Id,
    Guid? BusinessUnitId,
    string? BusinessUnitName,
    Guid? DepartmentId,
    string? DepartmentName,
    string? Notes);
