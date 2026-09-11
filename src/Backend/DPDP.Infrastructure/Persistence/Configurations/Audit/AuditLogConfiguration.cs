using DPDP.Domain.Modules.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Audit;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).HasMaxLength(150).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(150).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(150);
        builder.Property(a => a.OldValue).HasColumnType("jsonb");
        builder.Property(a => a.NewValue).HasColumnType("jsonb");
        builder.Property(a => a.IpAddress).HasMaxLength(64);
        builder.Property(a => a.UserAgent).HasMaxLength(300);
        builder.Property(a => a.CorrelationId).HasMaxLength(64);

        builder.HasIndex(a => new { a.OrganisationId, a.CreatedAt });
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.CorrelationId);

        // Append-only at the database-role level too — see
        // docs/SECURITY.md section 6; a migration script (not this
        // configuration) revokes UPDATE/DELETE on this table for the
        // application's role in a production deployment.
    }
}
