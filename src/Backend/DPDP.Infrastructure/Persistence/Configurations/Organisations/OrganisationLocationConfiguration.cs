using DPDP.Domain.Modules.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Organisations;

public sealed class OrganisationLocationConfiguration : IEntityTypeConfiguration<OrganisationLocation>
{
    public void Configure(EntityTypeBuilder<OrganisationLocation> builder)
    {
        builder.ToTable("organisation_locations");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Label).HasMaxLength(200).IsRequired();
        builder.Property(l => l.AddressLine1).HasMaxLength(300);
        builder.Property(l => l.AddressLine2).HasMaxLength(300);
        builder.Property(l => l.City).HasMaxLength(100);
        builder.Property(l => l.State).HasMaxLength(100);
        builder.Property(l => l.PostalCode).HasMaxLength(20);
        builder.Property(l => l.Country).HasMaxLength(100);

        builder.HasIndex(l => l.OrganisationId);

        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.UpdatedAt).IsRequired();
    }
}
