using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Application.Modules.ConsentPrivacy.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.ConsentPrivacy;

public sealed record CreateConsentPurposeRequest(string Name, string? Description, Guid? DataCategoryId);
public sealed record UpdateConsentPurposeRequest(string Name, string? Description, Guid? DataCategoryId, bool IsActive);

public static class ConsentPurposesEndpoints
{
    public static IEndpointRouteBuilder MapConsentPurposesEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/consent-purposes")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Consent & Privacy Operations")
            .RequireAuthorization();

        group.MapGet("/", async (string? search, bool? isActive, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetConsentPurposesQuery(search, isActive), ct)))
            .WithName("GetConsentPurposes").Produces<IReadOnlyList<ConsentPurposeDto>>().RequirePermission(PermissionKeys.ConsentPurposesRead);

        group.MapPost("/", async (CreateConsentPurposeRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateConsentPurposeCommand(request.Name, request.Description, request.DataCategoryId), ct);
                return Results.Created($"/api/v1/consent-purposes/{result.Id}", result);
            })
            .WithName("CreateConsentPurpose").Produces<ConsentPurposeDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.ConsentPurposesManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetConsentPurposeByIdQuery(id), ct)))
            .WithName("GetConsentPurposeById").Produces<ConsentPurposeDto>().RequirePermission(PermissionKeys.ConsentPurposesRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateConsentPurposeRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateConsentPurposeCommand(id, request.Name, request.Description, request.DataCategoryId, request.IsActive), ct)))
            .WithName("UpdateConsentPurpose").Produces<ConsentPurposeDto>().RequirePermission(PermissionKeys.ConsentPurposesManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteConsentPurposeCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteConsentPurpose").RequirePermission(PermissionKeys.ConsentPurposesManage);

        return app;
    }
}
