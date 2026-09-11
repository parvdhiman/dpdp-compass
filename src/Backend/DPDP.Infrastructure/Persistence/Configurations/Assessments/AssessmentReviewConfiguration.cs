using DPDP.Domain.Modules.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Assessments;

public sealed class AssessmentReviewConfiguration : IEntityTypeConfiguration<AssessmentReview>
{
    public void Configure(EntityTypeBuilder<AssessmentReview> builder)
    {
        builder.ToTable("assessment_reviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Decision).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Comments).HasMaxLength(2000);

        builder.HasOne(r => r.Assessment)
            .WithMany(a => a.Reviews)
            .HasForeignKey(r => r.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.OrganisationId);
        builder.HasIndex(r => r.AssessmentId);

        builder.Property(r => r.CreatedAt).IsRequired();
    }
}
