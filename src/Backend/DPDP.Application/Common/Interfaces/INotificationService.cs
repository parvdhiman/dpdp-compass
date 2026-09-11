using DPDP.Application.Common.Models;

namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// The one abstraction point every "someone should be told about this"
/// event goes through (finding assigned, remediation task assigned) — see
/// docs/ARCHITECTURE.md Module 6 section. The Notifications module itself
/// (real email/Teams delivery, user preferences, digests) is a later,
/// dedicated module (roadmap item 32); today's only implementation logs
/// the message, which is enough to prove every call site is wired
/// correctly without building undeliverable infrastructure early.
/// </summary>
public interface INotificationService
{
    Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
