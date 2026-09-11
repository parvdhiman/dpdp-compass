using DPDP.Domain.Modules.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Assessments;

public sealed class AssessmentControlQuestionConfiguration : IEntityTypeConfiguration<AssessmentControlQuestion>
{
    public void Configure(EntityTypeBuilder<AssessmentControlQuestion> builder)
    {
        builder.ToTable("assessment_control_questions");
        builder.HasKey(q => q.Id);

        builder.HasOne(q => q.AssessmentControl)
            .WithMany(c => c.Questions)
            .HasForeignKey(q => q.AssessmentControlId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.Question)
            .WithMany()
            .HasForeignKey(q => q.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Answer)
            .WithOne(a => a.AssessmentControlQuestion)
            .HasForeignKey<AssessmentAnswer>(a => a.AssessmentControlQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.OrganisationId);
        builder.HasIndex(q => q.AssessmentControlId);
        builder.HasIndex(q => new { q.AssessmentControlId, q.QuestionId }).IsUnique();

        builder.Property(q => q.CreatedAt).IsRequired();
    }
}
