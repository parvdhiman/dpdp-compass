using DPDP.Domain.Modules.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Assessments;

public sealed class AssessmentAnswerConfiguration : IEntityTypeConfiguration<AssessmentAnswer>
{
    public void Configure(EntityTypeBuilder<AssessmentAnswer> builder)
    {
        builder.ToTable("assessment_answers");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.AnswerValue).HasMaxLength(4000);
        builder.Property(a => a.AnswerValuesJson).HasColumnType("jsonb");
        builder.Property(a => a.Comment).HasMaxLength(2000);
        builder.Property(a => a.EvidenceJson).HasColumnType("jsonb");
        builder.Property(a => a.ReviewComment).HasMaxLength(2000);
        builder.Property(a => a.Confidence).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.AssessedRiskLevel).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.RemediationNotes).HasMaxLength(2000);

        builder.HasOne(a => a.Reviewer)
            .WithMany()
            .HasForeignKey(a => a.ReviewerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.OrganisationId);
        builder.HasIndex(a => a.AssessmentControlQuestionId).IsUnique();
        builder.HasIndex(a => a.Status);

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();
    }
}
