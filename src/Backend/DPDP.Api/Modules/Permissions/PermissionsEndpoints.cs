using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Application.Modules.Identity.Queries.GetPermissions;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Permissions;



public static class PermissionsEndpoints
{
    public static IEndpointRouteBuilder MapPermissionsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Permissions")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetPermissionsQuery(), ct)))
            .WithName("GetPermissions")
            .Produces<IReadOnlyList<PermissionDto>>()
            .RequirePermission(PermissionKeys.RolesRead);

        return app;
    }
}
