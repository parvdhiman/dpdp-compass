using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.CreateLegalReference;

/// <summary>SourceCitation is required — "every legal reference must contain its source" (Module 4 brief).</summary>
public sealed record CreateLegalReferenceCommand(
    Guid FrameworkVersionId,
    string Citation,
    string Title,
    string? Chapter,
    string? SummaryText,
    string SourceCitation) : IRequest<LegalReferenceDto>;

public sealed class CreateLegalReferenceCommandValidator : AbstractValidator<CreateLegalReferenceCommand>
{
    public CreateLegalReferenceCommandValidator()
    {
        RuleFor(x => x.Citation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.SourceCitation).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateLegalReferenceCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateLegalReferenceCommand, LegalReferenceDto>
{
    public async Task<LegalReferenceDto> Handle(CreateLegalReferenceCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var versionExists = await db.FrameworkVersions.AnyAsync(v => v.Id == request.FrameworkVersionId, cancellationToken);
        if (!versionExists)
        {
            throw new NotFoundException(nameof(FrameworkVersion), request.FrameworkVersionId);
        }

        var citationTaken = await db.LegalReferences
            .AnyAsync(l => l.FrameworkVersionId == request.FrameworkVersionId && l.Citation == request.Citation, cancellationToken);
        if (citationTaken)
        {
            throw new ConflictException("This citation already exists for this framework version.");
        }

        var legalReference = new LegalReference
        {
            FrameworkVersionId = request.FrameworkVersionId,
            Citation = request.Citation.Trim(),
            Title = request.Title.Trim(),
            Chapter = request.Chapter,
            SummaryText = request.SummaryText,
            SourceCitation = request.SourceCitation.Trim(),
            ReviewStatus = ContentReviewStatus.DRAFT,
        };

        db.LegalReferences.Add(legalReference);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.legal_reference_created", nameof(LegalReference), legalReference.Id.ToString(), newValue: new { legalReference.Citation }, cancellationToken: cancellationToken);

        return ComplianceMapper.ToDto(legalReference);
    }
}
