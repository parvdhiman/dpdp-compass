namespace DPDP.Application.Modules.DataDiscovery.DTOs;

/// <summary>Neither DTO ever includes the connection secret — see docs/DATA_DISCOVERY.md section 2.</summary>
public sealed record DataSourceSummaryDto(
    Guid Id, string Name, string SourceType, bool IsActive,
    DateTimeOffset? LastTestedAt, bool? LastTestSucceeded, DateTimeOffset CreatedAt);

public sealed record DataSourceDetailDto(
    Guid Id, string Name, string? Description, string SourceType,
    string? Host, int? Port, string? DatabaseName, string? Username, string? RootPath, string? SchemaFilter,
    bool IsActive, DateTimeOffset? LastTestedAt, bool? LastTestSucceeded, string? LastTestError,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
