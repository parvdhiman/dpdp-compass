using DPDP.Domain.Modules.ConsentPrivacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.ConsentPrivacy;

public sealed class DataPrincipalConfiguration : IEntityTypeConfiguration<DataPrincipal>
{
    public void Configure(EntityTypeBuilder<DataPrincipal> builder)
    {
        builder.ToTable("data_principals");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ExternalReferenceId).HasMaxLength(300).IsRequired();
        builder.Property(p => p.ReferenceCategory).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.HasIndex(p => p.OrganisationId);
        builder.HasIndex(p => new { p.OrganisationId, p.ExternalReferenceId });

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
    }
}
