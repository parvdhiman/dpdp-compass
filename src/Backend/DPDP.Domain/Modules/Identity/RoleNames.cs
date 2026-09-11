namespace DPDP.Domain.Modules.Identity;

/// <summary>The fixed set of system roles from MASTER_PROMPT section 6. Not user-creatable in Module 2.</summary>
public static class RoleNames
{
    public const string SuperAdministrator = "Super Administrator";
    public const string OrganisationAdministrator = "Organisation Administrator";
    public const string PrivacyOfficer = "Privacy Officer";
    public const string ComplianceOfficer = "Compliance Officer";
    public const string SecurityOfficer = "Security Officer";
    public const string ItAdministrator = "IT Administrator";
    public const string DepartmentOwner = "Department Owner";
    public const string Auditor = "Auditor";
    public const string Management = "Management";
    public const string ReadOnly = "Read Only";

    public static IReadOnlyList<string> All { get; } =
    [
        SuperAdministrator,
        OrganisationAdministrator,
        PrivacyOfficer,
        ComplianceOfficer,
        SecurityOfficer,
        ItAdministrator,
        DepartmentOwner,
        Auditor,
        Management,
        ReadOnly,
    ];
}
