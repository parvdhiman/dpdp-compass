using DPDP.Domain.Modules.DataInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataInventory;

public sealed class DataFlowConfiguration : IEntityTypeConfiguration<DataFlow>
{
    public void Configure(EntityTypeBuilder<DataFlow> builder)
    {
        builder.ToTable("data_flows");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.SequenceNumber).UseIdentityColumn();
        builder.HasIndex(f => new { f.OrganisationId, f.SequenceNumber }).IsUnique();

        builder.Property(f => f.Name).HasMaxLength(300).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(2000);
        builder.Property(f => f.FromDescription).HasMaxLength(500).IsRequired();
        builder.Property(f => f.ToDescription).HasMaxLength(500).IsRequired();
        builder.Property(f => f.TransferMechanism).HasMaxLength(300);
        builder.Property(f => f.CrossBorderCountry).HasMaxLength(100);

        builder.HasOne(f => f.ProcessingActivity).WithMany().HasForeignKey(f => f.ProcessingActivityId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(f => f.DataCategory).WithMany().HasForeignKey(f => f.DataCategoryId).OnDelete(DeleteBehavior.SetNull);

        // Two FKs to ItSystem (from/to) — no inverse collection navigation
        // on ItSystem, so each must be configured explicitly rather than
        // left to convention, which cannot infer which of two FKs pairs
        // with which meaning.
        builder.HasOne(f => f.FromItSystem).WithMany().HasForeignKey(f => f.FromItSystemId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(f => f.ToItSystem).WithMany().HasForeignKey(f => f.ToItSystemId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(f => f.FromDataCollectionSource).WithMany().HasForeignKey(f => f.FromDataCollectionSourceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(f => f.ToProcessor).WithMany().HasForeignKey(f => f.ToProcessorId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(f => f.ToRecipient).WithMany().HasForeignKey(f => f.ToRecipientId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(f => f.OrganisationId);
        builder.HasIndex(f => f.ProcessingActivityId);

        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.UpdatedAt).IsRequired();
    }
}
