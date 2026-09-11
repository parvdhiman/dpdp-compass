using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataDiscovery;

public sealed class DiscoveryJobConfiguration : IEntityTypeConfiguration<DiscoveryJob>
{
    public void Configure(EntityTypeBuilder<DiscoveryJob> builder)
    {
        builder.ToTable("discovery_jobs");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(j => j.ErrorMessage).HasMaxLength(2000);

        builder.HasOne(j => j.DataSource).WithMany(s => s.DiscoveryJobs).HasForeignKey(j => j.DataSourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(j => j.TriggeredByUser).WithMany().HasForeignKey(j => j.TriggeredByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(j => j.OrganisationId);
        builder.HasIndex(j => j.DataSourceId);
        builder.HasIndex(j => new { j.OrganisationId, j.Status });

        builder.Property(j => j.CreatedAt).IsRequired();
        builder.Property(j => j.UpdatedAt).IsRequired();
    }
}
