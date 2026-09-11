using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Identity.Commands.AssignUserRole;
using DPDP.Application.Modules.Identity.Commands.CreateUser;
using DPDP.Application.Modules.Identity.Commands.RevokeUserRole;
using DPDP.Application.Modules.Identity.Commands.SetUserActive;
using DPDP.Application.Modules.Identity.Commands.UpdateUser;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Application.Modules.Identity.Queries.GetUserById;
using DPDP.Application.Modules.Identity.Queries.GetUsers;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Users;

public sealed record CreateUserRequest(
    Guid OrganisationId,
    string Email,
    string FullName,
    string? PhoneNumber,
    string Password,
    IReadOnlyList<Guid> RoleIds);

public sealed record UpdateUserRequest(string FullName, string? PhoneNumber);

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/users")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, string? search, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetUsersQuery(page ?? 1, pageSize ?? 25, search), ct)))
            .WithName("GetUsers")
            .Produces<PagedResult<UserDto>>()
            .RequirePermission(PermissionKeys.UsersRead);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetUserByIdQuery(id), ct)))
            .WithName("GetUserById")
            .Produces<UserDto>()
            .RequirePermission(PermissionKeys.UsersRead);

        group.MapPost("/", async (CreateUserRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateUserCommand(
                    request.OrganisationId, request.Email, request.FullName, request.PhoneNumber, request.Password, request.RoleIds), ct);
                return Results.Created($"/api/v1/users/{result.Id}", result);
            })
            .WithName("CreateUser")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.UsersCreate);

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateUserCommand(id, request.FullName, request.PhoneNumber), ct)))
            .WithName("UpdateUser")
            .Produces<UserDto>()
            .RequirePermission(PermissionKeys.UsersUpdate);

        group.MapPost("/{id:guid}/activate", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new SetUserActiveCommand(id, true), ct);
                return Results.NoContent();
            })
            .WithName("ActivateUser")
            .RequirePermission(PermissionKeys.UsersDisable);

        group.MapPost("/{id:guid}/deactivate", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new SetUserActiveCommand(id, false), ct);
                return Results.NoContent();
            })
            .WithName("DeactivateUser")
            .RequirePermission(PermissionKeys.UsersDisable);

        group.MapPost("/{id:guid}/roles/{roleId:guid}", async (Guid id, Guid roleId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new AssignUserRoleCommand(id, roleId), ct);
                return Results.NoContent();
            })
            .WithName("AssignUserRole")
            .RequirePermission(PermissionKeys.RolesManage);

        group.MapDelete("/{id:guid}/roles/{roleId:guid}", async (Guid id, Guid roleId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RevokeUserRoleCommand(id, roleId), ct);
                return Results.NoContent();
            })
            .WithName("RevokeUserRole")
            .RequirePermission(PermissionKeys.RolesManage);

        return app;
    }
}
