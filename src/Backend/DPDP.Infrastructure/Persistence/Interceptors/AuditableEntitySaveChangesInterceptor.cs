using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DPDP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Stamps created_at/created_by/updated_at/updated_by on every
/// AuditableEntity automatically — see docs/DATABASE.md section 1
/// ("never populated by triggers... handled in app code"). This is that
/// one place, so no command handler has to remember to do it.
/// </summary>
public sealed class AuditableEntitySaveChangesInterceptor(
    ICurrentUserContext currentUser,
    IDateTimeProvider dateTimeProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = dateTimeProvider.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUser.UserId;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser.UserId;
                    break;
            }
        }
    }
}
