using DPDP.Domain.Modules.Evidence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Evidence;

public sealed class EvidenceVersionConfiguration : IEntityTypeConfiguration<EvidenceVersion>
{
    public void Configure(EntityTypeBuilder<EvidenceVersion> builder)
    {
        builder.ToTable("evidence_versions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.StorageKey).HasMaxLength(1000);
        builder.Property(v => v.OriginalFileName).HasMaxLength(300);
        builder.Property(v => v.ContentType).HasMaxLength(150);
        builder.Property(v => v.ChecksumSha256).HasMaxLength(64);
        builder.Property(v => v.ExternalUrl).HasMaxLength(2000);
        builder.Property(v => v.MalwareScanStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(v => v.MalwareScanDetails).HasMaxLength(2000);
        builder.Property(v => v.MetadataJson).HasColumnType("jsonb");

        builder.HasOne(v => v.EvidenceItem).WithMany(e => e.Versions).HasForeignKey(v => v.EvidenceItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(v => v.UploadedByUser).WithMany().HasForeignKey(v => v.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.OrganisationId);
        builder.HasIndex(v => new { v.EvidenceItemId, v.VersionNumber }).IsUnique();

        builder.Property(v => v.UploadedAt).IsRequired();
    }
}
