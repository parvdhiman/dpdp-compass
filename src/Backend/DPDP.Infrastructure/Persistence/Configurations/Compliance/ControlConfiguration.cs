using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Compliance;

public sealed class ControlConfiguration : IEntityTypeConfiguration<Control>
{
    public static Guid ControlEntityId(string controlId) => DeterministicGuid.Create($"control:{controlId}");

    public void Configure(EntityTypeBuilder<Control> builder)
    {
        builder.ToTable("controls");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ControlId).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(4000).IsRequired();
        builder.Property(c => c.Objective).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.ApplicableConditions).HasMaxLength(1000);
        builder.Property(c => c.EvidenceRequirementsSummary).HasMaxLength(1000);
        builder.Property(c => c.Guidance).HasMaxLength(2000);
        builder.Property(c => c.SourceReference).HasMaxLength(500).IsRequired();
        builder.Property(c => c.RiskLevel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.ReviewStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.ControlId).IsUnique();
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.RiskLevel);

        builder.HasOne(c => c.ControlCategory)
            .WithMany()
            .HasForeignKey(c => c.ControlCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasData(DpdpActSeedData.Controls.Select(c => new Control
        {
            Id = ControlEntityId(c.ControlId),
            ControlId = c.ControlId,
            ControlCategoryId = ControlCategoryConfiguration.CategoryId(c.CategoryKey),
            Name = c.Name,
            Description = c.Description,
            Objective = c.Objective,
            RiskLevel = Enum.Parse<RiskLevel>(c.RiskLevel),
            ApplicableConditions = c.ApplicableConditions,
            EvidenceRequirementsSummary = c.EvidenceSummary,
            Guidance = c.Guidance,
            SourceReference = c.SourceReference,
            EffectiveDate = null,
            ReviewDate = null,
            Version = 1,
            // Shipped ACTIVE (operationally usable) but ContentReviewStatus
            // DRAFT (pending legal review) — see
            // docs/COMPLIANCE_CONTENT_GOVERNANCE.md for why these are
            // deliberately independent flags, not the same thing.
            Status = ControlStatus.ACTIVE,
            ReviewStatus = ContentReviewStatus.DRAFT,
            CreatedAt = SeedClock.Timestamp,
            UpdatedAt = SeedClock.Timestamp,
        }));
    }
}
