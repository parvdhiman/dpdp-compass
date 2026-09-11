using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.ConsentPrivacy;

/// <summary>
/// Covers both Data Principal Requests and Grievances — the brief gives
/// them one identical workflow and field shape, so "Grievance" is
/// RequestType.GRIEVANCE on this entity, not a separate one. See
/// docs/CONSENT_PRIVACY_OPERATIONS.md section 4.
///
/// RequesterName/RequesterContactEmail/RequesterContactPhone are the
/// identity claimed at intake — necessary to process and correspond
/// about *this specific request*, not a duplicate PII database (they are
/// retained only for the request's own lifecycle, per whatever retention
/// policy the organisation applies). DataPrincipalId is set once that
/// claim is verified and matched to a known DataPrincipal reference.
/// </summary>
public sealed class DataPrincipalRequest : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public int SequenceNumber { get; set; }

    public DataPrincipalRequestType RequestType { get; set; }
    public DataPrincipalRequestStatus Status { get; set; } = DataPrincipalRequestStatus.REQUESTED;

    public string RequesterName { get; set; } = string.Empty;
    public string? RequesterContactEmail { get; set; }
    public string? RequesterContactPhone { get; set; }
    public string? ExternalReferenceId { get; set; }

    public Guid? DataPrincipalId { get; set; }
    public DataPrincipal? DataPrincipal { get; set; }
    public Guid? RelatedConsentId { get; set; }
    public ConsentRecord? RelatedConsent { get; set; }

    public string? Description { get; set; }

    public Guid? SlaPolicyId { get; set; }
    public SlaPolicy? SlaPolicy { get; set; }
    public DateTimeOffset? DueAt { get; set; }

    public DateTimeOffset? IdentityVerifiedAt { get; set; }
    public Guid? IdentityVerifiedBy { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
