using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Compliance;

/// <summary>
/// A body of law/regulation (e.g. "Digital Personal Data Protection Act,
/// 2023"). Global reference data, shared by every tenant — not
/// ITenantScoped. See docs/DATABASE.md and docs/COMPLIANCE_CONTENT_GOVERNANCE.md.
/// </summary>
public sealed class Framework : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    public string IssuingAuthority { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<FrameworkVersion> Versions { get; set; } = new List<FrameworkVersion>();
}
