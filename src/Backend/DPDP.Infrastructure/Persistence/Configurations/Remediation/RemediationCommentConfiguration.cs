using DPDP.Domain.Modules.Remediation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Remediation;

public sealed class RemediationCommentConfiguration : IEntityTypeConfiguration<RemediationComment>
{
    public void Configure(EntityTypeBuilder<RemediationComment> builder)
    {
        builder.ToTable("remediation_comments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Comment).HasMaxLength(2000).IsRequired();

        builder.HasOne(c => c.RemediationTask).WithMany(t => t.Comments).HasForeignKey(c => c.RemediationTaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Author).WithMany().HasForeignKey(c => c.AuthorUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.OrganisationId);
        builder.HasIndex(c => c.RemediationTaskId);

        builder.Property(c => c.CreatedAt).IsRequired();
    }
}
