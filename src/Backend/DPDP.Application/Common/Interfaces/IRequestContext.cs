namespace DPDP.Application.Common.Interfaces;

/// <summary>Caller IP/user-agent/correlation id, for audit and login-history records.</summary>
public interface IRequestContext
{
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? CorrelationId { get; }
}
