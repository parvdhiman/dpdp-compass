using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Application.Modules.DataInventory.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataInventory;

public sealed record CreateItSystemRequest(string Name, string? Description, string SystemType, Guid? OwnerUserId);
public sealed record UpdateItSystemRequest(string Name, string? Description, string SystemType, Guid? OwnerUserId, bool IsActive);

public static class ItSystemsEndpoints
{
    public static IEndpointRouteBuilder MapItSystemsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/it-systems")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (string? search, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetItSystemsQuery(search, isActive), ct)))
            .WithName("GetItSystems").Produces<IReadOnlyList<ItSystemDto>>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPost("/", async (CreateItSystemRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateItSystemCommand(request.Name, request.Description, request.SystemType, request.OwnerUserId), ct);
                return Results.Created($"/api/v1/it-systems/{result.Id}", result);
            })
            .WithName("CreateItSystem").Produces<ItSystemDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetItSystemByIdQuery(id), ct)))
            .WithName("GetItSystemById").Produces<ItSystemDto>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateItSystemRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateItSystemCommand(id, request.Name, request.Description, request.SystemType, request.OwnerUserId, request.IsActive), ct)))
            .WithName("UpdateItSystem").Produces<ItSystemDto>().RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteItSystemCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteItSystem").RequirePermission(PermissionKeys.DataInventoryManage);

        return app;
    }
}
