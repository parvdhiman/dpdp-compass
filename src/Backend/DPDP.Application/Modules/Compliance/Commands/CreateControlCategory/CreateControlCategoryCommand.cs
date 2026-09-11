using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.CreateControlCategory;

public sealed record CreateControlCategoryCommand(string Name, string? Description, int SortOrder) : IRequest<ControlCategoryDto>;

public sealed class CreateControlCategoryCommandValidator : AbstractValidator<CreateControlCategoryCommand>
{
    public CreateControlCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class CreateControlCategoryCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateControlCategoryCommand, ControlCategoryDto>
{
    public async Task<ControlCategoryDto> Handle(CreateControlCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var nameTaken = await db.ControlCategories.AnyAsync(c => c.Name == request.Name, cancellationToken);
        if (nameTaken)
        {
            throw new ConflictException("A control category with this name already exists.");
        }

        var category = new ControlCategory { Name = request.Name.Trim(), Description = request.Description, SortOrder = request.SortOrder };

        db.ControlCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.control_category_created", nameof(ControlCategory), category.Id.ToString(), newValue: new { category.Name }, cancellationToken: cancellationToken);

        return ComplianceMapper.ToDto(category, 0);
    }
}
