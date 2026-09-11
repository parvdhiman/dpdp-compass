using DPDP.Domain.Modules.DataInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataInventory;

public sealed class DataInventoryItemConfiguration : IEntityTypeConfiguration<DataInventoryItem>
{
    public void Configure(EntityTypeBuilder<DataInventoryItem> builder)
    {
        builder.ToTable("data_inventory_items");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.SequenceNumber).UseIdentityColumn();
        builder.HasIndex(i => new { i.OrganisationId, i.SequenceNumber }).IsUnique();

        builder.Property(i => i.DataElementName).HasMaxLength(300).IsRequired();
        builder.Property(i => i.Classification).HasConversion<string>().HasMaxLength(30);
        builder.Property(i => i.Purpose).HasMaxLength(2000);
        builder.Property(i => i.SharingDescription).HasMaxLength(2000);
        builder.Property(i => i.RiskLevel).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(i => i.DataCategory).WithMany().HasForeignKey(i => i.DataCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.DiscoveredDataElement).WithMany().HasForeignKey(i => i.DiscoveredDataElementId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(i => i.DataCollectionSource).WithMany().HasForeignKey(i => i.DataCollectionSourceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(i => i.ItSystem).WithMany().HasForeignKey(i => i.ItSystemId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(i => i.Owner).WithMany().HasForeignKey(i => i.OwnerUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(i => i.RetentionPolicy).WithMany().HasForeignKey(i => i.RetentionPolicyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(i => i.Processor).WithMany().HasForeignKey(i => i.ProcessorId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.OrganisationId);
        builder.HasIndex(i => i.DataCategoryId);
        builder.HasIndex(i => new { i.OrganisationId, i.RiskLevel });

        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.UpdatedAt).IsRequired();
    }
}
