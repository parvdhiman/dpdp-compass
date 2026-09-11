using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.System.Queries.GetSystemInfo;
using Xunit;

namespace DPDP.UnitTests.Application;

public class GetSystemInfoQueryHandlerTests
{
    private sealed class FakeApplicationInfo : IApplicationInfo
    {
        public string ApplicationName => "DPDP-COMPASS";
        public string Version => "0.1.0-test";
        public string EnvironmentName => "UnitTest";
    }

    private sealed class FakeDateTimeProvider(DateTimeOffset now) : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => now;
    }

    [Fact]
    public async Task Handle_returns_application_facts_and_never_exposes_secrets()
    {
        var now = new DateTimeOffset(2026, 9, 7, 12, 0, 0, TimeSpan.Zero);
        var handler = new GetSystemInfoQueryHandler(new FakeApplicationInfo(), new FakeDateTimeProvider(now));

        var result = await handler.Handle(new GetSystemInfoQuery(), CancellationToken.None);

        Assert.Equal("DPDP-COMPASS", result.ApplicationName);
        Assert.Equal("0.1.0-test", result.Version);
        Assert.Equal("UnitTest", result.Environment);
        Assert.Equal(now, result.ServerTimeUtc);

        var dtoProperties = typeof(SystemInfoDto).GetProperties().Select(p => p.Name);
        Assert.DoesNotContain(dtoProperties, name =>
            name.Contains("Secret", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Key", StringComparison.OrdinalIgnoreCase));
    }
}
