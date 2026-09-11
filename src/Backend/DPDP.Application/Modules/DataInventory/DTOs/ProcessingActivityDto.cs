namespace DPDP.Application.Modules.DataInventory.DTOs;

public sealed record ProcessingActivitySummaryDto(
    Guid Id, string ActivityNumber, string Name, string Status, Guid? OwnerUserId, string? OwnerName,
    DateOnly? ReviewDate, int DataCategoryCount, DateTimeOffset CreatedAt);

public sealed record ProcessingActivityDetailDto(
    Guid Id, string ActivityNumber, string Name, string Purpose, IReadOnlyList<string> DataSubjectCategories,
    IReadOnlyList<string> SecurityControls,
    Guid? RetentionPolicyId, string? RetentionPolicyName,
    Guid? OwnerUserId, string? OwnerName, string Status, DateOnly? ReviewDate,
    DateTimeOffset? SubmittedForReviewAt, DateTimeOffset? ReviewedAt, string? ReviewComments,
    DateTimeOffset? ApprovedAt, DateTimeOffset? ArchivedAt,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
    IReadOnlyList<DataCategoryDto> DataCategories, IReadOnlyList<ItSystemDto> ItSystems,
    IReadOnlyList<DataCollectionSourceDto> DataCollectionSources, IReadOnlyList<RecipientDto> Recipients,
    IReadOnlyList<ProcessorDto> Processors);
