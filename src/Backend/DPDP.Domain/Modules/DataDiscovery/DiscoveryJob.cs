using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.DataDiscovery;

/// <summary>
/// One execution attempt of scanning a DataSource. Child of DataSource
/// (cascade-deleted with it, no independent soft delete) — see
/// docs/ARCHITECTURE.md Module 8 section. CancellationRequested is a
/// durable flag the background worker polls between assets; it is the
/// fallback path if the in-memory cancellation registry
/// (IDiscoveryCancellationRegistry) can't reach an in-flight job, e.g.
/// after an API process restart — see docs/DATA_DISCOVERY.md section 5.
/// </summary>
public sealed class DiscoveryJob : AuditableEntity, ITenantScoped
{
    public Guid OrganisationId { get; set; }
    public Guid DataSourceId { get; set; }
    public DataSource DataSource { get; set; } = null!;

    public DiscoveryJobStatus Status { get; set; } = DiscoveryJobStatus.PENDING;
    public Guid TriggeredByUserId { get; set; }
    public User TriggeredByUser { get; set; } = null!;

    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public bool CancellationRequested { get; set; }

    public int AssetsDiscoveredCount { get; set; }
    public int ElementsDiscoveredCount { get; set; }

    public ICollection<DiscoveryResult> Results { get; set; } = new List<DiscoveryResult>();
}
