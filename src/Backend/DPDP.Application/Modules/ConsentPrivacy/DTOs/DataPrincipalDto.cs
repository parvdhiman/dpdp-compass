namespace DPDP.Application.Modules.ConsentPrivacy.DTOs;

public sealed record DataPrincipalDto(
    Guid Id, string ExternalReferenceId, string? ReferenceCategory, string? Notes, bool IsActive, DateTimeOffset CreatedAt);

public sealed record ConsentPurposeDto(
    Guid Id, string Name, string? Description, Guid? DataCategoryId, string? DataCategoryName, bool IsActive, DateTimeOffset CreatedAt);

public sealed record SlaPolicyDto(
    Guid Id, string Name, string? Description, string? RequestType, int ResponseDueDays, bool IsActive, DateTimeOffset CreatedAt);
