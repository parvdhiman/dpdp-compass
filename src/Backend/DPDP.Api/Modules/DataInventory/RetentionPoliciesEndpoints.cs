using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Application.Modules.DataInventory.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataInventory;

public sealed record CreateRetentionPolicyRequest(string Name, string? Description, int RetentionPeriodValue, string RetentionPeriodUnit, string? TriggerEvent);
public sealed record UpdateRetentionPolicyRequest(string Name, string? Description, int RetentionPeriodValue, string RetentionPeriodUnit, string? TriggerEvent, bool IsActive);

public static class RetentionPoliciesEndpoints
{
    public static IEndpointRouteBuilder MapRetentionPoliciesEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/retention-policies")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (string? search, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRetentionPoliciesQuery(search, isActive), ct)))
            .WithName("GetRetentionPolicies").Produces<IReadOnlyList<RetentionPolicyDto>>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPost("/", async (CreateRetentionPolicyRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateRetentionPolicyCommand(request.Name, request.Description, request.RetentionPeriodValue, request.RetentionPeriodUnit, request.TriggerEvent), ct);
                return Results.Created($"/api/v1/retention-policies/{result.Id}", result);
            })
            .WithName("CreateRetentionPolicy").Produces<RetentionPolicyDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRetentionPolicyByIdQuery(id), ct)))
            .WithName("GetRetentionPolicyById").Produces<RetentionPolicyDto>().RequirePermission(PermissionKeys.DataInventoryRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateRetentionPolicyRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateRetentionPolicyCommand(id, request.Name, request.Description, request.RetentionPeriodValue, request.RetentionPeriodUnit, request.TriggerEvent, request.IsActive), ct)))
            .WithName("UpdateRetentionPolicy").Produces<RetentionPolicyDto>().RequirePermission(PermissionKeys.DataInventoryManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteRetentionPolicyCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteRetentionPolicy").RequirePermission(PermissionKeys.DataInventoryManage);

        return app;
    }
}
