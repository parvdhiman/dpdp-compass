using DPDP.Domain.Modules.DataInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataInventory;

public sealed class DataCollectionSourceConfiguration : IEntityTypeConfiguration<DataCollectionSource>
{
    public void Configure(EntityTypeBuilder<DataCollectionSource> builder)
    {
        builder.ToTable("data_collection_sources");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.SourceType).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(s => s.OrganisationId);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
    }
}
