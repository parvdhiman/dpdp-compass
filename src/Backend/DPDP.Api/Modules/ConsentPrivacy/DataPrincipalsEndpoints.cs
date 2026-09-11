using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Application.Modules.ConsentPrivacy.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.ConsentPrivacy;

public sealed record CreateDataPrincipalRequest(string ExternalReferenceId, string? ReferenceCategory, string? Notes);
public sealed record UpdateDataPrincipalRequest(string ExternalReferenceId, string? ReferenceCategory, string? Notes, bool IsActive);

public static class DataPrincipalsEndpoints
{
    public static IEndpointRouteBuilder MapDataPrincipalsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/data-principals")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Consent & Privacy Operations")
            .RequireAuthorization();

        group.MapGet("/", async (string? search, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataPrincipalsQuery(search, isActive), ct)))
            .WithName("GetDataPrincipals").Produces<IReadOnlyList<DataPrincipalDto>>().RequirePermission(PermissionKeys.DataPrincipalsRead);

        group.MapPost("/", async (CreateDataPrincipalRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateDataPrincipalCommand(request.ExternalReferenceId, request.ReferenceCategory, request.Notes), ct);
                return Results.Created($"/api/v1/data-principals/{result.Id}", result);
            })
            .WithName("CreateDataPrincipal").Produces<DataPrincipalDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataPrincipalsManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataPrincipalByIdQuery(id), ct)))
            .WithName("GetDataPrincipalById").Produces<DataPrincipalDto>().RequirePermission(PermissionKeys.DataPrincipalsRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateDataPrincipalRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateDataPrincipalCommand(id, request.ExternalReferenceId, request.ReferenceCategory, request.Notes, request.IsActive), ct)))
            .WithName("UpdateDataPrincipal").Produces<DataPrincipalDto>().RequirePermission(PermissionKeys.DataPrincipalsManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteDataPrincipalCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteDataPrincipal").RequirePermission(PermissionKeys.DataPrincipalsManage);

        return app;
    }
}
