using System.Security.Cryptography;
using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Evidence;
using DPDP.Domain.Modules.Findings;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DPDP.Application.Modules.Evidence.Commands;

public sealed record UploadEvidenceCommand(
    string Title,
    string? Description,
    string EvidenceType,
    Guid? AssessmentId,
    Guid? ControlId,
    Guid? FindingId,
    string? VendorReference,
    string? ProcessingActivityReference,
    Guid? OwnerUserId,
    Guid? ReviewerUserId,
    DateOnly? ExpiryDate,
    string? ExternalUrl,
    EvidenceFileUpload? File) : IRequest<EvidenceDetailDto>;

public sealed class UploadEvidenceCommandValidator : AbstractValidator<UploadEvidenceCommand>
{
    public UploadEvidenceCommandValidator(IOptions<EvidenceOptions> options)
    {
        var maxSize = options.Value.MaxUploadSizeBytes;

        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.EvidenceType)
            .Must(v => Enum.TryParse<Domain.Modules.Evidence.EvidenceType>(v, out _))
            .WithMessage("evidenceType must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.Evidence.EvidenceType>()));

        RuleFor(x => x.ExternalUrl)
            .NotEmpty()
            .When(x => x.EvidenceType == nameof(Domain.Modules.Evidence.EvidenceType.URL))
            .WithMessage("externalUrl is required for URL evidence.");
        RuleFor(x => x.File)
            .Null()
            .When(x => x.EvidenceType == nameof(Domain.Modules.Evidence.EvidenceType.URL))
            .WithMessage("URL evidence must not include an uploaded file.");

        RuleFor(x => x.File)
            .NotNull()
            .When(x => x.EvidenceType != nameof(Domain.Modules.Evidence.EvidenceType.URL))
            .WithMessage("A file is required for this evidence type.");
        RuleFor(x => x.File!.Length)
            .LessThanOrEqualTo(maxSize)
            .When(x => x.File is not null)
            .WithMessage($"File exceeds the maximum upload size of {maxSize} bytes.");
        RuleFor(x => x.File!.FileName)
            .Must(name => EvidenceFileValidator.IsExtensionAllowed(Path.GetExtension(name)))
            .When(x => x.File is not null)
            .WithMessage("File type is not permitted.");
        RuleFor(x => x)
            .Must(x => x.File is null || EvidenceFileValidator.IsContentTypeAllowedForExtension(Path.GetExtension(x.File.FileName), x.File.ContentType))
            .When(x => x.File is not null)
            .WithMessage("The file's content type does not match its extension.");
    }
}

public sealed class UploadEvidenceCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IDateTimeProvider dateTimeProvider,
    IObjectStorageService objectStorage,
    IMalwareScanner malwareScanner,
    INotificationService notificationService,
    IAuditLogger auditLogger)
    : IRequestHandler<UploadEvidenceCommand, EvidenceDetailDto>
{
    public async Task<EvidenceDetailDto> Handle(UploadEvidenceCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can upload evidence.");
        }

        if (request.AssessmentId is { } assessmentId && !await db.Assessments.AnyAsync(a => a.Id == assessmentId, cancellationToken))
        {
            throw new NotFoundException(nameof(Assessment), assessmentId);
        }

        if (request.ControlId is { } controlId && !await db.Controls.AnyAsync(c => c.Id == controlId, cancellationToken))
        {
            throw new NotFoundException(nameof(Control), controlId);
        }

        if (request.FindingId is { } findingId && !await db.Findings.AnyAsync(f => f.Id == findingId, cancellationToken))
        {
            throw new NotFoundException(nameof(Finding), findingId);
        }

        var owner = request.OwnerUserId is { } ownerId
            ? await db.Users.FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken) ?? throw new NotFoundException(nameof(User), ownerId)
            : null;
        var reviewer = request.ReviewerUserId is { } reviewerId
            ? await db.Users.FirstOrDefaultAsync(u => u.Id == reviewerId, cancellationToken) ?? throw new NotFoundException(nameof(User), reviewerId)
            : null;

        var now = dateTimeProvider.UtcNow;
        var evidenceType = Enum.Parse<Domain.Modules.Evidence.EvidenceType>(request.EvidenceType);

        var evidence = new EvidenceItem
        {
            OrganisationId = organisationId,
            Title = request.Title.Trim(),
            Description = request.Description,
            EvidenceType = evidenceType,
            Status = EvidenceStatus.UPLOADED,
            AssessmentId = request.AssessmentId,
            ControlId = request.ControlId,
            FindingId = request.FindingId,
            VendorReference = request.VendorReference,
            ProcessingActivityReference = request.ProcessingActivityReference,
            OwnerUserId = owner?.Id,
            Owner = owner,
            ReviewerUserId = reviewer?.Id,
            Reviewer = reviewer,
            ExpiryDate = request.ExpiryDate,
        };

        var version = evidenceType == Domain.Modules.Evidence.EvidenceType.URL
            ? EvidenceFileProcessor.BuildUrlVersion(evidence, request.ExternalUrl!, 1, currentUser.UserId!.Value, now)
            : await EvidenceFileProcessor.BuildFileVersionAsync(evidence, request.File!, 1, currentUser.UserId!.Value, now, objectStorage, malwareScanner, cancellationToken);

        evidence.Versions.Add(version);

        db.EvidenceItems.Add(evidence);
        db.EvidenceVersions.Add(version);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "evidence.uploaded",
            nameof(EvidenceItem),
            evidence.Id.ToString(),
            newValue: new { evidence.Title, evidence.EvidenceType, version.ChecksumSha256 },
            cancellationToken: cancellationToken);

        if (reviewer is not null)
        {
            await notificationService.NotifyAsync(new NotificationMessage(
                reviewer.Id, "evidence.reviewer_assigned", $"Evidence awaiting review: {evidence.Title}",
                $"You have been set as the reviewer for evidence {EvidenceMapper.DisplayNumber(evidence)}."), cancellationToken);
        }

        return EvidenceMapper.ToDetailDto(evidence, DateOnly.FromDateTime(now.UtcDateTime));
    }
}
