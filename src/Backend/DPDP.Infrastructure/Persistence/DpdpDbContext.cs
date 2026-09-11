using System.Reflection;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Common;
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

namespace DPDP.Infrastructure.Persistence;

/// <summary>
/// Single physical DbContext for the whole modular monolith. Each module
/// contributes its own IEntityTypeConfiguration&lt;T&gt; classes, applied
/// below by assembly scan — no module registers itself elsewhere.
/// </summary>
public sealed class DpdpDbContext(DbContextOptions<DpdpDbContext> options, ICurrentUserContext currentUser)
    : DbContext(options), IAppDbContext
{
    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<OrganisationLocation> OrganisationLocations => Set<OrganisationLocation>();
    public DbSet<BusinessUnit> BusinessUnits => Set<BusinessUnit>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Framework> Frameworks => Set<Framework>();
    public DbSet<FrameworkVersion> FrameworkVersions => Set<FrameworkVersion>();
    public DbSet<LegalReference> LegalReferences => Set<LegalReference>();
    public DbSet<Requirement> Requirements => Set<Requirement>();
    public DbSet<ControlCategory> ControlCategories => Set<ControlCategory>();
    public DbSet<Control> Controls => Set<Control>();
    public DbSet<ControlMapping> ControlMappings => Set<ControlMapping>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<EvidenceRequirement> EvidenceRequirements => Set<EvidenceRequirement>();

    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<AssessmentScope> AssessmentScopes => Set<AssessmentScope>();
    public DbSet<AssessmentControl> AssessmentControls => Set<AssessmentControl>();
    public DbSet<AssessmentControlQuestion> AssessmentControlQuestions => Set<AssessmentControlQuestion>();
    public DbSet<AssessmentAnswer> AssessmentAnswers => Set<AssessmentAnswer>();
    public DbSet<AssessmentReview> AssessmentReviews => Set<AssessmentReview>();
    public DbSet<AssessmentApproval> AssessmentApprovals => Set<AssessmentApproval>();

    public DbSet<Finding> Findings => Set<Finding>();
    public DbSet<Risk> Risks => Set<Risk>();
    public DbSet<RemediationTask> RemediationTasks => Set<RemediationTask>();
    public DbSet<RemediationComment> RemediationComments => Set<RemediationComment>();

    public DbSet<EvidenceItem> EvidenceItems => Set<EvidenceItem>();
    public DbSet<EvidenceVersion> EvidenceVersions => Set<EvidenceVersion>();
    public DbSet<EvidenceReviewRecord> EvidenceReviewRecords => Set<EvidenceReviewRecord>();

    public DbSet<DataSource> DataSources => Set<DataSource>();
    public DbSet<DiscoveryJob> DiscoveryJobs => Set<DiscoveryJob>();
    public DbSet<DiscoveryResult> DiscoveryResults => Set<DiscoveryResult>();
    public DbSet<DataAsset> DataAssets => Set<DataAsset>();
    public DbSet<DataElement> DataElements => Set<DataElement>();

    public DbSet<DataCategory> DataCategories => Set<DataCategory>();
    public DbSet<ItSystem> ItSystems => Set<ItSystem>();
    public DbSet<DataCollectionSource> DataCollectionSources => Set<DataCollectionSource>();
    public DbSet<Processor> Processors => Set<Processor>();
    public DbSet<Recipient> Recipients => Set<Recipient>();
    public DbSet<RetentionPolicy> RetentionPolicies => Set<RetentionPolicy>();
    public DbSet<DataInventoryItem> DataInventoryItems => Set<DataInventoryItem>();
    public DbSet<ProcessingActivity> ProcessingActivities => Set<ProcessingActivity>();
    public DbSet<DataFlow> DataFlows => Set<DataFlow>();

    public DbSet<DataPrincipal> DataPrincipals => Set<DataPrincipal>();
    public DbSet<ConsentPurpose> ConsentPurposes => Set<ConsentPurpose>();
    public DbSet<PrivacyNotice> PrivacyNotices => Set<PrivacyNotice>();
    public DbSet<ConsentRecord> ConsentRecords => Set<ConsentRecord>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<DataPrincipalRequest> DataPrincipalRequests => Set<DataPrincipalRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Tenant isolation (docs/ARCHITECTURE.md section 4) + soft delete.
        // OrganisationId is nullable on User (null only for Super
        // Administrator), so this can't be a plain ITenantScoped filter —
        // see docs/DATABASE.md section 3 and User.cs.
        modelBuilder.Entity<User>().HasQueryFilter(u =>
            !u.IsDeleted && (currentUser.IsSuperAdministrator || u.OrganisationId == currentUser.OrganisationId));

        modelBuilder.Entity<Organisation>().HasQueryFilter(o => !o.IsDeleted);

        // Every other tenant-scoped entity has a non-nullable
        // OrganisationId, so it gets the one-line ITenantScoped filter —
        // this is the pattern every future tenant-scoped entity should
        // reuse (Module 3 completion report / docs/ARCHITECTURE.md §13).
        ApplyTenantScopedFilter<OrganisationLocation>(modelBuilder);
        ApplyTenantScopedFilter<BusinessUnit>(modelBuilder);
        ApplyTenantScopedFilter<Department>(modelBuilder);

        // Compliance framework/control library (Module 4): global reference
        // data shared by every tenant, deliberately NOT ITenantScoped — see
        // docs/ARCHITECTURE.md section 14. Only soft-delete needs filtering,
        // on the two leaf entities that support a delete action at all.
        ApplySoftDeleteFilter<AssessmentQuestion>(modelBuilder);
        ApplySoftDeleteFilter<EvidenceRequirement>(modelBuilder);

        // Assessments (Module 5): genuinely tenant-owned data (unlike the
        // Compliance library above), so every entity here is ITenantScoped —
        // denormalized OrganisationId on every child row rather than relying
        // on a join through Assessment, matching the Module 3 precedent
        // (Department denormalizes OrganisationId from BusinessUnit) — see
        // docs/ARCHITECTURE.md Module 5 section. Only Assessment itself is
        // independently soft-deletable; its children are never deleted on
        // their own, only cascaded with their parent.
        ApplyTenantScopedFilter<Assessment>(modelBuilder);
        ApplyTenantOnlyFilter<AssessmentScope>(modelBuilder);
        ApplyTenantOnlyFilter<AssessmentControl>(modelBuilder);
        ApplyTenantOnlyFilter<AssessmentControlQuestion>(modelBuilder);
        ApplyTenantOnlyFilter<AssessmentAnswer>(modelBuilder);
        ApplyTenantOnlyFilter<AssessmentReview>(modelBuilder);
        ApplyTenantOnlyFilter<AssessmentApproval>(modelBuilder);

        // Findings/Risk/Remediation (Module 6): tenant-owned data, same
        // denormalized-OrganisationId pattern as Module 5 — see
        // docs/ARCHITECTURE.md Module 6 section.
        ApplyTenantScopedFilter<Finding>(modelBuilder);
        ApplyTenantScopedFilter<Risk>(modelBuilder);
        ApplyTenantOnlyFilter<RemediationTask>(modelBuilder);
        ApplyTenantOnlyFilter<RemediationComment>(modelBuilder);

        // Evidence Management (Module 7): tenant-owned data, same
        // denormalized-OrganisationId pattern as Modules 5/6 — see
        // docs/ARCHITECTURE.md Module 7 section.
        ApplyTenantScopedFilter<EvidenceItem>(modelBuilder);
        ApplyTenantOnlyFilter<EvidenceVersion>(modelBuilder);
        ApplyTenantOnlyFilter<EvidenceReviewRecord>(modelBuilder);

        // Data Discovery (Module 8): tenant-owned data, same denormalized-
        // OrganisationId pattern as Modules 5/6/7 — see docs/ARCHITECTURE.md
        // Module 8 section. DataSource and DataAsset are the two aggregate
        // roots (independently soft-deletable); DiscoveryJob/DiscoveryResult/
        // DataElement are children, cascade-deleted with their parent.
        ApplyTenantScopedFilter<DataSource>(modelBuilder);
        ApplyTenantOnlyFilter<DiscoveryJob>(modelBuilder);
        ApplyTenantOnlyFilter<DiscoveryResult>(modelBuilder);
        ApplyTenantScopedFilter<DataAsset>(modelBuilder);
        ApplyTenantOnlyFilter<DataElement>(modelBuilder);

        // Data Inventory & Processing Activities (Module 9): every entity
        // is an independent, tenant-owned aggregate root (no parent-child
        // cascade relationships between them — they cross-reference each
        // other via nullable FKs/many-to-many instead) — see
        // docs/ARCHITECTURE.md Module 9 section.
        ApplyTenantScopedFilter<DataCategory>(modelBuilder);
        ApplyTenantScopedFilter<ItSystem>(modelBuilder);
        ApplyTenantScopedFilter<DataCollectionSource>(modelBuilder);
        ApplyTenantScopedFilter<Processor>(modelBuilder);
        ApplyTenantScopedFilter<Recipient>(modelBuilder);
        ApplyTenantScopedFilter<RetentionPolicy>(modelBuilder);
        ApplyTenantScopedFilter<DataInventoryItem>(modelBuilder);
        ApplyTenantScopedFilter<ProcessingActivity>(modelBuilder);
        ApplyTenantScopedFilter<DataFlow>(modelBuilder);

        // Consent & Privacy Operations (Module 10): independent tenant-owned
        // aggregate roots, same shape as Module 9. ConsentRecord is
        // deliberately NOT soft-deletable — it is compliance evidence with
        // no delete action at all, the same "append-only" precedent as
        // AuditLog — see docs/CONSENT_PRIVACY_OPERATIONS.md section 3.
        ApplyTenantScopedFilter<DataPrincipal>(modelBuilder);
        ApplyTenantScopedFilter<ConsentPurpose>(modelBuilder);
        ApplyTenantScopedFilter<PrivacyNotice>(modelBuilder);
        ApplyTenantOnlyFilter<ConsentRecord>(modelBuilder);
        ApplyTenantScopedFilter<SlaPolicy>(modelBuilder);
        ApplyTenantScopedFilter<DataPrincipalRequest>(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void ApplyTenantScopedFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            !e.IsDeleted && (currentUser.IsSuperAdministrator || e.OrganisationId == currentUser.OrganisationId));
    }

    private void ApplyTenantOnlyFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            currentUser.IsSuperAdministrator || e.OrganisationId == currentUser.OrganisationId);
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
