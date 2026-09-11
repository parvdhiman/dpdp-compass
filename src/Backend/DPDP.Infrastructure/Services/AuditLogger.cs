using System.Text.Json;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Audit;

namespace DPDP.Infrastructure.Services;

public sealed class AuditLogger(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IRequestContext requestContext,
    IDateTimeProvider dateTimeProvider)
    : IAuditLogger
{
    public async Task LogAsync(
        string action,
        string entityType,
        string? entityId,
        object? oldValue = null,
        object? newValue = null,
        CancellationToken cancellationToken = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            OrganisationId = currentUser.OrganisationId,
            UserId = currentUser.UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent,
            CorrelationId = requestContext.CorrelationId,
            CreatedAt = dateTimeProvider.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
