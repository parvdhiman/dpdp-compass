namespace DPDP.Application.Modules.DataInventory.DTOs;

public sealed record DataFlowDto(
    Guid Id, string FlowNumber, string Name, string? Description,
    Guid? ProcessingActivityId, string? ProcessingActivityName,
    Guid? DataCategoryId, string? DataCategoryName,
    Guid? FromItSystemId, string? FromItSystemName,
    Guid? FromDataCollectionSourceId, string? FromDataCollectionSourceName, string FromDescription,
    Guid? ToItSystemId, string? ToItSystemName,
    Guid? ToProcessorId, string? ToProcessorName,
    Guid? ToRecipientId, string? ToRecipientName, string ToDescription,
    string? TransferMechanism, bool IsCrossBorder, string? CrossBorderCountry,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
