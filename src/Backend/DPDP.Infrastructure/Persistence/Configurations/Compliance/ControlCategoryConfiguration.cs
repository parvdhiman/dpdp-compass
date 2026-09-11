using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPDP.Infrastructure.Persistence.Configurations.Compliance;

public sealed class ControlCategoryConfiguration : IEntityTypeConfiguration<ControlCategory>
{
    public static Guid CategoryId(string key) => DeterministicGuid.Create($"control-category:{key}");

    public void Configure(EntityTypeBuilder<ControlCategory> builder)
    {
        builder.ToTable("control_categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasData(DpdpActSeedData.Categories.Select(c => new ControlCategory
        {
            Id = CategoryId(c.Key),
            Name = c.Name,
            Description = c.Description,
            SortOrder = c.SortOrder,
            CreatedAt = SeedClock.Timestamp,
        }));
    }
}
