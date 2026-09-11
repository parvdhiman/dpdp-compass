using DPDP.Domain.Modules.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Organisations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(1000);

        builder.OwnsOne(d => d.Head, head =>
        {
            head.Property(c => c.Name).HasColumnName("head_name").HasMaxLength(200);
            head.Property(c => c.Email).HasColumnName("head_email").HasMaxLength(320);
            head.Property(c => c.Phone).HasColumnName("head_phone").HasMaxLength(30);
        });

        builder.HasOne(d => d.BusinessUnit)
            .WithMany(b => b.Departments)
            .HasForeignKey(d => d.BusinessUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.OrganisationId);
        builder.HasIndex(d => d.BusinessUnitId);
        builder.HasIndex(d => new { d.BusinessUnitId, d.Name });

        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt).IsRequired();
    }
}
