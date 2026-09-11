using DPDP.Domain.Modules.Evidence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Evidence;

public sealed class EvidenceItemConfiguration : IEntityTypeConfiguration<EvidenceItem>
{
    public void Configure(EntityTypeBuilder<EvidenceItem> builder)
    {
        builder.ToTable("evidence_items");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SequenceNumber).UseIdentityColumn();
        builder.HasIndex(e => new { e.OrganisationId, e.SequenceNumber }).IsUnique();

        builder.Property(e => e.Title).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.EvidenceType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.VendorReference).HasMaxLength(300);
        builder.Property(e => e.ProcessingActivityReference).HasMaxLength(300);
        builder.Property(e => e.RejectionReason).HasMaxLength(2000);

        builder.HasOne(e => e.Assessment).WithMany().HasForeignKey(e => e.AssessmentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.Control).WithMany().HasForeignKey(e => e.ControlId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.Finding).WithMany().HasForeignKey(e => e.FindingId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.Owner).WithMany().HasForeignKey(e => e.OwnerUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.Reviewer).WithMany().HasForeignKey(e => e.ReviewerUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.OrganisationId);
        builder.HasIndex(e => new { e.OrganisationId, e.Status });
        builder.HasIndex(e => new { e.OrganisationId, e.EvidenceType });
        builder.HasIndex(e => e.AssessmentId);
        builder.HasIndex(e => e.ControlId);
        builder.HasIndex(e => e.FindingId);
        builder.HasIndex(e => e.OwnerUserId);
        builder.HasIndex(e => e.ReviewerUserId);
        builder.HasIndex(e => e.ExpiryDate);

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
    }
}
