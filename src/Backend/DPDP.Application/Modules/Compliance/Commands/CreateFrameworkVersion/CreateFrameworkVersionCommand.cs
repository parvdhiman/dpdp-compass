using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.CreateFrameworkVersion;

public sealed record CreateFrameworkVersionCommand(
    Guid FrameworkId,
    string VersionLabel,
    string? OfficialCitation,
    DateOnly? PublicationDate,
    DateOnly? EffectiveDate,
    string? SourceUrl,
    string? ChangeSummary) : IRequest<FrameworkVersionDto>;

public sealed class CreateFrameworkVersionCommandValidator : AbstractValidator<CreateFrameworkVersionCommand>
{
    public CreateFrameworkVersionCommandValidator()
    {
        RuleFor(x => x.VersionLabel).NotEmpty().MaximumLength(50);
    }
}

public sealed class CreateFrameworkVersionCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateFrameworkVersionCommand, FrameworkVersionDto>
{
    public async Task<FrameworkVersionDto> Handle(CreateFrameworkVersionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var framework = await db.Frameworks.FirstOrDefaultAsync(f => f.Id == request.FrameworkId, cancellationToken)
            ?? throw new NotFoundException(nameof(Framework), request.FrameworkId);

        var labelTaken = await db.FrameworkVersions
            .AnyAsync(v => v.FrameworkId == request.FrameworkId && v.VersionLabel == request.VersionLabel, cancellationToken);
        if (labelTaken)
        {
            throw new ConflictException("A version with this label already exists for this framework.");
        }

        var version = new FrameworkVersion
        {
            FrameworkId = request.FrameworkId,
            VersionLabel = request.VersionLabel.Trim(),
            OfficialCitation = request.OfficialCitation,
            PublicationDate = request.PublicationDate,
            EffectiveDate = request.EffectiveDate,
            SourceUrl = request.SourceUrl,
            ChangeSummary = request.ChangeSummary,
            ReviewStatus = ContentReviewStatus.DRAFT,
            IsCurrent = false,
        };

        db.FrameworkVersions.Add(version);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.framework_version_created", nameof(FrameworkVersion), version.Id.ToString(), newValue: new { version.VersionLabel }, cancellationToken: cancellationToken);

        version.Framework = framework;
        return ComplianceMapper.ToDetailDto(version);
    }
}
