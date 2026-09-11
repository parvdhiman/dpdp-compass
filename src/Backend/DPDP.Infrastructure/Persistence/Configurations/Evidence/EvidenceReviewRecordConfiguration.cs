using DPDP.Domain.Modules.Evidence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Evidence;

public sealed class EvidenceReviewRecordConfiguration : IEntityTypeConfiguration<EvidenceReviewRecord>
{
    public void Configure(EntityTypeBuilder<EvidenceReviewRecord> builder)
    {
        builder.ToTable("evidence_review_records");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Decision).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Comments).HasMaxLength(2000);

        builder.HasOne(r => r.EvidenceItem).WithMany(e => e.Reviews).HasForeignKey(r => r.EvidenceItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Reviewer).WithMany().HasForeignKey(r => r.ReviewerUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.OrganisationId);
        builder.HasIndex(r => r.EvidenceItemId);

        builder.Property(r => r.CreatedAt).IsRequired();
    }
}
