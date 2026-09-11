namespace DPDP.Application.Modules.ConsentPrivacy.DTOs;

public sealed record ConsentRecordDto(
    Guid Id, string ConsentNumber, Guid DataPrincipalId, string DataPrincipalReference,
    Guid ConsentPurposeId, string ConsentPurposeName, Guid? NoticeVersionId, string? NoticeVersionLabel,
    DateTimeOffset GrantedAt, string Channel, string Status, DateTimeOffset? WithdrawnAt, DateTimeOffset? ExpiresAt,
    string? SourceSystem, string? ExternalReferenceId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
