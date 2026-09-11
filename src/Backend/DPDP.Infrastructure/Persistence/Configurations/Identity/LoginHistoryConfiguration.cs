using DPDP.Domain.Modules.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Identity;

public sealed class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("login_history");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.AttemptedEmail).HasMaxLength(320).IsRequired();
        builder.Property(h => h.FailureReason).HasMaxLength(100);
        builder.Property(h => h.IpAddress).HasMaxLength(64);
        builder.Property(h => h.UserAgent).HasMaxLength(300);

        builder.HasIndex(h => new { h.OrganisationId, h.CreatedAt });
        builder.HasIndex(h => h.UserId);
    }
}
