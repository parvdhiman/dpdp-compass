using DPDP.Domain.Modules.Risks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Risks;

public sealed class RiskConfiguration : IEntityTypeConfiguration<Risk>
{
    public void Configure(EntityTypeBuilder<Risk> builder)
    {
        builder.ToTable("risks");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.SequenceNumber).UseIdentityColumn();
        builder.HasIndex(r => new { r.OrganisationId, r.SequenceNumber }).IsUnique();

        builder.Property(r => r.Title).HasMaxLength(300).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(4000).IsRequired();
        builder.Property(r => r.Likelihood).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Impact).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.DataSensitivity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Exposure).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.CalculatedRiskLevel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.TreatmentPlan).HasMaxLength(2000);

        builder.HasOne(r => r.Owner).WithMany().HasForeignKey(r => r.OwnerUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.OrganisationId);
        builder.HasIndex(r => new { r.OrganisationId, r.CalculatedRiskLevel });
        builder.HasIndex(r => new { r.OrganisationId, r.Status });
        builder.HasIndex(r => r.OwnerUserId);

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();
    }
}
