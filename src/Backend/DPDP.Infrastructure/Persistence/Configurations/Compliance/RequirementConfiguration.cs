using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Compliance;

public sealed class RequirementConfiguration : IEntityTypeConfiguration<Requirement>
{
    public static Guid RequirementId(string code) => DeterministicGuid.Create($"requirement:{code}");

    public void Configure(EntityTypeBuilder<Requirement> builder)
    {
        builder.ToTable("compliance_requirements");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Title).HasMaxLength(300).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(4000).IsRequired();
        builder.Property(r => r.ReviewStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(r => r.Code).IsUnique();

        builder.HasOne(r => r.LegalReference)
            .WithMany(l => l.Requirements)
            .HasForeignKey(r => r.LegalReferenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(DpdpActSeedData.Requirements.Select(r => new Requirement
        {
            Id = RequirementId(r.Code),
            LegalReferenceId = LegalReferenceConfiguration.LegalReferenceId(r.LegalRefKey),
            Code = r.Code,
            Title = r.Title,
            Description = r.Description,
            ReviewStatus = ContentReviewStatus.DRAFT,
            CreatedAt = SeedClock.Timestamp,
        }));
    }
}
