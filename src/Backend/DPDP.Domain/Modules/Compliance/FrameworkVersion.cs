using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Compliance;

/// <summary>
/// A specific dated instance of a Framework — the versioning unit
/// MASTER_PROMPT requires ("the framework must be versioned"). Only one
/// version per Framework should be IsCurrent at a time (enforced in
/// ActivateFrameworkVersionCommand, not a DB constraint — see
/// docs/COMPLIANCE_CONTENT_GOVERNANCE.md).
/// </summary>
public sealed class FrameworkVersion : Entity
{
    public Guid FrameworkId { get; set; }
    public Framework Framework { get; set; } = null!;

    public string VersionLabel { get; set; } = string.Empty;
    public string? OfficialCitation { get; set; }
    public DateOnly? PublicationDate { get; set; }

    /// <summary>
    /// Null when commencement is staggered/notification-dependent and not
    /// yet confirmed for every provision — never guess this date. See
    /// docs/COMPLIANCE_CONTENT_GOVERNANCE.md.
    /// </summary>
    public DateOnly? EffectiveDate { get; set; }

    public string? SourceUrl { get; set; }
    public ContentReviewStatus ReviewStatus { get; set; } = ContentReviewStatus.DRAFT;
    public bool IsCurrent { get; set; }
    public string? ChangeSummary { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<LegalReference> LegalReferences { get; set; } = new List<LegalReference>();
}
