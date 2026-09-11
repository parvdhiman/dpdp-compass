using DPDP.Domain.Modules.Compliance;
using DPDP.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Compliance;

public sealed class FrameworkConfiguration : IEntityTypeConfiguration<Framework>
{
    public void Configure(EntityTypeBuilder<Framework> builder)
    {
        builder.ToTable("compliance_frameworks");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name).HasMaxLength(300).IsRequired();
        builder.Property(f => f.Code).HasMaxLength(50).IsRequired();
        builder.Property(f => f.Jurisdiction).HasMaxLength(100).IsRequired();
        builder.Property(f => f.IssuingAuthority).HasMaxLength(300).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(2000);
        builder.HasIndex(f => f.Code).IsUnique();

        builder.HasData(
            new Framework
            {
                Id = DpdpActSeedData.DpdpActFrameworkId,
                Name = "Digital Personal Data Protection Act, 2023",
                Code = "DPDPA-2023",
                Jurisdiction = "India",
                IssuingAuthority = "Parliament of India",
                Description = "India's principal law governing the processing of digital personal data. Enacted to provide for the processing of digital personal data in a manner that recognises both the right of individuals to protect their personal data and the need to process such data for lawful purposes.",
                CreatedAt = SeedClock.Timestamp,
            },
            new Framework
            {
                Id = DpdpActSeedData.DpdpRulesFrameworkId,
                Name = "Digital Personal Data Protection Rules, 2025",
                Code = "DPDPR-2025",
                Jurisdiction = "India",
                IssuingAuthority = "Ministry of Electronics and Information Technology, Government of India",
                Description = "Subordinate legislation made under the rule-making power of the Digital Personal Data Protection Act, 2023, prescribing procedural and implementation detail. Detailed rule-by-rule content has not been added to this system pending verification against the final official Gazette notification — see docs/COMPLIANCE_CONTENT_GOVERNANCE.md.",
                CreatedAt = SeedClock.Timestamp,
            });
    }
}
