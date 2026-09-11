using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Compliance;

/// <summary>An interpretive compliance requirement derived from a LegalReference — what an organisation must do to comply with it.</summary>
public sealed class Requirement : Entity
{
    public Guid LegalReferenceId { get; set; }
    public LegalReference LegalReference { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ContentReviewStatus ReviewStatus { get; set; } = ContentReviewStatus.DRAFT;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<ControlMapping> ControlMappings { get; set; } = new List<ControlMapping>();
}
