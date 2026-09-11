using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Common;
using DPDP.Domain.Modules.Organisations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.UpdateDepartment;

public sealed record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string? Description,
    string? HeadName,
    string? HeadEmail,
    string? HeadPhone,
    bool IsActive) : IRequest<DepartmentDto>;

public sealed class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.HeadEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.HeadEmail));
    }
}

public sealed class UpdateDepartmentCommandHandler(
    IAppDbContext db,
    IAuditLogger auditLogger)
    : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await db.Departments
            .Include(d => d.BusinessUnit)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Department), request.Id);

        var nameTaken = await db.Departments
            .AnyAsync(d => d.BusinessUnitId == department.BusinessUnitId && d.Name == request.Name && d.Id != request.Id, cancellationToken);
        if (nameTaken)
        {
            throw new ConflictException("A department with this name already exists in this business unit.");
        }

        var oldValue = new { department.Name, department.IsActive };

        department.Name = request.Name.Trim();
        department.Description = request.Description;
        department.Head = new ContactInfo { Name = request.HeadName, Email = request.HeadEmail, Phone = request.HeadPhone };
        department.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("department.updated", nameof(Department), department.Id.ToString(), oldValue, new { department.Name, department.IsActive }, cancellationToken);

        return new DepartmentDto(
            department.Id, department.OrganisationId, department.BusinessUnitId, department.BusinessUnit.Name,
            department.Name, department.Description, OrganisationMapper.ToDto(department.Head), department.IsActive, department.CreatedAt);
    }
}
