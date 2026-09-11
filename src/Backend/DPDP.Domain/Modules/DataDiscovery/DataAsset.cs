using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.DataDiscovery;

/// <summary>
/// The canonical, current-state record of one discovered table/view/file —
/// upserted across repeated DiscoveryJob runs against the same DataSource
/// (identity = DataSourceId + SchemaName + AssetName), rather than a new
/// row per run. DiscoveryResult is the per-run snapshot; this is "what we
/// currently know". Aggregate root (independently soft-deletable) — see
/// docs/ARCHITECTURE.md Module 8 section.
/// </summary>
public sealed class DataAsset : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public Guid DataSourceId { get; set; }
    public DataSource DataSource { get; set; } = null!;

    public string? DatabaseName { get; set; }
    public string? SchemaName { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public DataAssetType AssetType { get; set; }
    public string? FilePath { get; set; }

    /// <summary>Approximate — sourced from the engine's own statistics (e.g. pg_class.reltuples), never a live COUNT(*) against the customer system.</summary>
    public long? EstimatedRowCount { get; set; }
    public string? IndexesJson { get; set; }

    public DateTimeOffset? LastDiscoveredAt { get; set; }
    public Guid? LastDiscoveryJobId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<DataElement> Elements { get; set; } = new List<DataElement>();
    public ICollection<DiscoveryResult> Results { get; set; } = new List<DiscoveryResult>();
}
