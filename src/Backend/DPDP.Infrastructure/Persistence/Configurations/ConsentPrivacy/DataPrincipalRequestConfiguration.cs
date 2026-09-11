using DPDP.Domain.Modules.ConsentPrivacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.ConsentPrivacy;

public sealed class DataPrincipalRequestConfiguration : IEntityTypeConfiguration<DataPrincipalRequest>
{
    public void Configure(EntityTypeBuilder<DataPrincipalRequest> builder)
    {
        builder.ToTable("data_principal_requests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.SequenceNumber).UseIdentityColumn();
        builder.HasIndex(r => new { r.OrganisationId, r.SequenceNumber }).IsUnique();

        builder.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(r => r.RequesterName).HasMaxLength(300).IsRequired();
        builder.Property(r => r.RequesterContactEmail).HasMaxLength(300);
        builder.Property(r => r.RequesterContactPhone).HasMaxLength(50);
        builder.Property(r => r.ExternalReferenceId).HasMaxLength(300);
        builder.Property(r => r.Description).HasMaxLength(4000);
        builder.Property(r => r.ResolutionNotes).HasMaxLength(4000);
        builder.Property(r => r.RejectionReason).HasMaxLength(2000);

        builder.HasOne(r => r.DataPrincipal).WithMany().HasForeignKey(r => r.DataPrincipalId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(r => r.RelatedConsent).WithMany().HasForeignKey(r => r.RelatedConsentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(r => r.SlaPolicy).WithMany().HasForeignKey(r => r.SlaPolicyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(r => r.AssignedToUser).WithMany().HasForeignKey(r => r.AssignedToUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.OrganisationId);
        builder.HasIndex(r => new { r.OrganisationId, r.Status });
        builder.HasIndex(r => new { r.OrganisationId, r.RequestType });
        builder.HasIndex(r => r.DueAt);

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();
    }
}
