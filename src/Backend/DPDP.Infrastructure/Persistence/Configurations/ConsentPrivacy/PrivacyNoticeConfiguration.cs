using DPDP.Domain.Modules.ConsentPrivacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.ConsentPrivacy;

public sealed class PrivacyNoticeConfiguration : IEntityTypeConfiguration<PrivacyNotice>
{
    public void Configure(EntityTypeBuilder<PrivacyNotice> builder)
    {
        builder.ToTable("privacy_notices");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.SequenceNumber).UseIdentityColumn();
        builder.HasIndex(n => new { n.OrganisationId, n.SequenceNumber }).IsUnique();

        builder.Property(n => n.Code).HasMaxLength(100).IsRequired();
        builder.Property(n => n.Title).HasMaxLength(300).IsRequired();
        builder.Property(n => n.Version).HasMaxLength(50).IsRequired();
        builder.Property(n => n.Language).HasMaxLength(50).IsRequired();
        builder.Property(n => n.Purpose).HasMaxLength(2000).IsRequired();
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(n => new { n.OrganisationId, n.Code, n.Version }).IsUnique();
        builder.HasIndex(n => new { n.OrganisationId, n.Status });

        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt).IsRequired();

        builder.HasMany(n => n.DataCategories).WithMany()
            .UsingEntity(j => j.ToTable("privacy_notice_data_categories"));
    }
}
