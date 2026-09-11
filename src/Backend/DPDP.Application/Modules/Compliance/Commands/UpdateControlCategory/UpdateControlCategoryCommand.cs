using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.UpdateControlCategory;

public sealed record UpdateControlCategoryCommand(Guid Id, string Name, string? Description, int SortOrder) : IRequest<ControlCategoryDto>;

public sealed class UpdateControlCategoryCommandValidator : AbstractValidator<UpdateControlCategoryCommand>
{
    public UpdateControlCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class UpdateControlCategoryCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<UpdateControlCategoryCommand, ControlCategoryDto>
{
    public async Task<ControlCategoryDto> Handle(UpdateControlCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var category = await db.ControlCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ControlCategory), request.Id);

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.SortOrder = request.SortOrder;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.control_category_updated", nameof(ControlCategory), category.Id.ToString(), cancellationToken: cancellationToken);

        var controlCount = await db.Controls.CountAsync(c => c.ControlCategoryId == category.Id, cancellationToken);
        return ComplianceMapper.ToDto(category, controlCount);
    }
}
