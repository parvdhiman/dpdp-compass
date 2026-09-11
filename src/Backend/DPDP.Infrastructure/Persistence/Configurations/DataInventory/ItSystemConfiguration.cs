using DPDP.Domain.Modules.DataInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataInventory;

public sealed class ItSystemConfiguration : IEntityTypeConfiguration<ItSystem>
{
    public void Configure(EntityTypeBuilder<ItSystem> builder)
    {
        builder.ToTable("it_systems");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.SystemType).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(s => s.Owner).WithMany().HasForeignKey(s => s.OwnerUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => s.OrganisationId);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
    }
}
