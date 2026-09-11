using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Identity;

/// <summary>The raw token is never stored — only its SHA-256 hash. See docs/SECURITY.md section 2.</summary>
public sealed class RefreshToken : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
