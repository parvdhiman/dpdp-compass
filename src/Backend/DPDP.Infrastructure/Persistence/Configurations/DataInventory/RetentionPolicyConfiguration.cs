using DPDP.Domain.Modules.DataInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataInventory;

public sealed class RetentionPolicyConfiguration : IEntityTypeConfiguration<RetentionPolicy>
{
    public void Configure(EntityTypeBuilder<RetentionPolicy> builder)
    {
        builder.ToTable("retention_policies");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.RetentionPeriodUnit).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(p => p.TriggerEvent).HasMaxLength(500);

        builder.HasIndex(p => p.OrganisationId);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
    }
}
