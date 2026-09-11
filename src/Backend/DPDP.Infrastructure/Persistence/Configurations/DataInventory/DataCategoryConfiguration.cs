using DPDP.Domain.Modules.DataInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataInventory;

public sealed class DataCategoryConfiguration : IEntityTypeConfiguration<DataCategory>
{
    public void Configure(EntityTypeBuilder<DataCategory> builder)
    {
        builder.ToTable("data_categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.ClassificationCategory).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(c => c.OrganisationId);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();
    }
}
