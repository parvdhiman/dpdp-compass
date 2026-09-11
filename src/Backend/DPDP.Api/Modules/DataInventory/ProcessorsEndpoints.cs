using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Application.Modules.DataInventory.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataInventory;

public sealed record CreateProcessorRequest(string Name, string? Description, string? ContactEmail, string? Country);
public sealed record UpdateProcessorRequest(string Name, string? Description, string? ContactEmail, string? Country, bool IsActive);

public static class ProcessorsEndpoints
{
    public static IEndpointRouteBuilder MapProcessorsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/processors")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (string? search, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetProcessorsQuery(search, isActive), ct)))
            .WithName("GetProcessors").Produces<IReadOnlyList<ProcessorDto>>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPost("/", async (CreateProcessorRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateProcessorCommand(request.Name, request.Description, request.ContactEmail, request.Country), ct);
                return Results.Created($"/api/v1/processors/{result.Id}", result);
            })
            .WithName("CreateProcessor").Produces<ProcessorDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetProcessorByIdQuery(id), ct)))
            .WithName("GetProcessorById").Produces<ProcessorDto>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProcessorRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateProcessorCommand(id, request.Name, request.Description, request.ContactEmail, request.Country, request.IsActive), ct)))
            .WithName("UpdateProcessor").Produces<ProcessorDto>().RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteProcessorCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteProcessor").RequirePermission(PermissionKeys.DataInventoryManage);

        return app;
    }
}
