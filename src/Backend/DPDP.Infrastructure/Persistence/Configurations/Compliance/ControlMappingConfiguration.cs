using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Compliance;

public sealed class ControlMappingConfiguration : IEntityTypeConfiguration<ControlMapping>
{
    public void Configure(EntityTypeBuilder<ControlMapping> builder)
    {
        builder.ToTable("control_mappings");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MappingNotes).HasMaxLength(500);

        builder.HasOne(m => m.Control)
            .WithMany(c => c.ControlMappings)
            .HasForeignKey(m => m.ControlId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Requirement)
            .WithMany(r => r.ControlMappings)
            .HasForeignKey(m => m.RequirementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.ControlId, m.RequirementId }).IsUnique();

        builder.HasData(DpdpActSeedData.Controls.Select(c => new ControlMapping
        {
            Id = DeterministicGuid.Create($"control-mapping:{c.ControlId}:{c.RequirementCode}"),
            ControlId = ControlConfiguration.ControlEntityId(c.ControlId),
            RequirementId = RequirementConfiguration.RequirementId(c.RequirementCode),
            CreatedAt = SeedClock.Timestamp,
        }));
    }
}
