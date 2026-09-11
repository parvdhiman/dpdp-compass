namespace DPDP.Application.Modules.ConsentPrivacy.DTOs;

public sealed record PrivacyNoticeSummaryDto(
    Guid Id, string NoticeNumber, string Code, string Title, string Version, string Language,
    string Status, DateOnly? PublishedDate, DateOnly? EffectiveDate, DateTimeOffset CreatedAt);

public sealed record PrivacyNoticeDetailDto(
    Guid Id, string NoticeNumber, string Code, string Title, string Version, string Language, string Purpose,
    string Status, DateOnly? PublishedDate, DateOnly? EffectiveDate,
    DateTimeOffset? ApprovedAt, DateTimeOffset? PublishedAt, DateTimeOffset? ArchivedAt,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
    IReadOnlyList<DataCategoryRefDto> DataCategories);

public sealed record DataCategoryRefDto(Guid Id, string Name);
