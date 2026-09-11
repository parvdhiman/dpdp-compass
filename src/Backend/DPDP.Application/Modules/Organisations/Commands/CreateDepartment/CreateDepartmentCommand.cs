using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Common;
using DPDP.Domain.Modules.Organisations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.CreateDepartment;

/// <summary>
/// Takes a BusinessUnitId, not an OrganisationId — OrganisationId is
/// always derived from the loaded (already tenant-filtered) business unit,
/// never trusted from client input, so a Department can never end up with
/// a different OrganisationId than its BusinessUnit. See Department.cs.
/// </summary>
public sealed record CreateDepartmentCommand(
    Guid BusinessUnitId,
    string Name,
    string? Description,
    string? HeadName,
    string? HeadEmail,
    string? HeadPhone) : IRequest<DepartmentDto>;

public sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.BusinessUnitId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.HeadEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.HeadEmail));
    }
}

public sealed class CreateDepartmentCommandHandler(
    IAppDbContext db,
    IAuditLogger auditLogger)
    : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        // Tenant-filtered: a business unit outside the caller's
        // organisation is invisible here, so a non-Super-Administrator
        // can never attach a department to another tenant's business unit.
        var businessUnit = await db.BusinessUnits
            .FirstOrDefaultAsync(b => b.Id == request.BusinessUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessUnit), request.BusinessUnitId);

        var nameTaken = await db.Departments
            .AnyAsync(d => d.BusinessUnitId == request.BusinessUnitId && d.Name == request.Name, cancellationToken);
        if (nameTaken)
        {
            throw new ConflictException("A department with this name already exists in this business unit.");
        }

        var department = new Department
        {
            OrganisationId = businessUnit.OrganisationId,
            BusinessUnitId = businessUnit.Id,
            Name = request.Name.Trim(),
            Description = request.Description,
            Head = new ContactInfo { Name = request.HeadName, Email = request.HeadEmail, Phone = request.HeadPhone },
        };

        db.Departments.Add(department);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("department.created", nameof(Department), department.Id.ToString(), newValue: new { department.Name }, cancellationToken: cancellationToken);

        return new DepartmentDto(
            department.Id, department.OrganisationId, department.BusinessUnitId, businessUnit.Name,
            department.Name, department.Description, OrganisationMapper.ToDto(department.Head), department.IsActive, department.CreatedAt);
    }
}
