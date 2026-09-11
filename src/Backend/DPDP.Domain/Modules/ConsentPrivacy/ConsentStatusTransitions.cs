namespace DPDP.Domain.Modules.ConsentPrivacy;

public static class ConsentStatusTransitions
{
    private static readonly Dictionary<ConsentStatus, ConsentStatus[]> Allowed = new()
    {
        [ConsentStatus.GRANTED] = [ConsentStatus.WITHDRAWN, ConsentStatus.EXPIRED, ConsentStatus.REVOKED],
        [ConsentStatus.WITHDRAWN] = [],
        [ConsentStatus.EXPIRED] = [],
        [ConsentStatus.REVOKED] = [],
    };

    public static bool CanTransition(ConsentStatus from, ConsentStatus to) =>
        from == to || (Allowed.TryGetValue(from, out var targets) && targets.Contains(to));

    public static bool IsTerminal(ConsentStatus status) => status != ConsentStatus.GRANTED;
}
