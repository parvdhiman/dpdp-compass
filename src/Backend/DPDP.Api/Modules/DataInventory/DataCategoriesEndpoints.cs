using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Application.Modules.DataInventory.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataInventory;

public sealed record CreateDataCategoryRequest(string Name, string? Description, string? ClassificationCategory);
public sealed record UpdateDataCategoryRequest(string Name, string? Description, string? ClassificationCategory, bool IsActive);

public static class DataCategoriesEndpoints
{
    public static IEndpointRouteBuilder MapDataCategoriesEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/data-categories")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (string? search, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataCategoriesQuery(search, isActive), ct)))
            .WithName("GetDataCategories").Produces<IReadOnlyList<DataCategoryDto>>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPost("/", async (CreateDataCategoryRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateDataCategoryCommand(request.Name, request.Description, request.ClassificationCategory), ct);
                return Results.Created($"/api/v1/data-categories/{result.Id}", result);
            })
            .WithName("CreateDataCategory").Produces<DataCategoryDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataCategoryByIdQuery(id), ct)))
            .WithName("GetDataCategoryById").Produces<DataCategoryDto>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateDataCategoryRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateDataCategoryCommand(id, request.Name, request.Description, request.ClassificationCategory, request.IsActive), ct)))
            .WithName("UpdateDataCategory").Produces<DataCategoryDto>().RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteDataCategoryCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteDataCategory").RequirePermission(PermissionKeys.DataInventoryManage);

        return app;
    }
}
