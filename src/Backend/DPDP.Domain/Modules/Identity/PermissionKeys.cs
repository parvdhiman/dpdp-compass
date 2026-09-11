namespace DPDP.Domain.Modules.Identity;

/// <summary>
/// The full permission catalogue. Only the Identity/Organisation/Roles keys
/// are enforced by any endpoint in Module 2 — the rest are seeded now so
/// role templates are complete and stable for the modules that will
/// enforce them later (per docs/DATABASE.md section 4). Never check a role
/// name in authorization code — always check one of these keys.
/// </summary>
public static class PermissionKeys
{
    public const string OrganisationRead = "organisation.read";
    public const string OrganisationWrite = "organisation.write";

    public const string UsersRead = "users.read";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDisable = "users.disable";

    public const string RolesRead = "roles.read";
    public const string RolesManage = "roles.manage";

    public const string BusinessUnitsRead = "businessunits.read";
    public const string BusinessUnitsCreate = "businessunits.create";
    public const string BusinessUnitsUpdate = "businessunits.update";
    public const string BusinessUnitsDelete = "businessunits.delete";

    public const string DepartmentsRead = "departments.read";
    public const string DepartmentsCreate = "departments.create";
    public const string DepartmentsUpdate = "departments.update";
    public const string DepartmentsDelete = "departments.delete";

    // Not enforced by any endpoint yet — seeded for future modules.
    public const string AssessmentsRead = "assessments.read";
    public const string AssessmentsCreate = "assessments.create";
    public const string AssessmentsReview = "assessments.review";
    public const string AssessmentsApprove = "assessments.approve";
    public const string ControlsRead = "controls.read";
    public const string ControlsManage = "controls.manage";
    public const string RisksRead = "risks.read";
    public const string RisksManage = "risks.manage";
    public const string FindingsRead = "findings.read";
    public const string FindingsCreate = "findings.create";
    public const string FindingsAssign = "findings.assign";
    public const string FindingsClose = "findings.close";
    public const string RemediationRead = "remediation.read";
    public const string RemediationManage = "remediation.manage";
    public const string EvidenceRead = "evidence.read";
    public const string EvidenceUpload = "evidence.upload";
    public const string EvidenceReview = "evidence.review";
    public const string ReportsRead = "reports.read";
    public const string ReportsGenerate = "reports.generate";
    public const string AuditRead = "audit.read";

    public const string DataSourcesRead = "datasources.read";
    public const string DataSourcesManage = "datasources.manage";
    public const string DiscoveryJobsRead = "discoveryjobs.read";
    public const string DiscoveryJobsManage = "discoveryjobs.manage";
    public const string DataAssetsRead = "dataassets.read";
    public const string ClassificationReview = "classification.review";

    public const string DataInventoryRead = "datainventory.read";
    public const string DataInventoryManage = "datainventory.manage";
    public const string ProcessingActivitiesRead = "processingactivities.read";
    public const string ProcessingActivitiesManage = "processingactivities.manage";
    public const string ProcessingActivitiesReview = "processingactivities.review";
    public const string ProcessingActivitiesApprove = "processingactivities.approve";
    public const string DataFlowsRead = "dataflows.read";
    public const string DataFlowsManage = "dataflows.manage";

    public const string PrivacyNoticesRead = "privacynotices.read";
    public const string PrivacyNoticesManage = "privacynotices.manage";
    public const string PrivacyNoticesApprove = "privacynotices.approve";
    public const string ConsentPurposesRead = "consentpurposes.read";
    public const string ConsentPurposesManage = "consentpurposes.manage";
    public const string ConsentRead = "consent.read";
    public const string ConsentManage = "consent.manage";
    public const string DataPrincipalsRead = "dataprincipals.read";
    public const string DataPrincipalsManage = "dataprincipals.manage";
    public const string DataRequestsRead = "datarequests.read";
    public const string DataRequestsManage = "datarequests.manage";

    public static IReadOnlyList<string> All { get; } =
    [
        OrganisationRead, OrganisationWrite,
        UsersRead, UsersCreate, UsersUpdate, UsersDisable,
        RolesRead, RolesManage,
        BusinessUnitsRead, BusinessUnitsCreate, BusinessUnitsUpdate, BusinessUnitsDelete,
        DepartmentsRead, DepartmentsCreate, DepartmentsUpdate, DepartmentsDelete,
        AssessmentsRead, AssessmentsCreate, AssessmentsReview, AssessmentsApprove,
        ControlsRead, ControlsManage,
        RisksRead, RisksManage,
        FindingsRead, FindingsCreate, FindingsAssign, FindingsClose,
        RemediationRead, RemediationManage,
        EvidenceRead, EvidenceUpload, EvidenceReview,
        ReportsRead, ReportsGenerate,
        AuditRead,
        DataSourcesRead, DataSourcesManage, DiscoveryJobsRead, DiscoveryJobsManage, DataAssetsRead, ClassificationReview,
        DataInventoryRead, DataInventoryManage,
        ProcessingActivitiesRead, ProcessingActivitiesManage, ProcessingActivitiesReview, ProcessingActivitiesApprove,
        DataFlowsRead, DataFlowsManage,
        PrivacyNoticesRead, PrivacyNoticesManage, PrivacyNoticesApprove,
        ConsentPurposesRead, ConsentPurposesManage,
        ConsentRead, ConsentManage,
        DataPrincipalsRead, DataPrincipalsManage,
        DataRequestsRead, DataRequestsManage,
    ];
}
