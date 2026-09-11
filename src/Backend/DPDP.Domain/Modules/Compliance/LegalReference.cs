using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Compliance;

/// <summary>
/// A specific citable provision within a FrameworkVersion (e.g. "Section 8(5)").
/// SummaryText is always a plain-language paraphrase, never claimed to be
/// verbatim statutory text — see docs/COMPLIANCE_CONTENT_GOVERNANCE.md.
/// Every LegalReference must carry SourceCitation (MASTER_PROMPT: "every
/// legal reference must contain its source").
/// </summary>
public sealed class LegalReference : Entity
{
    public Guid FrameworkVersionId { get; set; }
    public FrameworkVersion FrameworkVersion { get; set; } = null!;

    public string Citation { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Chapter { get; set; }
    public string? SummaryText { get; set; }
    public string SourceCitation { get; set; } = string.Empty;
    public ContentReviewStatus ReviewStatus { get; set; } = ContentReviewStatus.DRAFT;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
}
