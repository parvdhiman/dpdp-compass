using DPDP.Domain.Modules.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(ur => ur.Id);

        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // See User.cs / UserRole.cs for why OrganisationId can't be part of
        // a composite primary key: two partial unique indexes instead of
        // one, split on whether organisation_id is null.
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.OrganisationId })
            .IsUnique()
            .HasFilter("organisation_id IS NOT NULL");

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasFilter("organisation_id IS NULL");

        builder.Property(ur => ur.AssignedAt).IsRequired();
    }
}
