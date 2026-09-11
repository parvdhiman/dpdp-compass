namespace DPDP.Application.Modules.DataDiscovery;

/// <summary>Bound from configuration section "DataDiscovery" — see appsettings.json and docs/DATA_DISCOVERY.md section 3.</summary>
public sealed class DiscoveryOptions
{
    public const string SectionName = "DataDiscovery";

    /// <summary>Rows fetched (via LIMIT/TOP, never unbounded) per table purely to derive masked samples — never persisted raw, never logged.</summary>
    public int SampleRowLimit { get; set; } = 5;

    /// <summary>Hard ceiling on assets scanned in a single job, so a very large customer schema can't turn one discovery run into an unbounded operation.</summary>
    public int MaxAssetsPerJob { get; set; } = 500;

    /// <summary>How many PENDING/RUNNING jobs one organisation may have at once.</summary>
    public int MaxConcurrentJobsPerOrganisation { get; set; } = 3;
}
