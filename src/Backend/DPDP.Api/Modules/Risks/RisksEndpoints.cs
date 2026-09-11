using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Risks.Commands;
using DPDP.Application.Modules.Risks.DTOs;
using DPDP.Application.Modules.Risks.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Risks;

public sealed record CreateRiskRequest(
    string Title, string Description, string Likelihood, string Impact, string DataSensitivity, string Exposure,
    Guid? OwnerUserId, string? TreatmentPlan);

public sealed record UpdateRiskRequest(
    string Title, string Description, string Likelihood, string Impact, string DataSensitivity, string Exposure,
    Guid? OwnerUserId, string Status, string? TreatmentPlan);

public static class RisksEndpoints
{
    public static IEndpointRouteBuilder MapRisksEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/risks")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Risks")
            .RequireAuthorization();

        group.MapGet("/", async (
                int? page, int? pageSize, string? search, string? riskLevel, string? status, Guid? ownerUserId,
                string? sortBy, bool? sortDescending, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRisksQuery(
                    page ?? 1, pageSize ?? 25, search, riskLevel, status, ownerUserId, sortBy ?? "score", sortDescending ?? true), ct)))
            .WithName("GetRisks")
            .Produces<PagedResult<RiskSummaryDto>>()
            .RequirePermission(PermissionKeys.RisksRead);

        group.MapPost("/", async (CreateRiskRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateRiskCommand(
                    request.Title, request.Description, request.Likelihood, request.Impact, request.DataSensitivity,
                    request.Exposure, request.OwnerUserId, request.TreatmentPlan), ct);
                return Results.Created($"/api/v1/risks/{result.Id}", result);
            })
            .WithName("CreateRisk")
            .Produces<RiskDetailDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.RisksManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRiskByIdQuery(id), ct)))
            .WithName("GetRiskById")
            .Produces<RiskDetailDto>()
            .RequirePermission(PermissionKeys.RisksRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateRiskRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateRiskCommand(
                    id, request.Title, request.Description, request.Likelihood, request.Impact, request.DataSensitivity,
                    request.Exposure, request.OwnerUserId, request.Status, request.TreatmentPlan), ct)))
            .WithName("UpdateRisk")
            .Produces<RiskDetailDto>()
            .RequirePermission(PermissionKeys.RisksManage);

        return app;
    }
}
