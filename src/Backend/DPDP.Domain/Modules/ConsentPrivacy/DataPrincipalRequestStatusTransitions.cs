namespace DPDP.Domain.Modules.ConsentPrivacy;

public static class DataPrincipalRequestStatusTransitions
{
    private static readonly Dictionary<DataPrincipalRequestStatus, DataPrincipalRequestStatus[]> Allowed = new()
    {
        [DataPrincipalRequestStatus.REQUESTED] = [DataPrincipalRequestStatus.IDENTITY_VERIFICATION, DataPrincipalRequestStatus.REJECTED],
        [DataPrincipalRequestStatus.IDENTITY_VERIFICATION] = [DataPrincipalRequestStatus.IN_PROGRESS, DataPrincipalRequestStatus.REJECTED],
        [DataPrincipalRequestStatus.IN_PROGRESS] = [DataPrincipalRequestStatus.AWAITING_INFORMATION, DataPrincipalRequestStatus.COMPLETED, DataPrincipalRequestStatus.REJECTED],
        [DataPrincipalRequestStatus.AWAITING_INFORMATION] = [DataPrincipalRequestStatus.IN_PROGRESS, DataPrincipalRequestStatus.REJECTED],
        [DataPrincipalRequestStatus.COMPLETED] = [DataPrincipalRequestStatus.CLOSED],
        [DataPrincipalRequestStatus.REJECTED] = [DataPrincipalRequestStatus.CLOSED],
        [DataPrincipalRequestStatus.CLOSED] = [],
    };

    public static bool CanTransition(DataPrincipalRequestStatus from, DataPrincipalRequestStatus to) =>
        from == to || (Allowed.TryGetValue(from, out var targets) && targets.Contains(to));

    public static bool IsTerminal(DataPrincipalRequestStatus status) => status == DataPrincipalRequestStatus.CLOSED;
}
