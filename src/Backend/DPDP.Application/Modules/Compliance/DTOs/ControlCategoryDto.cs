namespace DPDP.Application.Modules.Compliance.DTOs;

public sealed record ControlCategoryDto(Guid Id, string Name, string? Description, int SortOrder, int ControlCount);
