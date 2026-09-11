using DPDP.Domain.Modules.ConsentPrivacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.ConsentPrivacy;

public sealed class ConsentPurposeConfiguration : IEntityTypeConfiguration<ConsentPurpose>
{
    public void Configure(EntityTypeBuilder<ConsentPurpose> builder)
    {
        builder.ToTable("consent_purposes");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);

        builder.HasOne(p => p.DataCategory).WithMany().HasForeignKey(p => p.DataCategoryId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.OrganisationId);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
    }
}
