using System.Text.Json;
using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Compliance;

public sealed class AssessmentQuestionConfiguration : IEntityTypeConfiguration<AssessmentQuestion>
{
    public static Guid QuestionId(string code) => DeterministicGuid.Create($"question:{code}");

    public void Configure(EntityTypeBuilder<AssessmentQuestion> builder)
    {
        builder.ToTable("assessment_questions");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Code).HasMaxLength(50).IsRequired();
        builder.Property(q => q.Text).HasMaxLength(1000).IsRequired();
        builder.Property(q => q.HelpText).HasMaxLength(1000);
        builder.Property(q => q.QuestionType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(q => q.OptionsJson).HasColumnType("jsonb");
        builder.HasIndex(q => q.Code).IsUnique();

        builder.HasOne(q => q.Control)
            .WithMany(c => c.Questions)
            .HasForeignKey(q => q.ControlId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(DpdpActSeedData.Questions.Select((q, index) => new AssessmentQuestion
        {
            Id = QuestionId(q.Code),
            ControlId = ControlConfiguration.ControlEntityId(q.ControlId),
            Code = q.Code,
            Text = q.Text,
            HelpText = q.HelpText,
            QuestionType = Enum.Parse<QuestionType>(q.QuestionType),
            OptionsJson = q.Options is null ? null : JsonSerializer.Serialize(q.Options),
            IsRequired = true,
            SortOrder = index + 1,
            CreatedAt = SeedClock.Timestamp,
        }));
    }
}
