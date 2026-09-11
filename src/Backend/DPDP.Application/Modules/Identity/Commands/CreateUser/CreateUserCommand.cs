using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.CreateUser;

/// <summary>
/// Every user created here belongs to exactly one organisation — the
/// org-less Super Administrator bootstrap account is created only by the
/// startup seeder, never via this endpoint (see Module 2 completion
/// report). Consequently the Super Administrator role can never be
/// assigned through this command.
/// </summary>
public sealed record CreateUserCommand(
    Guid OrganisationId,
    string Email,
    string FullName,
    string? PhoneNumber,
    string Password,
    IReadOnlyList<Guid> RoleIds) : IRequest<UserDto>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IPasswordPolicy passwordPolicy)
    {
        RuleFor(x => x.OrganisationId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password)
            .NotEmpty()
            .Must((command, password) => passwordPolicy.Validate(password, command.Email).Count == 0)
            .WithMessage("Password does not meet the required policy.");
    }
}

public sealed class CreateUserCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ICurrentUserContext currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditLogger auditLogger)
    : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator && request.OrganisationId != currentUser.OrganisationId)
        {
            throw new ForbiddenException("Cannot create a user in another organisation.");
        }

        var organisation = await db.Organisations
            .FirstOrDefaultAsync(o => o.Id == request.OrganisationId && !o.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Organisation), request.OrganisationId);

        if (organisation.Status != OrganisationStatus.Active)
        {
            throw new ConflictException("Cannot create a user in a suspended organisation.");
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var emailTaken = await db.Users
            .AnyAsync(u => u.OrganisationId == request.OrganisationId && u.NormalizedEmail == normalizedEmail, cancellationToken);
        if (emailTaken)
        {
            throw new ConflictException("A user with this email already exists in this organisation.");
        }

        var roles = await db.Roles
            .Where(r => request.RoleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        if (roles.Count != request.RoleIds.Distinct().Count())
        {
            throw new ValidationException("One or more role ids do not exist.");
        }

        if (roles.Any(r => r.Name == RoleNames.SuperAdministrator))
        {
            throw new ForbiddenException("The Super Administrator role cannot be assigned via user creation.");
        }

        var now = dateTimeProvider.UtcNow;
        var user = new User
        {
            OrganisationId = request.OrganisationId,
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            MustChangePassword = true,
        };

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole
            {
                Role = role,
                OrganisationId = request.OrganisationId,
                AssignedAt = now,
                AssignedBy = currentUser.UserId,
            });
        }

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("user.created", nameof(User), user.Id.ToString(), newValue: new { user.Email, user.FullName, request.OrganisationId }, cancellationToken: cancellationToken);

        return new UserDto(
            user.Id,
            user.OrganisationId,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.IsActive,
            user.MustChangePassword,
            user.LastLoginAt,
            user.CreatedAt,
            roles.Select(r => new RoleSummaryDto(r.Id, r.Name)).ToList());
    }
}
