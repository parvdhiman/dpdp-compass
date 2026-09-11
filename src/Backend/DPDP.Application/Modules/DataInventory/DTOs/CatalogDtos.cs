namespace DPDP.Application.Modules.DataInventory.DTOs;

/// <summary>
/// The six shared catalogues (DataCategory/ItSystem/DataCollectionSource/
/// Processor/Recipient/RetentionPolicy) are small and structurally
/// near-identical, so their DTOs are grouped in one file rather than six
/// near-empty files — see docs/DATA_INVENTORY.md.
/// </summary>
public sealed record DataCategoryDto(Guid Id, string Name, string? Description, string? ClassificationCategory, bool IsActive, DateTimeOffset CreatedAt);

public sealed record ItSystemDto(Guid Id, string Name, string? Description, string SystemType, Guid? OwnerUserId, string? OwnerName, bool IsActive, DateTimeOffset CreatedAt);

public sealed record DataCollectionSourceDto(Guid Id, string Name, string? Description, string SourceType, bool IsActive, DateTimeOffset CreatedAt);

public sealed record ProcessorDto(Guid Id, string Name, string? Description, string? ContactEmail, string? Country, bool IsActive, DateTimeOffset CreatedAt);

public sealed record RecipientDto(Guid Id, string Name, string? Description, string RecipientType, bool IsActive, DateTimeOffset CreatedAt);

public sealed record RetentionPolicyDto(Guid Id, string Name, string? Description, int RetentionPeriodValue, string RetentionPeriodUnit, string? TriggerEvent, bool IsActive, DateTimeOffset CreatedAt);
