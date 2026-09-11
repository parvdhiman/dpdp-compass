using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Application.Modules.ConsentPrivacy.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.ConsentPrivacy;

public sealed record CreateSlaPolicyRequest(string Name, string? Description, string? RequestType, int ResponseDueDays);
public sealed record UpdateSlaPolicyRequest(string Name, string? Description, string? RequestType, int ResponseDueDays, bool IsActive);

/// <summary>Managed under the DataRequests permission pair — an SLA policy only ever governs Data Principal Request due dates, so it shares that permission scope rather than getting its own.</summary>
public static class SlaPoliciesEndpoints
{
    public static IEndpointRouteBuilder MapSlaPoliciesEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/sla-policies")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Consent & Privacy Operations")
            .RequireAuthorization();

        group.MapGet("/", async (string? search, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetSlaPoliciesQuery(search, isActive), ct)))
            .WithName("GetSlaPolicies").Produces<IReadOnlyList<SlaPolicyDto>>().RequirePermission(PermissionKeys.DataRequestsRead);

        group.MapPost("/", async (CreateSlaPolicyRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateSlaPolicyCommand(request.Name, request.Description, request.RequestType, request.ResponseDueDays), ct);
                return Results.Created($"/api/v1/sla-policies/{result.Id}", result);
            })
            .WithName("CreateSlaPolicy").Produces<SlaPolicyDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataRequestsManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetSlaPolicyByIdQuery(id), ct)))
            .WithName("GetSlaPolicyById").Produces<SlaPolicyDto>().RequirePermission(PermissionKeys.DataRequestsRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateSlaPolicyRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateSlaPolicyCommand(id, request.Name, request.Description, request.RequestType, request.ResponseDueDays, request.IsActive), ct)))
            .WithName("UpdateSlaPolicy").Produces<SlaPolicyDto>().RequirePermission(PermissionKeys.DataRequestsManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteSlaPolicyCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteSlaPolicy").RequirePermission(PermissionKeys.DataRequestsManage);

        return app;
    }
}
