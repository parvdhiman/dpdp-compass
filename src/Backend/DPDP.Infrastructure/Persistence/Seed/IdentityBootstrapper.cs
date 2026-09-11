using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using DPDP.Infrastructure.Persistence.Configurations.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DPDP.Infrastructure.Persistence.Seed;

/// <summary>
/// Creates the first organisation and its Super Administrator account, once,
/// on startup — idempotent (no-op once any Super Administrator exists).
/// Credentials come only from environment variables
/// (BOOTSTRAP_SUPERADMIN_EMAIL / BOOTSTRAP_SUPERADMIN_PASSWORD /
/// BOOTSTRAP_ORGANISATION_NAME); never hard-coded, never defaulted to
/// anything guessable. See .env.example and docs/DEVELOPMENT.md. This is
/// the *only* place an org-less Super Administrator account can be
/// created — CreateUserCommand deliberately refuses to (see its doc
/// comment).
/// </summary>
public static class IdentityBootstrapper
{
    public static async Task RunAsync(IServiceProvider rootServices, CancellationToken cancellationToken = default)
    {
        await using var scope = rootServices.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var db = services.GetRequiredService<IAppDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityBootstrapper");

        var superAdministratorExists = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.OrganisationId == null && !u.IsDeleted, cancellationToken);

        if (superAdministratorExists)
        {
            return;
        }

        var email = Environment.GetEnvironmentVariable("BOOTSTRAP_SUPERADMIN_EMAIL");
        var password = Environment.GetEnvironmentVariable("BOOTSTRAP_SUPERADMIN_PASSWORD");
        var organisationName = Environment.GetEnvironmentVariable("BOOTSTRAP_ORGANISATION_NAME") ?? "Default Organisation";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No Super Administrator account exists and BOOTSTRAP_SUPERADMIN_EMAIL/BOOTSTRAP_SUPERADMIN_PASSWORD " +
                "are not set — skipping bootstrap. Set both environment variables and restart to create the first account.");
            return;
        }

        var passwordPolicy = services.GetRequiredService<IPasswordPolicy>();
        var policyErrors = passwordPolicy.Validate(password, email);
        if (policyErrors.Count > 0)
        {
            logger.LogError(
                "BOOTSTRAP_SUPERADMIN_PASSWORD does not satisfy the password policy ({ErrorCount} rule(s) failed) — skipping bootstrap.",
                policyErrors.Count);
            return;
        }

        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var now = DateTimeOffset.UtcNow;

        var organisation = new Organisation
        {
            Name = organisationName,
            Status = OrganisationStatus.Active,
        };
        db.Organisations.Add(organisation);

        var user = new User
        {
            OrganisationId = null,
            Email = email.Trim(),
            NormalizedEmail = email.Trim().ToUpperInvariant(),
            PasswordHash = passwordHasher.HashPassword(password),
            FullName = "Super Administrator",
            IsActive = true,
            MustChangePassword = false,
        };
        user.UserRoles.Add(new UserRole
        {
            RoleId = RoleConfiguration.RoleId(RoleNames.SuperAdministrator),
            OrganisationId = null,
            AssignedAt = now,
        });
        db.Users.Add(user);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Bootstrapped Super Administrator account and organisation {OrganisationName}.",
            organisationName);
    }
}
