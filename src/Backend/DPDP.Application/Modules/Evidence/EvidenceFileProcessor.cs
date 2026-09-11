using System.Security.Cryptography;
using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Evidence;
using FluentValidation;
using FluentValidation.Results;

namespace DPDP.Application.Modules.Evidence;

/// <summary>
/// Shared upload pipeline for both UploadEvidenceCommand and
/// UploadEvidenceVersionCommand: extension/MIME allow-list check, magic-
/// byte signature verification (not just the client-declared Content-Type
/// — see EvidenceFileValidator), checksum, malware scan, then the actual
/// object-storage write. Centralized here rather than duplicated per
/// command so the security checks can only be fixed/extended in one
/// place.
/// </summary>
internal static class EvidenceFileProcessor
{
    public static async Task<EvidenceVersion> BuildFileVersionAsync(
        EvidenceItem evidence,
        EvidenceFileUpload file,
        int versionNumber,
        Guid uploadedByUserId,
        DateTimeOffset now,
        IObjectStorageService objectStorage,
        IMalwareScanner malwareScanner,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!EvidenceFileValidator.IsExtensionAllowed(extension))
        {
            throw new ValidationException([new ValidationFailure(nameof(file.FileName), "File type is not permitted.")]);
        }

        if (!EvidenceFileValidator.IsContentTypeAllowedForExtension(extension, file.ContentType))
        {
            throw new ValidationException([new ValidationFailure(nameof(file.ContentType), "The file's content type does not match its extension.")]);
        }

        using var buffer = new MemoryStream();
        await file.Content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var header = new byte[Math.Min(16, buffer.Length)];
        var read = await buffer.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        buffer.Position = 0;
        if (!EvidenceFileValidator.MatchesFileSignature(extension, header.AsSpan(0, read)))
        {
            throw new ValidationException([new ValidationFailure(nameof(file.FileName), "The file's content does not match its declared type.")]);
        }

        var checksum = Convert.ToHexStringLower(await SHA256.HashDataAsync(buffer, cancellationToken));
        buffer.Position = 0;

        var scanResult = await malwareScanner.ScanAsync(buffer, file.FileName, cancellationToken);
        if (!scanResult.IsClean)
        {
            throw new ConflictException($"Upload rejected: malware scan flagged this file ({scanResult.ThreatName}).");
        }

        buffer.Position = 0;
        var storageKey = $"{evidence.OrganisationId}/{evidence.Id}/v{versionNumber}/blob{extension}";
        await objectStorage.UploadAsync(storageKey, buffer, file.ContentType, cancellationToken);

        return new EvidenceVersion
        {
            OrganisationId = evidence.OrganisationId,
            EvidenceItem = evidence,
            VersionNumber = versionNumber,
            StorageKey = storageKey,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            ChecksumSha256 = checksum,
            MalwareScanStatus = MalwareScanStatus.CLEAN,
            UploadedByUserId = uploadedByUserId,
            UploadedAt = now,
        };
    }

    public static EvidenceVersion BuildUrlVersion(EvidenceItem evidence, string externalUrl, int versionNumber, Guid uploadedByUserId, DateTimeOffset now) => new()
    {
        OrganisationId = evidence.OrganisationId,
        EvidenceItem = evidence,
        VersionNumber = versionNumber,
        ExternalUrl = externalUrl.Trim(),
        MalwareScanStatus = MalwareScanStatus.SKIPPED,
        UploadedByUserId = uploadedByUserId,
        UploadedAt = now,
    };
}
