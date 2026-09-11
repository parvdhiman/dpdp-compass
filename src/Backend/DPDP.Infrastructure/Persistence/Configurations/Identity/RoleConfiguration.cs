using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Identity;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private static readonly (string Name, string Description)[] Catalogue =
    [
        (RoleNames.SuperAdministrator, "Full, cross-tenant administrative access to the entire platform."),
        (RoleNames.OrganisationAdministrator, "Full administrative access within one organisation."),
        (RoleNames.PrivacyOfficer, "Manages privacy compliance activities: assessments, findings, evidence."),
        (RoleNames.ComplianceOfficer, "Oversees compliance posture: approves assessments, closes findings."),
        (RoleNames.SecurityOfficer, "Manages security controls, risk, and incident-adjacent findings."),
        (RoleNames.ItAdministrator, "Manages user accounts and technical controls."),
        (RoleNames.DepartmentOwner, "Departmental visibility into assessments, findings, and evidence."),
        (RoleNames.Auditor, "Read-only access across the platform, plus the audit trail."),
        (RoleNames.Management, "Executive read-only visibility into compliance posture and reports."),
        (RoleNames.ReadOnly, "Minimal read-only access."),
    ];

    public static Guid RoleId(string name) => DeterministicGuid.Create($"role:{name}");

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasData(Catalogue.Select(r => new Role
        {
            Id = RoleId(r.Name),
            Name = r.Name,
            Description = r.Description,
            IsSystemRole = true,
            CreatedAt = SeedClock.Timestamp,
        }));
    }
}
