namespace DPDP.Application.Common.Models;

/// <summary>
/// A channel-agnostic notification — "Notifications should be abstracted
/// for future email/Teams integration" (Module 6 brief). RecipientUserId
/// lets the concrete channel look up the user's real email/Teams handle
/// itself; this record never carries a delivery address directly, so
/// swapping the channel never requires changing a call site.
/// </summary>
public sealed record NotificationMessage(Guid RecipientUserId, string Category, string Subject, string Body);
