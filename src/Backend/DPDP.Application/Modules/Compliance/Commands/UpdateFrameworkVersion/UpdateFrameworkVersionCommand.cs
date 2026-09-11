using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.UpdateFrameworkVersion;

public sealed record UpdateFrameworkVersionCommand(
    Guid Id,
    string? OfficialCitation,
    DateOnly? PublicationDate,
    DateOnly? EffectiveDate,
    string? SourceUrl,
    string? ChangeSummary,
    string ReviewStatus) : IRequest<FrameworkVersionDto>;

public sealed class UpdateFrameworkVersionCommandValidator : AbstractValidator<UpdateFrameworkVersionCommand>
{
    public UpdateFrameworkVersionCommandValidator()
    {
        RuleFor(x => x.ReviewStatus)
            .Must(status => Enum.TryParse<ContentReviewStatus>(status, out _))
            .WithMessage("reviewStatus must be one of: " + string.Join(", ", Enum.GetNames<ContentReviewStatus>()));
    }
}

public sealed class UpdateFrameworkVersionCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<UpdateFrameworkVersionCommand, FrameworkVersionDto>
{
    public async Task<FrameworkVersionDto> Handle(UpdateFrameworkVersionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var version = await db.FrameworkVersions
            .Include(v => v.Framework)
            .Include(v => v.LegalReferences)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FrameworkVersion), request.Id);

        var oldStatus = version.ReviewStatus;

        version.OfficialCitation = request.OfficialCitation;
        version.PublicationDate = request.PublicationDate;
        version.EffectiveDate = request.EffectiveDate;
        version.SourceUrl = request.SourceUrl;
        version.ChangeSummary = request.ChangeSummary;
        version.ReviewStatus = Enum.Parse<ContentReviewStatus>(request.ReviewStatus);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "compliance.framework_version_updated",
            nameof(FrameworkVersion),
            version.Id.ToString(),
            oldValue: new { ReviewStatus = oldStatus.ToString() },
            newValue: new { ReviewStatus = version.ReviewStatus.ToString() },
            cancellationToken: cancellationToken);

        return ComplianceMapper.ToDetailDto(version);
    }
}
