using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.DataDiscovery;

/// <summary>
/// A discovered column (or, for a non-tabular file asset, its single
/// opaque "content" element) — the unit classification operates on.
/// SampleMaskedValue is the only sample data ever persisted, and it is
/// always already masked by SampleMasker before it reaches this entity;
/// no code path stores a raw value here. Child of DataAsset (cascade
/// deleted with it) — see docs/ARCHITECTURE.md Module 8 section and
/// docs/DATA_DISCOVERY.md.
/// </summary>
public sealed class DataElement : AuditableEntity, ITenantScoped
{
    public Guid OrganisationId { get; set; }
    public Guid DataAssetId { get; set; }
    public DataAsset DataAsset { get; set; } = null!;

    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public int OrdinalPosition { get; set; }
    public string? SampleMaskedValue { get; set; }

    public ClassificationCategory? ClassificationCategory { get; set; }
    public decimal? ClassificationConfidence { get; set; }
    public ClassificationSource? ClassificationSource { get; set; }
    public bool IsHumanCorrected { get; set; }
    public Guid? CorrectedByUserId { get; set; }
    public User? CorrectedByUser { get; set; }
    public DateTimeOffset? CorrectedAt { get; set; }

    public DateTimeOffset? LastDiscoveredAt { get; set; }
}
