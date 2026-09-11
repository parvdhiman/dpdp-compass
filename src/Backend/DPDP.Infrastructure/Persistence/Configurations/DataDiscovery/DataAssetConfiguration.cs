using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataDiscovery;

public sealed class DataAssetConfiguration : IEntityTypeConfiguration<DataAsset>
{
    public void Configure(EntityTypeBuilder<DataAsset> builder)
    {
        builder.ToTable("data_assets");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.DatabaseName).HasMaxLength(200);
        builder.Property(a => a.SchemaName).HasMaxLength(200);
        builder.Property(a => a.AssetName).HasMaxLength(300).IsRequired();
        builder.Property(a => a.AssetType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.FilePath).HasMaxLength(2000);
        builder.Property(a => a.IndexesJson).HasColumnType("jsonb");

        builder.HasOne(a => a.DataSource).WithMany(s => s.DataAssets).HasForeignKey(a => a.DataSourceId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.OrganisationId);
        builder.HasIndex(a => a.DataSourceId);
        builder.HasIndex(a => new { a.DataSourceId, a.SchemaName, a.AssetName }).IsUnique();

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();
    }
}
