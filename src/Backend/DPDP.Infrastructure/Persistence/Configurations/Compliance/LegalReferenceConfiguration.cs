using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Compliance;

public sealed class LegalReferenceConfiguration : IEntityTypeConfiguration<LegalReference>
{
    public static Guid LegalReferenceId(string key) => DeterministicGuid.Create($"legal-reference:{key}");

    public void Configure(EntityTypeBuilder<LegalReference> builder)
    {
        builder.ToTable("compliance_legal_references");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Citation).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Title).HasMaxLength(300).IsRequired();
        builder.Property(l => l.Chapter).HasMaxLength(300);
        builder.Property(l => l.SummaryText).HasMaxLength(4000);
        builder.Property(l => l.SourceCitation).HasMaxLength(500).IsRequired();
        builder.Property(l => l.ReviewStatus).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(l => l.FrameworkVersion)
            .WithMany(v => v.LegalReferences)
            .HasForeignKey(l => l.FrameworkVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.FrameworkVersionId, l.Citation }).IsUnique();

        builder.HasData(DpdpActSeedData.LegalReferences.Select(r => new LegalReference
        {
            Id = LegalReferenceId(r.Key),
            FrameworkVersionId = DpdpActSeedData.DpdpActVersionId,
            Citation = r.Citation,
            Title = r.Title,
            Chapter = r.Chapter,
            SummaryText = r.Summary,
            SourceCitation = $"Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), {r.Citation}",
            ReviewStatus = ContentReviewStatus.DRAFT,
            CreatedAt = SeedClock.Timestamp,
        }));
    }
}
