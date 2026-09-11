using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.UpdateUser;

public sealed record UpdateUserCommand(Guid UserId, string FullName, string? PhoneNumber) : IRequest<UserDto>;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
    }
}

/// <summary>
/// The global query filter on User already prevents loading a user from
/// another organisation (see docs/ARCHITECTURE.md section 4) — an
/// out-of-tenant UserId here surfaces as NotFoundException, not Forbidden.
/// </summary>
public sealed class UpdateUserCommandHandler(
    IAppDbContext db,
    IAuditLogger auditLogger)
    : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var oldValue = new { user.FullName, user.PhoneNumber };
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("user.updated", nameof(User), user.Id.ToString(), oldValue, new { user.FullName, user.PhoneNumber }, cancellationToken);

        return new UserDto(
            user.Id, user.OrganisationId, user.Email, user.FullName, user.PhoneNumber,
            user.IsActive, user.MustChangePassword, user.LastLoginAt, user.CreatedAt,
            user.UserRoles.Select(ur => new RoleSummaryDto(ur.RoleId, ur.Role.Name)).ToList());
    }
}
