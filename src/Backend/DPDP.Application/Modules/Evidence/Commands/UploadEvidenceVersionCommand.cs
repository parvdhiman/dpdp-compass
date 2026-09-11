using System.Security.Cryptography;
using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Domain.Modules.Evidence;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Evidence.Commands;

internal static class EvidenceLoader
{
    public static async Task<EvidenceItem> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.EvidenceItems
            .Include(e => e.Control)
            .Include(e => e.Finding)
            .Include(e => e.Owner)
            .Include(e => e.Reviewer)
            .Include(e => e.Versions).ThenInclude(v => v.UploadedByUser)
            .Include(e => e.Reviews).ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(EvidenceItem), id);
}

/// <summary>
/// "Versioning" — uploading a new version always resets Status to
/// UPLOADED (restarting the review cycle), from any status except
/// ARCHIVED, unconditionally — the same "restart, don't validate a
/// generic transition" precedent Module 6 set for Finding.Reopen.
/// </summary>
public sealed record UploadEvidenceVersionCommand(Guid EvidenceId, string? ExternalUrl, EvidenceFileUpload? File) : IRequest<EvidenceDetailDto>;

public sealed class UploadEvidenceVersionCommandValidator : AbstractValidator<UploadEvidenceVersionCommand>
{
    public UploadEvidenceVersionCommandValidator()
    {
        RuleFor(x => x.EvidenceId).NotEmpty();
    }
}

public sealed class UploadEvidenceVersionCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IDateTimeProvider dateTimeProvider,
    IObjectStorageService objectStorage,
    IMalwareScanner malwareScanner,
    IAuditLogger auditLogger)
    : IRequestHandler<UploadEvidenceVersionCommand, EvidenceDetailDto>
{
    public async Task<EvidenceDetailDto> Handle(UploadEvidenceVersionCommand request, CancellationToken cancellationToken)
    {
        var evidence = await EvidenceLoader.LoadForDetailAsync(db, request.EvidenceId, cancellationToken);

        if (evidence.Status == EvidenceStatus.ARCHIVED)
        {
            throw new ConflictException("Cannot add a new version to archived evidence.");
        }

        var isUrlType = evidence.EvidenceType == Domain.Modules.Evidence.EvidenceType.URL;
        if (isUrlType && string.IsNullOrWhiteSpace(request.ExternalUrl))
        {
            throw new ValidationException([new ValidationFailure(nameof(request.ExternalUrl), "externalUrl is required for URL evidence.")]);
        }

        if (!isUrlType && request.File is null)
        {
            throw new ValidationException([new ValidationFailure(nameof(request.File), "A file is required for this evidence type.")]);
        }

        var now = dateTimeProvider.UtcNow;
        var nextVersionNumber = evidence.Versions.Max(v => v.VersionNumber) + 1;

        var version = isUrlType
            ? EvidenceFileProcessor.BuildUrlVersion(evidence, request.ExternalUrl!, nextVersionNumber, currentUser.UserId!.Value, now)
            : await EvidenceFileProcessor.BuildFileVersionAsync(evidence, request.File!, nextVersionNumber, currentUser.UserId!.Value, now, objectStorage, malwareScanner, cancellationToken);

        evidence.Versions.Add(version);
        db.EvidenceVersions.Add(version);
        evidence.Status = EvidenceStatus.UPLOADED;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "evidence.version_uploaded",
            nameof(EvidenceItem),
            evidence.Id.ToString(),
            newValue: new { VersionNumber = nextVersionNumber },
            cancellationToken: cancellationToken);

        return EvidenceMapper.ToDetailDto(evidence, DateOnly.FromDateTime(now.UtcDateTime));
    }
}
