using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Default role → permission templates. Assessments/Controls/Risk/
/// Findings/Remediation/Evidence/Reports permissions are seeded here (so
/// role templates are complete) even though nothing enforces them yet —
/// see docs/DATABASE.md section 4. Editable afterwards only by a Super
/// Administrator via /api/v1/roles/{id}/permissions.
/// </summary>
public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    private static readonly Dictionary<string, string[]> DefaultAssignments = new()
    {
        [RoleNames.SuperAdministrator] = PermissionKeys.All.ToArray(),

        [RoleNames.OrganisationAdministrator] =
        [
            PermissionKeys.OrganisationRead, PermissionKeys.OrganisationWrite,
            PermissionKeys.UsersRead, PermissionKeys.UsersCreate, PermissionKeys.UsersUpdate, PermissionKeys.UsersDisable,
            PermissionKeys.RolesRead, PermissionKeys.RolesManage,
            PermissionKeys.BusinessUnitsRead, PermissionKeys.BusinessUnitsCreate, PermissionKeys.BusinessUnitsUpdate, PermissionKeys.BusinessUnitsDelete,
            PermissionKeys.DepartmentsRead, PermissionKeys.DepartmentsCreate, PermissionKeys.DepartmentsUpdate, PermissionKeys.DepartmentsDelete,
            PermissionKeys.AssessmentsRead, PermissionKeys.ControlsRead, PermissionKeys.RisksRead,
            PermissionKeys.FindingsRead, PermissionKeys.RemediationRead, PermissionKeys.EvidenceRead,
            PermissionKeys.ReportsRead, PermissionKeys.ReportsGenerate, PermissionKeys.AuditRead,
            PermissionKeys.DataSourcesRead, PermissionKeys.DiscoveryJobsRead, PermissionKeys.DataAssetsRead,
            PermissionKeys.DataInventoryRead, PermissionKeys.ProcessingActivitiesRead, PermissionKeys.DataFlowsRead,
            PermissionKeys.PrivacyNoticesRead, PermissionKeys.ConsentPurposesRead, PermissionKeys.ConsentRead,
            PermissionKeys.DataPrincipalsRead, PermissionKeys.DataRequestsRead,
        ],

        [RoleNames.PrivacyOfficer] =
        [
            PermissionKeys.OrganisationRead, PermissionKeys.UsersRead, PermissionKeys.RolesRead,
            PermissionKeys.BusinessUnitsRead, PermissionKeys.DepartmentsRead,
            PermissionKeys.AssessmentsRead, PermissionKeys.AssessmentsCreate,
            PermissionKeys.ControlsRead, PermissionKeys.RisksRead,
            PermissionKeys.FindingsRead, PermissionKeys.FindingsCreate,
            PermissionKeys.RemediationRead, PermissionKeys.EvidenceRead, PermissionKeys.EvidenceUpload,
            PermissionKeys.ReportsRead, PermissionKeys.ReportsGenerate, PermissionKeys.AuditRead,
            PermissionKeys.DataSourcesRead, PermissionKeys.DataSourcesManage,
            PermissionKeys.DiscoveryJobsRead, PermissionKeys.DiscoveryJobsManage,
            PermissionKeys.DataAssetsRead, PermissionKeys.ClassificationReview,
            PermissionKeys.DataInventoryRead, PermissionKeys.DataInventoryManage,
            PermissionKeys.ProcessingActivitiesRead, PermissionKeys.ProcessingActivitiesManage,
            PermissionKeys.DataFlowsRead, PermissionKeys.DataFlowsManage,
            PermissionKeys.PrivacyNoticesRead, PermissionKeys.PrivacyNoticesManage,
            PermissionKeys.ConsentPurposesRead, PermissionKeys.ConsentPurposesManage,
            PermissionKeys.ConsentRead, PermissionKeys.ConsentManage,
            PermissionKeys.DataPrincipalsRead, PermissionKeys.DataPrincipalsManage,
            PermissionKeys.DataRequestsRead, PermissionKeys.DataRequestsManage,
        ],

        [RoleNames.ComplianceOfficer] =
        [
            PermissionKeys.OrganisationRead, PermissionKeys.UsersRead, PermissionKeys.RolesRead,
            PermissionKeys.BusinessUnitsRead, PermissionKeys.DepartmentsRead,
            PermissionKeys.AssessmentsRead, PermissionKeys.AssessmentsCreate, PermissionKeys.AssessmentsReview, PermissionKeys.AssessmentsApprove,
            PermissionKeys.ControlsRead, PermissionKeys.ControlsManage,
            PermissionKeys.RisksRead, PermissionKeys.RisksManage,
            PermissionKeys.FindingsRead, PermissionKeys.FindingsCreate, PermissionKeys.FindingsAssign, PermissionKeys.FindingsClose,
            PermissionKeys.RemediationRead, PermissionKeys.RemediationManage,
            PermissionKeys.EvidenceRead, PermissionKeys.EvidenceReview,
            PermissionKeys.ReportsRead, PermissionKeys.ReportsGenerate, PermissionKeys.AuditRead,
            PermissionKeys.DataSourcesRead, PermissionKeys.DiscoveryJobsRead,
            PermissionKeys.DataAssetsRead, PermissionKeys.ClassificationReview,
            PermissionKeys.DataInventoryRead, PermissionKeys.ProcessingActivitiesRead,
            PermissionKeys.ProcessingActivitiesReview, PermissionKeys.ProcessingActivitiesApprove,
            PermissionKeys.DataFlowsRead,
            PermissionKeys.PrivacyNoticesRead, PermissionKeys.PrivacyNoticesApprove,
            PermissionKeys.ConsentPurposesRead, PermissionKeys.ConsentRead,
            PermissionKeys.DataPrincipalsRead, PermissionKeys.DataRequestsRead,
        ],

        [RoleNames.SecurityOfficer] =
        [
            PermissionKeys.OrganisationRead, PermissionKeys.UsersRead, PermissionKeys.RolesRead,
            PermissionKeys.BusinessUnitsRead, PermissionKeys.DepartmentsRead,
            PermissionKeys.ControlsRead, PermissionKeys.ControlsManage,
            PermissionKeys.RisksRead, PermissionKeys.RisksManage,
            PermissionKeys.FindingsRead, PermissionKeys.FindingsCreate, PermissionKeys.FindingsAssign, PermissionKeys.FindingsClose,
            PermissionKeys.RemediationRead, PermissionKeys.RemediationManage,
            PermissionKeys.EvidenceRead, PermissionKeys.EvidenceReview,
            PermissionKeys.ReportsRead, PermissionKeys.AuditRead,
            PermissionKeys.DataSourcesRead, PermissionKeys.DataSourcesManage,
            PermissionKeys.DiscoveryJobsRead, PermissionKeys.DiscoveryJobsManage, PermissionKeys.DataAssetsRead,
            PermissionKeys.DataInventoryRead, PermissionKeys.ProcessingActivitiesRead, PermissionKeys.DataFlowsRead,
            PermissionKeys.PrivacyNoticesRead, PermissionKeys.ConsentPurposesRead, PermissionKeys.ConsentRead,
            PermissionKeys.DataPrincipalsRead, PermissionKeys.DataRequestsRead,
        ],

        [RoleNames.ItAdministrator] =
        [
            PermissionKeys.OrganisationRead,
            PermissionKeys.UsersRead, PermissionKeys.UsersCreate, PermissionKeys.UsersUpdate, PermissionKeys.UsersDisable,
            PermissionKeys.RolesRead, PermissionKeys.BusinessUnitsRead, PermissionKeys.DepartmentsRead, PermissionKeys.ControlsRead,
            PermissionKeys.EvidenceRead, PermissionKeys.EvidenceUpload, PermissionKeys.AuditRead,
            PermissionKeys.DataSourcesRead, PermissionKeys.DataSourcesManage,
            PermissionKeys.DiscoveryJobsRead, PermissionKeys.DiscoveryJobsManage, PermissionKeys.DataAssetsRead,
            PermissionKeys.DataInventoryRead, PermissionKeys.ProcessingActivitiesRead, PermissionKeys.DataFlowsRead,
        ],

        [RoleNames.DepartmentOwner] =
        [
            PermissionKeys.OrganisationRead, PermissionKeys.UsersRead,
            PermissionKeys.BusinessUnitsRead, PermissionKeys.DepartmentsRead,
            PermissionKeys.AssessmentsRead, PermissionKeys.FindingsRead, PermissionKeys.RemediationRead,
            PermissionKeys.EvidenceRead, PermissionKeys.EvidenceUpload, PermissionKeys.ReportsRead,
            PermissionKeys.DataAssetsRead,
            PermissionKeys.DataInventoryRead, PermissionKeys.ProcessingActivitiesRead,
        ],

        [RoleNames.Auditor] =
        [
            PermissionKeys.OrganisationRead, PermissionKeys.UsersRead, PermissionKeys.RolesRead,
            PermissionKeys.BusinessUnitsRead, PermissionKeys.DepartmentsRead,
            PermissionKeys.AssessmentsRead, PermissionKeys.ControlsRead, PermissionKeys.RisksRead,
            PermissionKeys.FindingsRead, PermissionKeys.RemediationRead, PermissionKeys.EvidenceRead,
            PermissionKeys.ReportsRead, PermissionKeys.AuditRead,
            PermissionKeys.DataSourcesRead, PermissionKeys.DiscoveryJobsRead, PermissionKeys.DataAssetsRead,
            PermissionKeys.DataInventoryRead, PermissionKeys.ProcessingActivitiesRead, PermissionKeys.DataFlowsRead,
            PermissionKeys.PrivacyNoticesRead, PermissionKeys.ConsentPurposesRead, PermissionKeys.ConsentRead,
            PermissionKeys.DataPrincipalsRead, PermissionKeys.DataRequestsRead,
        ],

        [RoleNames.Management] =
        [
            PermissionKeys.OrganisationRead, PermissionKeys.BusinessUnitsRead, PermissionKeys.DepartmentsRead,
            PermissionKeys.AssessmentsRead, PermissionKeys.RisksRead,
            PermissionKeys.FindingsRead, PermissionKeys.ReportsRead, PermissionKeys.ReportsGenerate,
            PermissionKeys.AuditRead, PermissionKeys.DataAssetsRead,
            PermissionKeys.ProcessingActivitiesRead, PermissionKeys.DataRequestsRead,
        ],

        [RoleNames.ReadOnly] =
        [
            PermissionKeys.OrganisationRead, PermissionKeys.UsersRead, PermissionKeys.RolesRead,
            PermissionKeys.BusinessUnitsRead, PermissionKeys.DepartmentsRead,
            PermissionKeys.AssessmentsRead, PermissionKeys.ControlsRead, PermissionKeys.RisksRead,
            PermissionKeys.FindingsRead, PermissionKeys.RemediationRead, PermissionKeys.EvidenceRead,
            PermissionKeys.ReportsRead, PermissionKeys.AuditRead,
            PermissionKeys.DataSourcesRead, PermissionKeys.DiscoveryJobsRead, PermissionKeys.DataAssetsRead,
            PermissionKeys.DataInventoryRead, PermissionKeys.ProcessingActivitiesRead, PermissionKeys.DataFlowsRead,
            PermissionKeys.PrivacyNoticesRead, PermissionKeys.ConsentPurposesRead, PermissionKeys.ConsentRead,
            PermissionKeys.DataPrincipalsRead, PermissionKeys.DataRequestsRead,
        ],
    };

    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(DefaultAssignments.SelectMany(kvp => kvp.Value.Select(permissionKey => new RolePermission
        {
            RoleId = RoleConfiguration.RoleId(kvp.Key),
            PermissionId = DeterministicGuid.Create($"permission:{permissionKey}"),
        })));
    }
}
