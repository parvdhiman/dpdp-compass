using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataDiscovery.Commands;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Application.Modules.DataDiscovery.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataDiscovery;

public sealed record CreateDataSourceRequest(
    string Name, string? Description, string SourceType,
    string? Host, int? Port, string? DatabaseName, string? Username, string? Password, string? RootPath, string? SchemaFilter);

public sealed record UpdateDataSourceRequest(
    string Name, string? Description, string? Host, int? Port, string? DatabaseName,
    string? Username, string? Password, string? RootPath, string? SchemaFilter, bool IsActive);

public static class DataSourcesEndpoints
{
    public static IEndpointRouteBuilder MapDataSourcesEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/data-sources")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Discovery")
            .RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, string? search, string? sourceType, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataSourcesQuery(page ?? 1, pageSize ?? 25, search, sourceType, isActive), ct)))
            .WithName("GetDataSources")
            .Produces<PagedResult<DataSourceSummaryDto>>()
            .RequirePermission(PermissionKeys.DataSourcesRead);

        group.MapPost("/", async (CreateDataSourceRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateDataSourceCommand(
                    request.Name, request.Description, request.SourceType, request.Host, request.Port,
                    request.DatabaseName, request.Username, request.Password, request.RootPath, request.SchemaFilter), ct);
                return Results.Created($"/api/v1/data-sources/{result.Id}", result);
            })
            .WithName("CreateDataSource")
            .Produces<DataSourceDetailDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.DataSourcesManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataSourceByIdQuery(id), ct)))
            .WithName("GetDataSourceById")
            .Produces<DataSourceDetailDto>()
            .RequirePermission(PermissionKeys.DataSourcesRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateDataSourceRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateDataSourceCommand(
                    id, request.Name, request.Description, request.Host, request.Port,
                    request.DatabaseName, request.Username, request.Password, request.RootPath, request.SchemaFilter, request.IsActive), ct)))
            .WithName("UpdateDataSource")
            .Produces<DataSourceDetailDto>()
            .RequirePermission(PermissionKeys.DataSourcesManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteDataSourceCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteDataSource")
            .RequirePermission(PermissionKeys.DataSourcesManage);

        group.MapPost("/{id:guid}/test-connection", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new TestDataSourceConnectionCommand(id), ct)))
            .WithName("TestDataSourceConnection")
            .Produces<TestDataSourceConnectionResultDto>()
            .RequirePermission(PermissionKeys.DataSourcesManage);

        group.MapPost("/{id:guid}/discovery-jobs", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new StartDiscoveryJobCommand(id), ct);
                return Results.Created($"/api/v1/discovery-jobs/{result.Id}", result);
            })
            .WithName("StartDiscoveryJob")
            .Produces<DiscoveryJobDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.DiscoveryJobsManage);

        return app;
    }
}
