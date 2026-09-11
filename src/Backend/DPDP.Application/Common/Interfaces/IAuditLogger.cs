namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// Writes to the append-only audit_logs table — see docs/SECURITY.md
/// section 6. Never pass a password or token value as oldValue/newValue.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(
        string action,
        string entityType,
        string? entityId,
        object? oldValue = null,
        object? newValue = null,
        CancellationToken cancellationToken = default);
}
