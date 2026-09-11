using DPDP.Domain.Modules.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Assessments;

public sealed class AssessmentApprovalConfiguration : IEntityTypeConfiguration<AssessmentApproval>
{
    public void Configure(EntityTypeBuilder<AssessmentApproval> builder)
    {
        builder.ToTable("assessment_approvals");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Decision).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Comments).HasMaxLength(2000);

        builder.HasOne(a => a.Assessment)
            .WithMany(a => a.Approvals)
            .HasForeignKey(a => a.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.DecidedByUser)
            .WithMany()
            .HasForeignKey(a => a.DecidedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.OrganisationId);
        builder.HasIndex(a => a.AssessmentId);

        builder.Property(a => a.CreatedAt).IsRequired();
    }
}
