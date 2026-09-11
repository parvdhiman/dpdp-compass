using DPDP.Domain.Modules.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Assessments;

public sealed class AssessmentScopeConfiguration : IEntityTypeConfiguration<AssessmentScope>
{
    public void Configure(EntityTypeBuilder<AssessmentScope> builder)
    {
        builder.ToTable("assessment_scopes");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Notes).HasMaxLength(1000);

        builder.HasOne(s => s.Assessment)
            .WithMany(a => a.Scopes)
            .HasForeignKey(s => s.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.BusinessUnit)
            .WithMany()
            .HasForeignKey(s => s.BusinessUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Department)
            .WithMany()
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.OrganisationId);
        builder.HasIndex(s => s.AssessmentId);

        builder.Property(s => s.CreatedAt).IsRequired();
    }
}
