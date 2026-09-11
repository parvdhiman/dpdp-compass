using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Application.Modules.DataInventory.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataInventory;

public sealed record CreateDataCollectionSourceRequest(string Name, string? Description, string SourceType);
public sealed record UpdateDataCollectionSourceRequest(string Name, string? Description, string SourceType, bool IsActive);

public static class DataCollectionSourcesEndpoints
{
    public static IEndpointRouteBuilder MapDataCollectionSourcesEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/data-collection-sources")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (string? search, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataCollectionSourcesQuery(search, isActive), ct)))
            .WithName("GetDataCollectionSources").Produces<IReadOnlyList<DataCollectionSourceDto>>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPost("/", async (CreateDataCollectionSourceRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateDataCollectionSourceCommand(request.Name, request.Description, request.SourceType), ct);
                return Results.Created($"/api/v1/data-collection-sources/{result.Id}", result);
            })
            .WithName("CreateDataCollectionSource").Produces<DataCollectionSourceDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataCollectionSourceByIdQuery(id), ct)))
            .WithName("GetDataCollectionSourceById").Produces<DataCollectionSourceDto>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateDataCollectionSourceRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateDataCollectionSourceCommand(id, request.Name, request.Description, request.SourceType, request.IsActive), ct)))
            .WithName("UpdateDataCollectionSource").Produces<DataCollectionSourceDto>().RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteDataCollectionSourceCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteDataCollectionSource").RequirePermission(PermissionKeys.DataInventoryManage);

        return app;
    }
}
