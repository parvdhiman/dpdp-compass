using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.ConsentPrivacy;

/// <summary>
/// Deliberately NOT ISoftDeletable — a consent record is compliance
/// evidence, the same "append-only, state changes via dedicated actions,
/// never deleted" shape as AuditLog. Its lifecycle only ever moves
/// forward via CreateConsentCommand → Withdraw/Revoke/MarkExpired, never
/// a delete endpoint. See docs/CONSENT_PRIVACY_OPERATIONS.md section 3.
/// </summary>
public sealed class ConsentRecord : AuditableEntity, ITenantScoped
{
    public Guid OrganisationId { get; set; }
    public int SequenceNumber { get; set; }

    public Guid DataPrincipalId { get; set; }
    public DataPrincipal DataPrincipal { get; set; } = null!;
    public Guid ConsentPurposeId { get; set; }
    public ConsentPurpose ConsentPurpose { get; set; } = null!;
    public Guid? NoticeVersionId { get; set; }
    public PrivacyNotice? NoticeVersion { get; set; }

    public DateTimeOffset GrantedAt { get; set; }
    public ConsentChannel Channel { get; set; }
    public ConsentStatus Status { get; set; } = ConsentStatus.GRANTED;
    public DateTimeOffset? WithdrawnAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string? SourceSystem { get; set; }
    public string? ExternalReferenceId { get; set; }
}
