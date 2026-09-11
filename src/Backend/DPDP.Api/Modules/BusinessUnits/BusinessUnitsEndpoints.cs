using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Organisations.Commands.CreateBusinessUnit;
using DPDP.Application.Modules.Organisations.Commands.DeleteBusinessUnit;
using DPDP.Application.Modules.Organisations.Commands.UpdateBusinessUnit;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Application.Modules.Organisations.Queries.GetBusinessUnitById;
using DPDP.Application.Modules.Organisations.Queries.GetBusinessUnits;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.BusinessUnits;

public sealed record CreateBusinessUnitRequest(Guid OrganisationId, string Name, string? Description, string? HeadName, string? HeadEmail, string? HeadPhone);
public sealed record UpdateBusinessUnitRequest(string Name, string? Description, string? HeadName, string? HeadEmail, string? HeadPhone, bool IsActive);

public static class BusinessUnitsEndpoints
{
    public static IEndpointRouteBuilder MapBusinessUnitsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/business-units")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("BusinessUnits")
            .RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, string? search, bool? isActive, string? sortBy, bool? sortDescending, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetBusinessUnitsQuery(page ?? 1, pageSize ?? 25, search, isActive, sortBy ?? "name", sortDescending ?? false), ct)))
            .WithName("GetBusinessUnits")
            .Produces<PagedResult<BusinessUnitDto>>()
            .RequirePermission(PermissionKeys.BusinessUnitsRead);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetBusinessUnitByIdQuery(id), ct)))
            .WithName("GetBusinessUnitById")
            .Produces<BusinessUnitDto>()
            .RequirePermission(PermissionKeys.BusinessUnitsRead);

        group.MapPost("/", async (CreateBusinessUnitRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateBusinessUnitCommand(
                    request.OrganisationId, request.Name, request.Description, request.HeadName, request.HeadEmail, request.HeadPhone), ct);
                return Results.Created($"/api/v1/business-units/{result.Id}", result);
            })
            .WithName("CreateBusinessUnit")
            .Produces<BusinessUnitDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.BusinessUnitsCreate);

        group.MapPut("/{id:guid}", async (Guid id, UpdateBusinessUnitRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateBusinessUnitCommand(
                    id, request.Name, request.Description, request.HeadName, request.HeadEmail, request.HeadPhone, request.IsActive), ct)))
            .WithName("UpdateBusinessUnit")
            .Produces<BusinessUnitDto>()
            .RequirePermission(PermissionKeys.BusinessUnitsUpdate);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteBusinessUnitCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteBusinessUnit")
            .RequirePermission(PermissionKeys.BusinessUnitsDelete);

        return app;
    }
}
