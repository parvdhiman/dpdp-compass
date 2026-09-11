using DPDP.Domain.Modules.DataInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataInventory;

public sealed class ProcessorConfiguration : IEntityTypeConfiguration<Processor>
{
    public void Configure(EntityTypeBuilder<Processor> builder)
    {
        builder.ToTable("processors");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.ContactEmail).HasMaxLength(300);
        builder.Property(p => p.Country).HasMaxLength(100);

        builder.HasIndex(p => p.OrganisationId);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
    }
}
