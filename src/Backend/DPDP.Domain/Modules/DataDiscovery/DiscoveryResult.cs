using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.DataDiscovery;

/// <summary>
/// A per-run, per-asset snapshot: what a specific DiscoveryJob observed
/// about a specific DataAsset. Append-only history alongside DataAsset's
/// "current state" — see docs/ARCHITECTURE.md Module 8 section. Child of
/// both DiscoveryJob and DataAsset (cascade-deleted with either).
/// </summary>
public sealed class DiscoveryResult : Entity, ITenantScoped
{
    public Guid OrganisationId { get; set; }
    public Guid DiscoveryJobId { get; set; }
    public DiscoveryJob DiscoveryJob { get; set; } = null!;
    public Guid DataAssetId { get; set; }
    public DataAsset DataAsset { get; set; } = null!;

    public long? RowCountAtScan { get; set; }
    public int ColumnsDiscovered { get; set; }
    public string? IndexesJson { get; set; }
    public DateTimeOffset ScannedAt { get; set; }
}
