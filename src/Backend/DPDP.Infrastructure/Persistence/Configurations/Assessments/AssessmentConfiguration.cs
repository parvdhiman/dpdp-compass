using DPDP.Domain.Modules.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Assessments;

public sealed class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("assessments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(300).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2000);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(a => a.FrameworkVersion)
            .WithMany()
            .HasForeignKey(a => a.FrameworkVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AssignedToUser)
            .WithMany()
            .HasForeignKey(a => a.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.OrganisationId);
        builder.HasIndex(a => new { a.OrganisationId, a.Status });
        builder.HasIndex(a => a.FrameworkVersionId);
        builder.HasIndex(a => a.AssignedToUserId);

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();
    }
}
