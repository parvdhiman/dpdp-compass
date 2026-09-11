using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataDiscovery;

public sealed class DataElementConfiguration : IEntityTypeConfiguration<DataElement>
{
    public void Configure(EntityTypeBuilder<DataElement> builder)
    {
        builder.ToTable("data_elements");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ColumnName).HasMaxLength(300).IsRequired();
        builder.Property(e => e.DataType).HasMaxLength(100).IsRequired();
        // Masked only — see SampleMasker and docs/DATA_DISCOVERY.md section 3.
        builder.Property(e => e.SampleMaskedValue).HasMaxLength(300);
        builder.Property(e => e.ClassificationCategory).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.ClassificationConfidence).HasPrecision(5, 2);
        builder.Property(e => e.ClassificationSource).HasConversion<string>().HasMaxLength(10);

        builder.HasOne(e => e.DataAsset).WithMany(a => a.Elements).HasForeignKey(e => e.DataAssetId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.CorrectedByUser).WithMany().HasForeignKey(e => e.CorrectedByUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.OrganisationId);
        builder.HasIndex(e => e.DataAssetId);
        builder.HasIndex(e => new { e.DataAssetId, e.ColumnName }).IsUnique();
        builder.HasIndex(e => new { e.OrganisationId, e.ClassificationCategory });

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
    }
}
