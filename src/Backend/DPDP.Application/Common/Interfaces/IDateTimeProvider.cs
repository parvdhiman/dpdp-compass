namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// Abstraction over "now" so handlers stay unit-testable without wall-clock
/// dependencies. All timestamps in the system are UTC — see docs/DATABASE.md.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
