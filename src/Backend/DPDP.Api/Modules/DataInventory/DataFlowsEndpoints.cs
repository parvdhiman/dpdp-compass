using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Application.Modules.DataInventory.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataInventory;

public sealed record CreateDataFlowRequest(
    string Name, string? Description, Guid? ProcessingActivityId, Guid? DataCategoryId,
    Guid? FromItSystemId, Guid? FromDataCollectionSourceId, string FromDescription,
    Guid? ToItSystemId, Guid? ToProcessorId, Guid? ToRecipientId, string ToDescription,
    string? TransferMechanism, bool IsCrossBorder, string? CrossBorderCountry);

public sealed record UpdateDataFlowRequest(
    string Name, string? Description, Guid? ProcessingActivityId, Guid? DataCategoryId,
    Guid? FromItSystemId, Guid? FromDataCollectionSourceId, string FromDescription,
    Guid? ToItSystemId, Guid? ToProcessorId, Guid? ToRecipientId, string ToDescription,
    string? TransferMechanism, bool IsCrossBorder, string? CrossBorderCountry);

public static class DataFlowsEndpoints
{
    public static IEndpointRouteBuilder MapDataFlowsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/data-flows")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, string? search, Guid? processingActivityId, bool? crossBorderOnly, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataFlowsQuery(page ?? 1, pageSize ?? 25, search, processingActivityId, crossBorderOnly), ct)))
            .WithName("GetDataFlows").Produces<PagedResult<DataFlowDto>>().RequirePermission(PermissionKeys.DataFlowsRead);

        group.MapPost("/", async (CreateDataFlowRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateDataFlowCommand(
                    request.Name, request.Description, request.ProcessingActivityId, request.DataCategoryId,
                    request.FromItSystemId, request.FromDataCollectionSourceId, request.FromDescription,
                    request.ToItSystemId, request.ToProcessorId, request.ToRecipientId, request.ToDescription,
                    request.TransferMechanism, request.IsCrossBorder, request.CrossBorderCountry), ct);
                return Results.Created($"/api/v1/data-flows/{result.Id}", result);
            })
            .WithName("CreateDataFlow").Produces<DataFlowDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataFlowsManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataFlowByIdQuery(id), ct)))
            .WithName("GetDataFlowById").Produces<DataFlowDto>().RequirePermission(PermissionKeys.DataFlowsRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateDataFlowRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateDataFlowCommand(
                    id, request.Name, request.Description, request.ProcessingActivityId, request.DataCategoryId,
                    request.FromItSystemId, request.FromDataCollectionSourceId, request.FromDescription,
                    request.ToItSystemId, request.ToProcessorId, request.ToRecipientId, request.ToDescription,
                    request.TransferMechanism, request.IsCrossBorder, request.CrossBorderCountry), ct)))
            .WithName("UpdateDataFlow").Produces<DataFlowDto>().RequirePermission(PermissionKeys.DataFlowsManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteDataFlowCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteDataFlow").RequirePermission(PermissionKeys.DataFlowsManage);

        return app;
    }
}
