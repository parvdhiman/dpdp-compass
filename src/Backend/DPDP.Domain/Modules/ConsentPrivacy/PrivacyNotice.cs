using DPDP.Domain.Common;
using DPDP.Domain.Modules.DataInventory;

namespace DPDP.Domain.Modules.ConsentPrivacy;

/// <summary>
/// One version of a privacy notice. "Code" identifies which notice this
/// is (e.g. "PRIVACY-POLICY", "COOKIE-NOTICE") across its revisions;
/// "Version" identifies which revision (e.g. "1.0", "2.1"). A
/// ConsentRecord references one specific PrivacyNotice row (a specific
/// version), never just a code — see docs/CONSENT_PRIVACY_OPERATIONS.md
/// section 2.
/// </summary>
public sealed class PrivacyNotice : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public int SequenceNumber { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;

    public PrivacyNoticeStatus Status { get; set; } = PrivacyNoticeStatus.DRAFT;
    public DateOnly? PublishedDate { get; set; }
    public DateOnly? EffectiveDate { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<DataCategory> DataCategories { get; set; } = new List<DataCategory>();
}
