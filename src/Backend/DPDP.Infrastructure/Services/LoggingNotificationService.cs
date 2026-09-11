using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace DPDP.Infrastructure.Services;

/// <summary>
/// The only implementation of INotificationService today — logs the
/// notification instead of delivering it. Swap this registration for a
/// real email/Teams-backed implementation when the Notifications module is
/// built; no call site changes. See docs/ARCHITECTURE.md Module 6 section.
/// </summary>
public sealed class LoggingNotificationService(ILogger<LoggingNotificationService> logger) : INotificationService
{
    public Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification [{Category}] to user {RecipientUserId}: {Subject} — {Body}",
            message.Category, message.RecipientUserId, message.Subject, message.Body);
        return Task.CompletedTask;
    }
}
