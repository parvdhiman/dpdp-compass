using DPDP.Domain.Modules.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Organisations;

public sealed class BusinessUnitConfiguration : IEntityTypeConfiguration<BusinessUnit>
{
    public void Configure(EntityTypeBuilder<BusinessUnit> builder)
    {
        builder.ToTable("business_units");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Description).HasMaxLength(1000);

        builder.OwnsOne(b => b.Head, head =>
        {
            head.Property(c => c.Name).HasColumnName("head_name").HasMaxLength(200);
            head.Property(c => c.Email).HasColumnName("head_email").HasMaxLength(320);
            head.Property(c => c.Phone).HasColumnName("head_phone").HasMaxLength(30);
        });

        builder.HasOne(b => b.Organisation)
            .WithMany()
            .HasForeignKey(b => b.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.OrganisationId);
        builder.HasIndex(b => new { b.OrganisationId, b.Name });

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt).IsRequired();
    }
}
