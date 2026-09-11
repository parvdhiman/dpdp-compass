namespace DPDP.Domain.Modules.ConsentPrivacy;

public enum DataPrincipalRequestStatus
{
    REQUESTED,
    IDENTITY_VERIFICATION,
    IN_PROGRESS,
    AWAITING_INFORMATION,
    COMPLETED,
    REJECTED,
    CLOSED,
}
