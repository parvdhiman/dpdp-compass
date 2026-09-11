using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Application.Modules.DataInventory.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataInventory;

public sealed record CreateRecipientRequest(string Name, string? Description, string RecipientType);
public sealed record UpdateRecipientRequest(string Name, string? Description, string RecipientType, bool IsActive);

public static class RecipientsEndpoints
{
    public static IEndpointRouteBuilder MapRecipientsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/recipients")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (string? search, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRecipientsQuery(search, isActive), ct)))
            .WithName("GetRecipients").Produces<IReadOnlyList<RecipientDto>>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPost("/", async (CreateRecipientRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateRecipientCommand(request.Name, request.Description, request.RecipientType), ct);
                return Results.Created($"/api/v1/recipients/{result.Id}", result);
            })
            .WithName("CreateRecipient").Produces<RecipientDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRecipientByIdQuery(id), ct)))
            .WithName("GetRecipientById").Produces<RecipientDto>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateRecipientRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateRecipientCommand(id, request.Name, request.Description, request.RecipientType, request.IsActive), ct)))
            .WithName("UpdateRecipient").Produces<RecipientDto>().RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteRecipientCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteRecipient").RequirePermission(PermissionKeys.DataInventoryManage);

        return app;
    }
}
