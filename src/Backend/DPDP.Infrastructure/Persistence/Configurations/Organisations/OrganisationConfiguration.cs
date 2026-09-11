using DPDP.Domain.Modules.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Organisations;

public sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisations");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name).HasMaxLength(200).IsRequired();
        builder.Property(o => o.LegalName).HasMaxLength(300);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.Industry).HasMaxLength(100);
        builder.Property(o => o.Size).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Country).HasMaxLength(100);
        builder.Property(o => o.Website).HasMaxLength(300);

        builder.OwnsOne(o => o.PrimaryContact, contact =>
        {
            contact.Property(c => c.Name).HasColumnName("primary_contact_name").HasMaxLength(200);
            contact.Property(c => c.Email).HasColumnName("primary_contact_email").HasMaxLength(320);
            contact.Property(c => c.Phone).HasColumnName("primary_contact_phone").HasMaxLength(30);
        });

        builder.OwnsOne(o => o.PrivacyContact, contact =>
        {
            contact.Property(c => c.Name).HasColumnName("privacy_contact_name").HasMaxLength(200);
            contact.Property(c => c.Email).HasColumnName("privacy_contact_email").HasMaxLength(320);
            contact.Property(c => c.Phone).HasColumnName("privacy_contact_phone").HasMaxLength(30);
        });

        builder.OwnsOne(o => o.DpoContact, contact =>
        {
            contact.Property(c => c.Name).HasColumnName("dpo_name").HasMaxLength(200);
            contact.Property(c => c.Email).HasColumnName("dpo_email").HasMaxLength(320);
            contact.Property(c => c.Phone).HasColumnName("dpo_phone").HasMaxLength(30);
        });

        builder.HasMany(o => o.Locations)
            .WithOne(l => l.Organisation)
            .HasForeignKey(l => l.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();
    }
}
