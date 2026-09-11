using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataDiscovery;

public sealed class DiscoveryResultConfiguration : IEntityTypeConfiguration<DiscoveryResult>
{
    public void Configure(EntityTypeBuilder<DiscoveryResult> builder)
    {
        builder.ToTable("discovery_results");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.IndexesJson).HasColumnType("jsonb");

        builder.HasOne(r => r.DiscoveryJob).WithMany(j => j.Results).HasForeignKey(r => r.DiscoveryJobId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.DataAsset).WithMany(a => a.Results).HasForeignKey(r => r.DataAssetId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.OrganisationId);
        builder.HasIndex(r => r.DiscoveryJobId);
        builder.HasIndex(r => r.DataAssetId);

        builder.Property(r => r.ScannedAt).IsRequired();
    }
}
