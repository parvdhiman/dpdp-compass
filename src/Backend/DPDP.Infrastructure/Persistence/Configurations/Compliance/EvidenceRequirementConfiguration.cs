using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Compliance;

public sealed class EvidenceRequirementConfiguration : IEntityTypeConfiguration<EvidenceRequirement>
{
    public void Configure(EntityTypeBuilder<EvidenceRequirement> builder)
    {
        builder.ToTable("evidence_requirements");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.AcceptableFormats).HasMaxLength(300);

        builder.HasOne(e => e.AssessmentQuestion)
            .WithMany(q => q.EvidenceRequirements)
            .HasForeignKey(e => e.AssessmentQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(DpdpActSeedData.EvidenceRequirements.Select(e => new EvidenceRequirement
        {
            Id = DeterministicGuid.Create($"evidence:{e.QuestionCode}"),
            AssessmentQuestionId = AssessmentQuestionConfiguration.QuestionId(e.QuestionCode),
            Name = e.Name,
            Description = e.Description,
            IsMandatory = e.IsMandatory,
            AcceptableFormats = e.AcceptableFormats,
            CreatedAt = SeedClock.Timestamp,
        }));
    }
}
