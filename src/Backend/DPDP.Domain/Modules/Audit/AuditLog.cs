using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Audit;

/// <summary>
/// Append-only — see docs/SECURITY.md section 6 for the tamper-resistance
/// approach (a restricted DB role, not application convention alone).
/// </summary>
public sealed class AuditLog : Entity
{
    public Guid? OrganisationId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
