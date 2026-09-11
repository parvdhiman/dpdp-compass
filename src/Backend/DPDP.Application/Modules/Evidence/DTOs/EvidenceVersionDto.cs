namespace DPDP.Application.Modules.Evidence.DTOs;

public sealed record EvidenceVersionDto(
    Guid Id,
    int VersionNumber,
    string? OriginalFileName,
    string? ContentType,
    long? SizeBytes,
    string? ChecksumSha256,
    string? ExternalUrl,
    string MalwareScanStatus,
    bool CanPreview,
    Guid UploadedByUserId,
    string UploadedByName,
    DateTimeOffset UploadedAt);
