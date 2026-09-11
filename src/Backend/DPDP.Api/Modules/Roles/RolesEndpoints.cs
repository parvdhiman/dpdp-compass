using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.Identity.Commands.AssignRolePermission;
using DPDP.Application.Modules.Identity.Commands.RevokeRolePermission;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Application.Modules.Identity.Queries.GetRoles;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Roles;

public static class RolesEndpoints
{
    public static IEndpointRouteBuilder MapRolesEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Roles")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRolesQuery(), ct)))
            .WithName("GetRoles")
            .Produces<IReadOnlyList<RoleDto>>()
            .RequirePermission(PermissionKeys.RolesRead);

        // Role permission templates are global (shared across every
        // tenant) — the handler itself further restricts this to Super
        // Administrator regardless of who holds roles.manage, since
        // roles.manage also covers the much narrower, org-scoped action of
        // assigning an existing role to a user. See AssignRolePermissionCommand.
        group.MapPost("/{roleId:guid}/permissions/{permissionId:guid}", async (Guid roleId, Guid permissionId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new AssignRolePermissionCommand(roleId, permissionId), ct);
                return Results.NoContent();
            })
            .WithName("AssignRolePermission")
            .RequirePermission(PermissionKeys.RolesManage);

        group.MapDelete("/{roleId:guid}/permissions/{permissionId:guid}", async (Guid roleId, Guid permissionId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RevokeRolePermissionCommand(roleId, permissionId), ct);
                return Results.NoContent();
            })
            .WithName("RevokeRolePermission")
            .RequirePermission(PermissionKeys.RolesManage);

        return app;
    }
}
