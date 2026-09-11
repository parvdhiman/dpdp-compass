using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Audit;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.ConsentPrivacy;
using DPDP.Domain.Modules.DataDiscovery;
using DPDP.Domain.Modules.DataInventory;
using DPDP.Domain.Modules.Evidence;
using DPDP.Domain.Modules.Findings;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using DPDP.Domain.Modules.Remediation;
using DPDP.Domain.Modules.Risks;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// The Application layer's view of the database — deliberately not the
/// concrete DpdpDbContext, so Application never references Infrastructure
/// (Clean Architecture boundary, docs/ARCHITECTURE.md section 1).
/// </summary>
public interface IAppDbContext
{
    DbSet<Organisation> Organisations { get; }
    DbSet<OrganisationLocation> OrganisationLocations { get; }
    DbSet<BusinessUnit> BusinessUnits { get; }
    DbSet<Department> Departments { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<LoginHistory> LoginHistories { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Framework> Frameworks { get; }
    DbSet<FrameworkVersion> FrameworkVersions { get; }
    DbSet<LegalReference> LegalReferences { get; }
    DbSet<Requirement> Requirements { get; }
    DbSet<ControlCategory> ControlCategories { get; }
    DbSet<Control> Controls { get; }
    DbSet<ControlMapping> ControlMappings { get; }
    DbSet<AssessmentQuestion> AssessmentQuestions { get; }
    DbSet<EvidenceRequirement> EvidenceRequirements { get; }

    DbSet<Assessment> Assessments { get; }
    DbSet<AssessmentScope> AssessmentScopes { get; }
    DbSet<AssessmentControl> AssessmentControls { get; }
    DbSet<AssessmentControlQuestion> AssessmentControlQuestions { get; }
    DbSet<AssessmentAnswer> AssessmentAnswers { get; }
    DbSet<AssessmentReview> AssessmentReviews { get; }
    DbSet<AssessmentApproval> AssessmentApprovals { get; }

    DbSet<Finding> Findings { get; }
    DbSet<Risk> Risks { get; }
    DbSet<RemediationTask> RemediationTasks { get; }
    DbSet<RemediationComment> RemediationComments { get; }

    DbSet<EvidenceItem> EvidenceItems { get; }
    DbSet<EvidenceVersion> EvidenceVersions { get; }
    DbSet<EvidenceReviewRecord> EvidenceReviewRecords { get; }

    DbSet<DataSource> DataSources { get; }
    DbSet<DiscoveryJob> DiscoveryJobs { get; }
    DbSet<DiscoveryResult> DiscoveryResults { get; }
    DbSet<DataAsset> DataAssets { get; }
    DbSet<DataElement> DataElements { get; }

    DbSet<DataCategory> DataCategories { get; }
    DbSet<ItSystem> ItSystems { get; }
    DbSet<DataCollectionSource> DataCollectionSources { get; }
    DbSet<Processor> Processors { get; }
    DbSet<Recipient> Recipients { get; }
    DbSet<RetentionPolicy> RetentionPolicies { get; }
    DbSet<DataInventoryItem> DataInventoryItems { get; }
    DbSet<ProcessingActivity> ProcessingActivities { get; }
    DbSet<DataFlow> DataFlows { get; }

    DbSet<DataPrincipal> DataPrincipals { get; }
    DbSet<ConsentPurpose> ConsentPurposes { get; }
    DbSet<PrivacyNotice> PrivacyNotices { get; }
    DbSet<ConsentRecord> ConsentRecords { get; }
    DbSet<SlaPolicy> SlaPolicies { get; }
    DbSet<DataPrincipalRequest> DataPrincipalRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
