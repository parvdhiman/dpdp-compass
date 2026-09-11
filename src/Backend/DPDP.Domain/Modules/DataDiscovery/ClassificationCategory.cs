namespace DPDP.Domain.Modules.DataDiscovery;

/// <summary>
/// Informational categorization of a discovered column, never a legal
/// determination — see docs/DATA_DISCOVERY.md. A null
/// DataElement.ClassificationCategory means "not classified as personal
/// data", not "unknown"; OTHER_PERSONAL_DATA is the catch-all for a column
/// the classifier believes is personal data but cannot place more
/// specifically.
/// </summary>
public enum ClassificationCategory
{
    IDENTIFIER,
    CONTACT,
    ADDRESS,
    FINANCIAL,
    IDENTITY,
    EMPLOYEE,
    CUSTOMER,
    CHILD,
    HEALTH_RELATED,
    LOCATION,
    OTHER_PERSONAL_DATA,
}
