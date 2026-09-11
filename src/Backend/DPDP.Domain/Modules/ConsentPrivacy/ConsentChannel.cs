namespace DPDP.Domain.Modules.ConsentPrivacy;

/// <summary>
/// How consent was captured — a distinct concept from Module 9's
/// DataCollectionSourceType (where a piece of data was originally
/// collected from). The two can diverge (e.g. consent confirmed by phone
/// for data originally submitted via a web form), so this is its own
/// enum rather than a reuse, even though the value sets overlap.
/// </summary>
public enum ConsentChannel
{
    WEB,
    MOBILE_APP,
    EMAIL,
    PHONE,
    PAPER,
    IN_PERSON,
    API,
    OTHER,
}
