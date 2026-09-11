using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Evidence;

/// <summary>An append-only approve/reject decision log — never edited or deleted. Multiple rows accumulate across upload/reject/re-upload cycles.</summary>
public sealed class EvidenceReviewRecord : Entity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid EvidenceItemId { get; set; }
    public EvidenceItem EvidenceItem { get; set; } = null!;

    public int EvidenceVersionNumber { get; set; }

    public Guid ReviewerUserId { get; set; }
    public User Reviewer { get; set; } = null!;

    public EvidenceReviewDecision Decision { get; set; }
    public string? Comments { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
