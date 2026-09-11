using DPDP.Domain.Modules.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Assessments;

public sealed class AssessmentControlConfiguration : IEntityTypeConfiguration<AssessmentControl>
{
    public void Configure(EntityTypeBuilder<AssessmentControl> builder)
    {
        builder.ToTable("assessment_controls");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.Notes).HasMaxLength(2000);

        builder.HasOne(c => c.Assessment)
            .WithMany(a => a.Controls)
            .HasForeignKey(c => c.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Control)
            .WithMany()
            .HasForeignKey(c => c.ControlId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.OrganisationId);
        builder.HasIndex(c => c.AssessmentId);
        builder.HasIndex(c => new { c.AssessmentId, c.ControlId }).IsUnique();
        builder.HasIndex(c => c.Status);

        builder.Property(c => c.CreatedAt).IsRequired();
    }
}
