using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Domain.Modules.Evidence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Evidence.Queries;

/// <summary>
/// Streams the original uploaded file for one version. VersionNumber
/// defaults to the latest. Tenant isolation is enforced by the global
/// query filter on EvidenceItems — a cross-tenant Id simply 404s.
/// </summary>
public sealed record DownloadEvidenceVersionQuery(Guid EvidenceId, int? VersionNumber = null) : IRequest<FileDownloadResult>;

public sealed class DownloadEvidenceVersionQueryHandler(IAppDbContext db, IObjectStorageService objectStorage)
    : IRequestHandler<DownloadEvidenceVersionQuery, FileDownloadResult>
{
    public async Task<FileDownloadResult> Handle(DownloadEvidenceVersionQuery request, CancellationToken cancellationToken)
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

        if (string.IsNullOrEmpty(version.StorageKey))
        {
            throw new ConflictException("This evidence version is a URL reference and has no downloadable file.");
        }

        var stream = await objectStorage.DownloadAsync(version.StorageKey, cancellationToken);
        return new FileDownloadResult(stream, version.ContentType ?? "application/octet-stream", version.OriginalFileName ?? "evidence");
    }
}
