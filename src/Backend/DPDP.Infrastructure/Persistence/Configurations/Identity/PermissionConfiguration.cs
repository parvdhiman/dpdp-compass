using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Identity;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    // (key, module, description)
    private static readonly (string Key, string Module, string Description)[] Catalogue =
    [
        (PermissionKeys.OrganisationRead, "Organisations", "View organisation details."),
        (PermissionKeys.OrganisationWrite, "Organisations", "Edit organisation details."),
        (PermissionKeys.UsersRead, "Identity", "View users."),
        (PermissionKeys.UsersCreate, "Identity", "Create users."),
        (PermissionKeys.UsersUpdate, "Identity", "Edit user profile details."),
        (PermissionKeys.UsersDisable, "Identity", "Activate or deactivate user accounts."),
        (PermissionKeys.RolesRead, "Identity", "View roles and their permissions."),
        (PermissionKeys.RolesManage, "Identity", "Assign roles to users."),
        (PermissionKeys.BusinessUnitsRead, "Organisations", "View business units."),
        (PermissionKeys.BusinessUnitsCreate, "Organisations", "Create business units."),
        (PermissionKeys.BusinessUnitsUpdate, "Organisations", "Edit business units."),
        (PermissionKeys.BusinessUnitsDelete, "Organisations", "Delete business units."),
        (PermissionKeys.DepartmentsRead, "Organisations", "View departments."),
        (PermissionKeys.DepartmentsCreate, "Organisations", "Create departments."),
        (PermissionKeys.DepartmentsUpdate, "Organisations", "Edit departments."),
        (PermissionKeys.DepartmentsDelete, "Organisations", "Delete departments."),
        (PermissionKeys.AssessmentsRead, "Assessments", "View compliance assessments."),
        (PermissionKeys.AssessmentsCreate, "Assessments", "Create compliance assessments."),
        (PermissionKeys.AssessmentsReview, "Assessments", "Review submitted compliance assessments."),
        (PermissionKeys.AssessmentsApprove, "Assessments", "Approve compliance assessments."),
        (PermissionKeys.ControlsRead, "Controls", "View the DPDP control library."),
        (PermissionKeys.ControlsManage, "Controls", "Manage the DPDP control library."),
        (PermissionKeys.RisksRead, "Risk", "View the risk register."),
        (PermissionKeys.RisksManage, "Risk", "Manage the risk register."),
        (PermissionKeys.FindingsRead, "Findings", "View findings."),
        (PermissionKeys.FindingsCreate, "Findings", "Create findings."),
        (PermissionKeys.FindingsAssign, "Findings", "Assign findings."),
        (PermissionKeys.FindingsClose, "Findings", "Close findings."),
        (PermissionKeys.RemediationRead, "Remediation", "View remediation tasks."),
        (PermissionKeys.RemediationManage, "Remediation", "Manage remediation tasks."),
        (PermissionKeys.EvidenceRead, "Evidence", "View evidence."),
        (PermissionKeys.EvidenceUpload, "Evidence", "Upload evidence."),
        (PermissionKeys.EvidenceReview, "Evidence", "Review and approve evidence."),
        (PermissionKeys.ReportsRead, "Reports", "View reports."),
        (PermissionKeys.ReportsGenerate, "Reports", "Generate reports."),
        (PermissionKeys.AuditRead, "Audit", "View the audit trail."),
        (PermissionKeys.DataSourcesRead, "DataDiscovery", "View registered data sources."),
        (PermissionKeys.DataSourcesManage, "DataDiscovery", "Register, edit, test, and remove data sources."),
        (PermissionKeys.DiscoveryJobsRead, "DataDiscovery", "View discovery jobs."),
        (PermissionKeys.DiscoveryJobsManage, "DataDiscovery", "Start and cancel discovery jobs."),
        (PermissionKeys.DataAssetsRead, "DataDiscovery", "View discovered data assets and elements."),
        (PermissionKeys.ClassificationReview, "DataDiscovery", "Correct a data element's classification."),
        (PermissionKeys.DataInventoryRead, "DataInventory", "View the data inventory and its catalogues (categories, systems, sources, processors, recipients, retention policies)."),
        (PermissionKeys.DataInventoryManage, "DataInventory", "Create and edit data inventory items and their catalogues."),
        (PermissionKeys.ProcessingActivitiesRead, "DataInventory", "View the processing activity register."),
        (PermissionKeys.ProcessingActivitiesManage, "DataInventory", "Create and edit processing activities."),
        (PermissionKeys.ProcessingActivitiesReview, "DataInventory", "Review a submitted processing activity."),
        (PermissionKeys.ProcessingActivitiesApprove, "DataInventory", "Approve a reviewed processing activity."),
        (PermissionKeys.DataFlowsRead, "DataInventory", "View data flow metadata."),
        (PermissionKeys.DataFlowsManage, "DataInventory", "Create and edit data flow metadata."),
        (PermissionKeys.PrivacyNoticesRead, "ConsentPrivacy", "View privacy notices."),
        (PermissionKeys.PrivacyNoticesManage, "ConsentPrivacy", "Create, edit, publish, and archive privacy notices."),
        (PermissionKeys.PrivacyNoticesApprove, "ConsentPrivacy", "Approve a privacy notice before publication."),
        (PermissionKeys.ConsentPurposesRead, "ConsentPrivacy", "View consent purposes."),
        (PermissionKeys.ConsentPurposesManage, "ConsentPrivacy", "Create and edit consent purposes."),
        (PermissionKeys.ConsentRead, "ConsentPrivacy", "View consent records."),
        (PermissionKeys.ConsentManage, "ConsentPrivacy", "Capture, withdraw, and revoke consent records."),
        (PermissionKeys.DataPrincipalsRead, "ConsentPrivacy", "View data principal references."),
        (PermissionKeys.DataPrincipalsManage, "ConsentPrivacy", "Create and edit data principal references."),
        (PermissionKeys.DataRequestsRead, "ConsentPrivacy", "View data principal requests and grievances."),
        (PermissionKeys.DataRequestsManage, "ConsentPrivacy", "Manage data principal requests and grievances, including SLA policies."),
    ];

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Key).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Module).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.Key).IsUnique();

        builder.HasData(Catalogue.Select(p => new Permission
        {
            Id = DeterministicGuid.Create($"permission:{p.Key}"),
            Key = p.Key,
            Description = p.Description,
            Module = p.Module,
            CreatedAt = SeedClock.Timestamp,
        }));
    }
}
