using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.Property(u => u.NormalizedEmail).HasMaxLength(320).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.SecurityStamp).HasMaxLength(100).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.PhoneNumber).HasMaxLength(30);
        builder.Property(u => u.MfaSecret).HasMaxLength(200);

        builder.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey(u => u.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Postgres primary/unique keys treat NULL as distinct per row, so a
        // plain unique index on (organisation_id, normalized_email) would
        // let one org have unlimited NULL-organisation rows — that's fine
        // here because the *other* filtered index below covers the
        // Super-Administrator (organisation_id IS NULL) case exactly once.
        builder.HasIndex(u => new { u.OrganisationId, u.NormalizedEmail })
            .IsUnique()
            .HasFilter("organisation_id IS NOT NULL");

        builder.HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasFilter("organisation_id IS NULL");

        builder.HasIndex(u => u.OrganisationId);

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
