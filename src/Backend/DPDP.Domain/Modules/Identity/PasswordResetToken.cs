using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Identity;

/// <summary>The raw token is never stored — only its SHA-256 hash, and it is never logged. See docs/SECURITY.md.</summary>
public sealed class PasswordResetToken : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsActive => UsedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
