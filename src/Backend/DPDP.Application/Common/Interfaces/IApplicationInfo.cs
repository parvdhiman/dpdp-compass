namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// Read-only facts about the running deployment, exposed by GET /api/v1/system/info.
/// Deliberately narrow: this must never grow to expose configuration values,
/// connection strings, or secrets — see docs/SECURITY.md section 9.
/// </summary>
public interface IApplicationInfo
{
    string ApplicationName { get; }
    string Version { get; }
    string EnvironmentName { get; }
}
