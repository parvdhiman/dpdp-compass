using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataDiscovery;

public sealed class DataSourceConfiguration : IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.ToTable("data_sources");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.SourceType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(s => s.Host).HasMaxLength(300);
        builder.Property(s => s.DatabaseName).HasMaxLength(200);
        builder.Property(s => s.Username).HasMaxLength(200);
        // Ciphertext from IConnectionSecretProtector — never the plaintext
        // secret. See docs/DATA_DISCOVERY.md section 2.
        builder.Property(s => s.EncryptedSecret).HasMaxLength(4000);
        builder.Property(s => s.RootPath).HasMaxLength(1000);
        builder.Property(s => s.SchemaFilter).HasMaxLength(200);
        builder.Property(s => s.LastTestError).HasMaxLength(2000);

        builder.HasIndex(s => s.OrganisationId);
        builder.HasIndex(s => new { s.OrganisationId, s.SourceType });

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
    }
}
