using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Domain.Modules.Evidence;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.Evidence.Commands;

/// <summary>Metadata-only edit (title/description/links/owner/reviewer/expiry) — never touches file content or status.</summary>
public sealed record UpdateEvidenceCommand(
    Guid Id, string Title, string? Description, string? VendorReference, string? ProcessingActivityReference,
    Guid? OwnerUserId, Guid? ReviewerUserId, DateOnly? ExpiryDate) : IRequest<EvidenceDetailDto>;

public sealed class UpdateEvidenceCommandValidator : AbstractValidator<UpdateEvidenceCommand>
{
    public UpdateEvidenceCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class UpdateEvidenceCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<UpdateEvidenceCommand, EvidenceDetailDto>
{
    public async Task<EvidenceDetailDto> Handle(UpdateEvidenceCommand request, CancellationToken cancellationToken)
    {
        var evidence = await EvidenceLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (evidence.Status == EvidenceStatus.ARCHIVED)
        {
            throw new ConflictException("Cannot edit archived evidence.");
        }

        var owner = request.OwnerUserId is { } ownerId
            ? await db.Users.FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken) ?? throw new NotFoundException(nameof(User), ownerId)
            : null;
        var reviewer = request.ReviewerUserId is { } reviewerId
            ? await db.Users.FirstOrDefaultAsync(u => u.Id == reviewerId, cancellationToken) ?? throw new NotFoundException(nameof(User), reviewerId)
            : null;

        evidence.Title = request.Title.Trim();
        evidence.Description = request.Description;
        evidence.VendorReference = request.VendorReference;
        evidence.ProcessingActivityReference = request.ProcessingActivityReference;
        evidence.OwnerUserId = owner?.Id;
        evidence.Owner = owner;
        evidence.ReviewerUserId = reviewer?.Id;
        evidence.Reviewer = reviewer;
        evidence.ExpiryDate = request.ExpiryDate;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("evidence.updated", nameof(EvidenceItem), evidence.Id.ToString(), cancellationToken: cancellationToken);

        return EvidenceMapper.ToDetailDto(evidence, DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime));
    }
}
