using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Domain.Modules.Evidence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Evidence.Queries;

/// <summary>
/// Same as DownloadEvidenceVersionQuery but restricted to content types
/// EvidenceFileValidator.IsPreviewSafe allows — deliberately excludes
/// HTML/SVG/XML so an inline browser preview can never execute script
/// from an uploaded file.
/// </summary>
public sealed record PreviewEvidenceVersionQuery(Guid EvidenceId, int? VersionNumber = null) : IRequest<FileDownloadResult>;

public sealed class PreviewEvidenceVersionQueryHandler(IAppDbContext db, IObjectStorageService objectStorage)
    : IRequestHandler<PreviewEvidenceVersionQuery, FileDownloadResult>
{
    public async Task<FileDownloadResult> Handle(PreviewEvidenceVersionQuery request, CancellationToken cancellationToken)
    {
        var evidence = await db.EvidenceItems
            .Include(e => e.Versions)
            .FirstOrDefaultAsync(e => e.Id == request.EvidenceId, cancellationToken)
            ?? throw new NotFoundException(nameof(EvidenceItem), request.EvidenceId);

        var version = request.VersionNumber is { } versionNumber
            ? evidence.Versions.FirstOrDefault(v => v.VersionNumber == versionNumber)
            : evidence.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();

        if (version is null)
        {
            throw new NotFoundException(nameof(EvidenceVersion), request.VersionNumber ?? 0);
        }

        if (string.IsNullOrEmpty(version.StorageKey) || !EvidenceFileValidator.IsPreviewSafe(version.ContentType))
        {
            throw new ConflictException("This evidence version cannot be previewed inline; download it instead.");
        }

        var stream = await objectStorage.DownloadAsync(version.StorageKey, cancellationToken);
        return new FileDownloadResult(stream, version.ContentType!, version.OriginalFileName ?? "evidence");
    }
}
