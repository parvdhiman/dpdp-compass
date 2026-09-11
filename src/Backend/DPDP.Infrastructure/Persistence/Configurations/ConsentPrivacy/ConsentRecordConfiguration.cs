using DPDP.Domain.Modules.ConsentPrivacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.ConsentPrivacy;

public sealed class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.ToTable("consent_records");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.SequenceNumber).UseIdentityColumn();
        builder.HasIndex(c => new { c.OrganisationId, c.SequenceNumber }).IsUnique();

        builder.Property(c => c.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.SourceSystem).HasMaxLength(200);
        builder.Property(c => c.ExternalReferenceId).HasMaxLength(300);

        builder.HasOne(c => c.DataPrincipal).WithMany().HasForeignKey(c => c.DataPrincipalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.ConsentPurpose).WithMany().HasForeignKey(c => c.ConsentPurposeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.NoticeVersion).WithMany().HasForeignKey(c => c.NoticeVersionId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.OrganisationId);
        builder.HasIndex(c => c.DataPrincipalId);
        builder.HasIndex(c => new { c.OrganisationId, c.Status });
        builder.HasIndex(c => c.ExpiresAt);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();
    }
}
