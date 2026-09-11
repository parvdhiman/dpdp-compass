namespace DPDP.Infrastructure.Persistence.Configurations;

/// <summary>
/// A fixed instant used only for HasData seed rows' created_at columns.
/// Must never be DateTimeOffset.UtcNow — EF Core's migration model
/// comparer needs a value that is identical every time the model is built,
/// or every migration-add run would generate a spurious no-op migration.
/// </summary>
internal static class SeedClock
{
    public static readonly DateTimeOffset Timestamp = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
