namespace DPDP.Application.Modules.DataInventory.DTOs;

public sealed record DataInventoryItemDto(
    Guid Id, string ItemNumber, Guid DataCategoryId, string DataCategoryName, string DataElementName,
    Guid? DiscoveredDataElementId, string? Classification,
    Guid? DataCollectionSourceId, string? DataCollectionSourceName,
    Guid? ItSystemId, string? ItSystemName,
    Guid? OwnerUserId, string? OwnerName, string? Purpose,
    Guid? RetentionPolicyId, string? RetentionPolicyName, string? SharingDescription,
    Guid? ProcessorId, string? ProcessorName, string? RiskLevel,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
