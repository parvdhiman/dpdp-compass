using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.UpdateLegalReference;

public sealed record UpdateLegalReferenceCommand(
    Guid Id,
    string Title,
    string? Chapter,
    string? SummaryText,
    string SourceCitation,
    string ReviewStatus) : IRequest<LegalReferenceDto>;

public sealed class UpdateLegalReferenceCommandValidator : AbstractValidator<UpdateLegalReferenceCommand>
{
    public UpdateLegalReferenceCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.SourceCitation).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ReviewStatus)
            .Must(status => Enum.TryParse<ContentReviewStatus>(status, out _))
            .WithMessage("reviewStatus must be one of: " + string.Join(", ", Enum.GetNames<ContentReviewStatus>()));
    }
}

public sealed class UpdateLegalReferenceCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<UpdateLegalReferenceCommand, LegalReferenceDto>
{
    public async Task<LegalReferenceDto> Handle(UpdateLegalReferenceCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var legalReference = await db.LegalReferences
            .Include(l => l.Requirements)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(LegalReference), request.Id);

        legalReference.Title = request.Title.Trim();
        legalReference.Chapter = request.Chapter;
        legalReference.SummaryText = request.SummaryText;
        legalReference.SourceCitation = request.SourceCitation.Trim();
        legalReference.ReviewStatus = Enum.Parse<ContentReviewStatus>(request.ReviewStatus);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.legal_reference_updated", nameof(LegalReference), legalReference.Id.ToString(), cancellationToken: cancellationToken);

        return ComplianceMapper.ToDto(legalReference);
    }
}
