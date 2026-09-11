using DPDP.Domain.Modules.Findings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Findings;

public sealed class FindingConfiguration : IEntityTypeConfiguration<Finding>
{
    public void Configure(EntityTypeBuilder<Finding> builder)
    {
        builder.ToTable("findings");
        builder.HasKey(f => f.Id);

        // Database-generated, atomic under concurrent creation — the
        // human-facing "FIND-00001" display value is computed from this,
        // never stored as a formatted string. See Finding.cs.
        builder.Property(f => f.SequenceNumber).UseIdentityColumn();
        builder.HasIndex(f => new { f.OrganisationId, f.SequenceNumber }).IsUnique();

        builder.Property(f => f.Title).HasMaxLength(300).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(4000).IsRequired();
        builder.Property(f => f.Source).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(f => f.AssetReference).HasMaxLength(300);
        builder.Property(f => f.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(f => f.Recommendation).HasMaxLength(2000);

        builder.HasOne(f => f.Assessment).WithMany().HasForeignKey(f => f.AssessmentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(f => f.AssessmentControl).WithMany().HasForeignKey(f => f.AssessmentControlId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(f => f.Control).WithMany().HasForeignKey(f => f.ControlId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(f => f.Risk).WithMany(r => r.Findings).HasForeignKey(f => f.RiskId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(f => f.Owner).WithMany().HasForeignKey(f => f.OwnerUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(f => f.OrganisationId);
        builder.HasIndex(f => new { f.OrganisationId, f.Status });
        builder.HasIndex(f => new { f.OrganisationId, f.Severity });
        builder.HasIndex(f => f.OwnerUserId);
        builder.HasIndex(f => f.RiskId);
        builder.HasIndex(f => f.AssessmentId);

        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.UpdatedAt).IsRequired();
    }
}
