namespace DPDP.Application.Modules.Evidence;

/// <summary>Bound from configuration section "Evidence" — see appsettings.json.</summary>
public sealed class EvidenceOptions
{
    public const string SectionName = "Evidence";

    /// <summary>Default 25 MB.</summary>
    public long MaxUploadSizeBytes { get; set; } = 25 * 1024 * 1024;
}
