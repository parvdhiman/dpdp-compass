using System.Text;
using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Application.Modules.DataInventory.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataInventory;

public sealed record CreateDataInventoryItemRequest(
    Guid DataCategoryId, string DataElementName, Guid? DiscoveredDataElementId, string? Classification,
    Guid? DataCollectionSourceId, Guid? ItSystemId, Guid? OwnerUserId, string? Purpose,
    Guid? RetentionPolicyId, string? SharingDescription, Guid? ProcessorId, string? RiskLevel);

public sealed record UpdateDataInventoryItemRequest(
    Guid DataCategoryId, string DataElementName, string? Classification,
    Guid? DataCollectionSourceId, Guid? ItSystemId, Guid? OwnerUserId, string? Purpose,
    Guid? RetentionPolicyId, string? SharingDescription, Guid? ProcessorId, string? RiskLevel);

public static class DataInventoryItemsEndpoints
{
    public static IEndpointRouteBuilder MapDataInventoryItemsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/data-inventory")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (
                int? page, int? pageSize, string? search, Guid? dataCategoryId, Guid? itSystemId, Guid? processorId,
                string? classification, string? riskLevel, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataInventoryItemsQuery(
                    page ?? 1, pageSize ?? 25, search, dataCategoryId, itSystemId, processorId, classification, riskLevel), ct)))
            .WithName("GetDataInventoryItems").Produces<PagedResult<DataInventoryItemDto>>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapGet("/export", async (
                string? search, Guid? dataCategoryId, Guid? itSystemId, Guid? processorId, string? classification, string? riskLevel,
                ISender sender, CancellationToken ct) =>
            {
                var csv = await sender.Send(new ExportDataInventoryQuery(search, dataCategoryId, itSystemId, processorId, classification, riskLevel), ct);
                return Results.File(Encoding.UTF8.GetBytes(csv), "text/csv", $"data-inventory-{DateTime.UtcNow:yyyyMMdd}.csv");
            })
            .WithName("ExportDataInventory").RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPost("/", async (CreateDataInventoryItemRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateDataInventoryItemCommand(
                    request.DataCategoryId, request.DataElementName, request.DiscoveredDataElementId, request.Classification,
                    request.DataCollectionSourceId, request.ItSystemId, request.OwnerUserId, request.Purpose,
                    request.RetentionPolicyId, request.SharingDescription, request.ProcessorId, request.RiskLevel), ct);
                return Results.Created($"/api/v1/data-inventory/{result.Id}", result);
            })
            .WithName("CreateDataInventoryItem").Produces<DataInventoryItemDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataInventoryItemByIdQuery(id), ct)))
            .WithName("GetDataInventoryItemById").Produces<DataInventoryItemDto>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateDataInventoryItemRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateDataInventoryItemCommand(
                    id, request.DataCategoryId, request.DataElementName, request.Classification,
                    request.DataCollectionSourceId, request.ItSystemId, request.OwnerUserId, request.Purpose,
                    request.RetentionPolicyId, request.SharingDescription, request.ProcessorId, request.RiskLevel), ct)))
            .WithName("UpdateDataInventoryItem").Produces<DataInventoryItemDto>().RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteDataInventoryItemCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteDataInventoryItem").RequirePermission(PermissionKeys.DataInventoryManage);

        return app;
    }
}
