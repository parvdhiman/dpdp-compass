using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Organisations.Commands.CreateDepartment;
using DPDP.Application.Modules.Organisations.Commands.DeleteDepartment;
using DPDP.Application.Modules.Organisations.Commands.UpdateDepartment;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Application.Modules.Organisations.Queries.GetDepartmentById;
using DPDP.Application.Modules.Organisations.Queries.GetDepartments;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Departments;

public sealed record CreateDepartmentRequest(Guid BusinessUnitId, string Name, string? Description, string? HeadName, string? HeadEmail, string? HeadPhone);
public sealed record UpdateDepartmentRequest(string Name, string? Description, string? HeadName, string? HeadEmail, string? HeadPhone, bool IsActive);

public static class DepartmentsEndpoints
{
    public static IEndpointRouteBuilder MapDepartmentsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/departments")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Departments")
            .RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, string? search, Guid? businessUnitId, bool? isActive, string? sortBy, bool? sortDescending, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDepartmentsQuery(page ?? 1, pageSize ?? 25, search, businessUnitId, isActive, sortBy ?? "name", sortDescending ?? false), ct)))
            .WithName("GetDepartments")
            .Produces<PagedResult<DepartmentDto>>()
            .RequirePermission(PermissionKeys.DepartmentsRead);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDepartmentByIdQuery(id), ct)))
            .WithName("GetDepartmentById")
            .Produces<DepartmentDto>()
            .RequirePermission(PermissionKeys.DepartmentsRead);

        group.MapPost("/", async (CreateDepartmentRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateDepartmentCommand(
                    request.BusinessUnitId, request.Name, request.Description, request.HeadName, request.HeadEmail, request.HeadPhone), ct);
                return Results.Created($"/api/v1/departments/{result.Id}", result);
            })
            .WithName("CreateDepartment")
            .Produces<DepartmentDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.DepartmentsCreate);

        group.MapPut("/{id:guid}", async (Guid id, UpdateDepartmentRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateDepartmentCommand(
                    id, request.Name, request.Description, request.HeadName, request.HeadEmail, request.HeadPhone, request.IsActive), ct)))
            .WithName("UpdateDepartment")
            .Produces<DepartmentDto>()
            .RequirePermission(PermissionKeys.DepartmentsUpdate);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteDepartmentCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteDepartment")
            .RequirePermission(PermissionKeys.DepartmentsDelete);

        return app;
    }
}
