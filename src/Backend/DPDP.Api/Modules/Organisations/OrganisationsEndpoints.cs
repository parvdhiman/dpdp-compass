using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Organisations.Commands.CreateOrganisation;
using DPDP.Application.Modules.Organisations.Commands.CreateOrganisationLocation;
using DPDP.Application.Modules.Organisations.Commands.DeleteOrganisation;
using DPDP.Application.Modules.Organisations.Commands.DeleteOrganisationLocation;
using DPDP.Application.Modules.Organisations.Commands.UpdateOrganisationLocation;
using DPDP.Application.Modules.Organisations.Commands.UpdateOrganisationProfile;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Application.Modules.Organisations.Queries.GetOrganisationDashboard;
using DPDP.Application.Modules.Organisations.Queries.GetOrganisationProfile;
using DPDP.Application.Modules.Organisations.Queries.GetOrganisations;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Organisations;

public sealed record CreateOrganisationRequest(
    string Name, string? LegalName, string? Industry, string? Size, string? Country, string? Website,
    string? PrimaryContactName, string? PrimaryContactEmail, string? PrimaryContactPhone);

public sealed record UpdateOrganisationProfileRequest(
    string Name, string? LegalName, string? Industry, string? Size, string? Country, string? Website,
    string? PrimaryContactName, string? PrimaryContactEmail, string? PrimaryContactPhone,
    string? PrivacyContactName, string? PrivacyContactEmail, string? PrivacyContactPhone,
    string? DpoName, string? DpoEmail, string? DpoPhone);

public sealed record LocationRequest(
    string Label, string? AddressLine1, string? AddressLine2, string? City, string? State,
    string? PostalCode, string? Country, bool IsPrimary);

public static class OrganisationsEndpoints
{
    public static IEndpointRouteBuilder MapOrganisationsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/organisations")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Organisations")
            .RequireAuthorization();

        // Cross-tenant listing — Super Administrator only, enforced in the
        // handler itself (see GetOrganisationsQuery's doc comment).
        group.MapGet("/", async (int? page, int? pageSize, string? search, string? status, string? sortBy, bool? sortDescending, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetOrganisationsQuery(page ?? 1, pageSize ?? 25, search, status, sortBy ?? "name", sortDescending ?? false), ct)))
            .WithName("GetOrganisations")
            .Produces<PagedResult<OrganisationSummaryDto>>()
            .RequirePermission(PermissionKeys.OrganisationRead);

        group.MapPost("/", async (CreateOrganisationRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateOrganisationCommand(
                    request.Name, request.LegalName, request.Industry, request.Size, request.Country, request.Website,
                    request.PrimaryContactName, request.PrimaryContactEmail, request.PrimaryContactPhone), ct);
                return Results.Created($"/api/v1/organisations/{result.Id}", result);
            })
            .WithName("CreateOrganisation")
            .Produces<OrganisationProfileDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.OrganisationWrite);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetOrganisationProfileQuery(id), ct)))
            .WithName("GetOrganisationProfile")
            .Produces<OrganisationProfileDto>()
            .RequirePermission(PermissionKeys.OrganisationRead);

        group.MapGet("/{id:guid}/dashboard", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetOrganisationDashboardQuery(id), ct)))
            .WithName("GetOrganisationDashboard")
            .Produces<OrganisationDashboardDto>()
            .RequirePermission(PermissionKeys.OrganisationRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateOrganisationProfileRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateOrganisationProfileCommand(
                    id, request.Name, request.LegalName, request.Industry, request.Size, request.Country, request.Website,
                    request.PrimaryContactName, request.PrimaryContactEmail, request.PrimaryContactPhone,
                    request.PrivacyContactName, request.PrivacyContactEmail, request.PrivacyContactPhone,
                    request.DpoName, request.DpoEmail, request.DpoPhone), ct)))
            .WithName("UpdateOrganisationProfile")
            .Produces<OrganisationProfileDto>()
            .RequirePermission(PermissionKeys.OrganisationWrite);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteOrganisationCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteOrganisation")
            .RequirePermission(PermissionKeys.OrganisationWrite);

        group.MapPost("/{id:guid}/locations", async (Guid id, LocationRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateOrganisationLocationCommand(
                    id, request.Label, request.AddressLine1, request.AddressLine2, request.City, request.State,
                    request.PostalCode, request.Country, request.IsPrimary), ct);
                return Results.Created($"/api/v1/organisations/{id}/locations/{result.Id}", result);
            })
            .WithName("CreateOrganisationLocation")
            .Produces<OrganisationLocationDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.OrganisationWrite);

        group.MapPut("/locations/{locationId:guid}", async (Guid locationId, LocationRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateOrganisationLocationCommand(
                    locationId, request.Label, request.AddressLine1, request.AddressLine2, request.City, request.State,
                    request.PostalCode, request.Country, request.IsPrimary), ct)))
            .WithName("UpdateOrganisationLocation")
            .Produces<OrganisationLocationDto>()
            .RequirePermission(PermissionKeys.OrganisationWrite);

        group.MapDelete("/locations/{locationId:guid}", async (Guid locationId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteOrganisationLocationCommand(locationId), ct);
                return Results.NoContent();
            })
            .WithName("DeleteOrganisationLocation")
            .RequirePermission(PermissionKeys.OrganisationWrite);

        return app;
    }
}
