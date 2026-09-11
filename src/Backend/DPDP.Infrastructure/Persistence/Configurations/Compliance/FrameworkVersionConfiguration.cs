using DPDP.Domain.Modules.Compliance;
using DPDP.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Compliance;

public sealed class FrameworkVersionConfiguration : IEntityTypeConfiguration<FrameworkVersion>
{
    public void Configure(EntityTypeBuilder<FrameworkVersion> builder)
    {
        builder.ToTable("compliance_framework_versions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VersionLabel).HasMaxLength(50).IsRequired();
        builder.Property(v => v.OfficialCitation).HasMaxLength(200);
        builder.Property(v => v.SourceUrl).HasMaxLength(500);
        builder.Property(v => v.ChangeSummary).HasMaxLength(2000);
        builder.Property(v => v.ReviewStatus).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(v => v.Framework)
            .WithMany(f => f.Versions)
            .HasForeignKey(v => v.FrameworkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.FrameworkId, v.VersionLabel }).IsUnique();

        builder.HasData(
            new FrameworkVersion
            {
                Id = DpdpActSeedData.DpdpActVersionId,
                FrameworkId = DpdpActSeedData.DpdpActFrameworkId,
                VersionLabel = "2023",
                OfficialCitation = "Act No. 22 of 2023",
                PublicationDate = new DateOnly(2023, 8, 11),
                EffectiveDate = null,
                ReviewStatus = ContentReviewStatus.DRAFT,
                IsCurrent = true,
                ChangeSummary = "Initial seed reflecting the enacted Act's chapter/section structure. Section-level summaries are plain-language paraphrases pending qualified legal review — not verbatim statutory text. Commencement of individual provisions is subject to phased notification by the Central Government under Section 1(2); confirm current commencement status against the official Gazette before treating any provision as legally binding on a specific date.",
                CreatedAt = SeedClock.Timestamp,
            },
            new FrameworkVersion
            {
                Id = DpdpActSeedData.DpdpRulesVersionId,
                FrameworkId = DpdpActSeedData.DpdpRulesFrameworkId,
                VersionLabel = "2025",
                OfficialCitation = null,
                PublicationDate = null,
                EffectiveDate = null,
                ReviewStatus = ContentReviewStatus.DRAFT,
                IsCurrent = true,
                ChangeSummary = "Placeholder version created to demonstrate framework versioning capability. No legal references, requirements, or controls have been populated under this version — content will be added once verified against the final official Gazette notification of the Rules.",
                CreatedAt = SeedClock.Timestamp,
            });
    }
}
