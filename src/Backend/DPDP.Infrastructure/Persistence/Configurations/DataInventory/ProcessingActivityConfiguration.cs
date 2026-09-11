using DPDP.Domain.Modules.DataInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.DataInventory;

public sealed class ProcessingActivityConfiguration : IEntityTypeConfiguration<ProcessingActivity>
{
    public void Configure(EntityTypeBuilder<ProcessingActivity> builder)
    {
        builder.ToTable("processing_activities");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.SequenceNumber).UseIdentityColumn();
        builder.HasIndex(a => new { a.OrganisationId, a.SequenceNumber }).IsUnique();

        builder.Property(a => a.Name).HasMaxLength(300).IsRequired();
        builder.Property(a => a.Purpose).HasMaxLength(2000).IsRequired();
        builder.Property(a => a.DataSubjectCategoriesJson).HasColumnType("jsonb");
        builder.Property(a => a.SecurityControlsJson).HasColumnType("jsonb");
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.ReviewComments).HasMaxLength(2000);

        builder.HasOne(a => a.RetentionPolicy).WithMany().HasForeignKey(a => a.RetentionPolicyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.Owner).WithMany().HasForeignKey(a => a.OwnerUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.OrganisationId);
        builder.HasIndex(a => new { a.OrganisationId, a.Status });

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        // Five independent many-to-many catalogues, per the brief's
        // relationship diagram — plain implicit join tables (no extra
        // columns needed on the relationship itself, so no dedicated join
        // entity class), each named explicitly rather than left to
        // EF Core's default naming.
        builder.HasMany(a => a.DataCategories).WithMany()
            .UsingEntity(j => j.ToTable("processing_activity_data_categories"));
        builder.HasMany(a => a.ItSystems).WithMany()
            .UsingEntity(j => j.ToTable("processing_activity_it_systems"));
        builder.HasMany(a => a.DataCollectionSources).WithMany()
            .UsingEntity(j => j.ToTable("processing_activity_data_collection_sources"));
        builder.HasMany(a => a.Recipients).WithMany()
            .UsingEntity(j => j.ToTable("processing_activity_recipients"));
        builder.HasMany(a => a.Processors).WithMany()
            .UsingEntity(j => j.ToTable("processing_activity_processors"));
    }
}
