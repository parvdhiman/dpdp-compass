using DPDP.Domain.Modules.Remediation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Remediation;

public sealed class RemediationTaskConfiguration : IEntityTypeConfiguration<RemediationTask>
{
    public void Configure(EntityTypeBuilder<RemediationTask> builder)
    {
        builder.ToTable("remediation_tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).HasMaxLength(300).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(t => t.EvidenceJson).HasColumnType("jsonb");
        builder.Property(t => t.VerificationNotes).HasMaxLength(2000);

        builder.HasOne(t => t.Finding).WithMany(f => f.RemediationTasks).HasForeignKey(t => t.FindingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Owner).WithMany().HasForeignKey(t => t.OwnerUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(t => t.VerifiedByUser).WithMany().HasForeignKey(t => t.VerifiedByUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.OrganisationId);
        builder.HasIndex(t => t.FindingId);
        builder.HasIndex(t => new { t.OrganisationId, t.Status });
        builder.HasIndex(t => t.OwnerUserId);
        builder.HasIndex(t => t.DueDate);

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
    }
}
