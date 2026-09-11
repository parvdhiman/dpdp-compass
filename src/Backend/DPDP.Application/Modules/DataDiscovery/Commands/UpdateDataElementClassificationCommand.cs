using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Commands;

/// <summary>
/// The human-correction path required by the brief ("Allow human
/// correction... Do not make legal determinations automatically"). A
/// human's classification always fully overwrites the system's suggestion
/// with 100% confidence and ClassificationSource.HUMAN — there is no
/// partial-agreement state. Passing a null Category clears the
/// classification entirely (marks the column as reviewed and found not to
/// be personal data), which is itself a form of human correction, not an
/// unset/unknown state.
/// </summary>
public sealed record UpdateDataElementClassificationCommand(Guid Id, string? Category) : IRequest<DataElementDto>;

public sealed class UpdateDataElementClassificationCommandValidator : AbstractValidator<UpdateDataElementClassificationCommand>
{
    public UpdateDataElementClassificationCommandValidator()
    {
        RuleFor(x => x.Category)
            .Must(v => v is null || Enum.TryParse<ClassificationCategory>(v, out _))
            .WithMessage("category must be one of: " + string.Join(", ", Enum.GetNames<ClassificationCategory>()));
    }
}

public sealed class UpdateDataElementClassificationCommandHandler(
    IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<UpdateDataElementClassificationCommand, DataElementDto>
{
    public async Task<DataElementDto> Handle(UpdateDataElementClassificationCommand request, CancellationToken cancellationToken)
    {
        var element = await db.DataElements
            .Include(e => e.DataAsset)
            .Include(e => e.CorrectedByUser)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(DataElement), request.Id);

        var oldCategory = element.ClassificationCategory;
        element.ClassificationCategory = request.Category is null ? null : Enum.Parse<ClassificationCategory>(request.Category);
        element.ClassificationConfidence = element.ClassificationCategory is null ? null : 100m;
        element.ClassificationSource = ClassificationSource.HUMAN;
        element.IsHumanCorrected = true;
        element.CorrectedByUserId = currentUser.UserId;
        element.CorrectedAt = dateTimeProvider.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "datadiscovery.classification_corrected", nameof(DataElement), element.Id.ToString(),
            oldValue: new { Category = oldCategory?.ToString() }, newValue: new { Category = element.ClassificationCategory?.ToString() },
            cancellationToken: cancellationToken);

        element.CorrectedByUser = await db.Users.FirstAsync(u => u.Id == currentUser.UserId, cancellationToken);
        return DataDiscoveryMapper.ToDto(element);
    }
}
