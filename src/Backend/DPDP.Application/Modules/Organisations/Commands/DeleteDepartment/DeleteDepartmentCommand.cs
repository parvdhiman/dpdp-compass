using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Organisations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.DeleteDepartment;

public sealed record DeleteDepartmentCommand(Guid Id) : IRequest;

public sealed class DeleteDepartmentCommandHandler(
    IAppDbContext db,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<DeleteDepartmentCommand>
{
    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await db.Departments
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Department), request.Id);

        department.IsDeleted = true;
        department.DeletedAt = dateTimeProvider.UtcNow;
        department.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("department.deleted", nameof(Department), department.Id.ToString(), cancellationToken: cancellationToken);
    }
}
