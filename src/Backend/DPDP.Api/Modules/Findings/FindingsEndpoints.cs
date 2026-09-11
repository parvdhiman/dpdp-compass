using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Findings.Commands;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Application.Modules.Findings.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Findings;

public sealed record CreateFindingRequest(
    string Title, string Description, string Severity, Guid? ControlId, string? AssetReference,
    Guid? OwnerUserId, DateOnly? DueDate, string? Recommendation);

public sealed record CreateFindingFromControlRequest(Guid AssessmentControlId, string? Recommendation);

public sealed record UpdateFindingRequest(string Title, string Description, string Severity, string? AssetReference, DateOnly? DueDate, string? Recommendation);
public sealed record AssignFindingRequest(Guid OwnerUserId);
public sealed record UpdateFindingStatusRequest(string Status);
public sealed record AcceptFindingRiskRequest(string Comments);
public sealed record CreateRiskFromFindingRequest(string Likelihood, string Impact, string? TreatmentPlan);

public static class FindingsEndpoints
{
    public static IEndpointRouteBuilder MapFindingsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/findings")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Findings")
            .RequireAuthorization();

        group.MapGet("/", async (
                int? page, int? pageSize, string? search, string? status, string? severity, Guid? ownerUserId, Guid? riskId,
                bool? overdueOnly, string? sortBy, bool? sortDescending, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetFindingsQuery(
                    page ?? 1, pageSize ?? 25, search, status, severity, ownerUserId, riskId, overdueOnly,
                    sortBy ?? "createdAt", sortDescending ?? true), ct)))
            .WithName("GetFindings")
            .Produces<PagedResult<FindingSummaryDto>>()
            .RequirePermission(PermissionKeys.FindingsRead);

        group.MapPost("/", async (CreateFindingRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateFindingCommand(
                    request.Title, request.Description, request.Severity, request.ControlId, request.AssetReference,
                    request.OwnerUserId, request.DueDate, request.Recommendation), ct);
                return Results.Created($"/api/v1/findings/{result.Id}", result);
            })
            .WithName("CreateFinding")
            .Produces<FindingDetailDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.FindingsCreate);

        group.MapPost("/from-assessment-control", async (CreateFindingFromControlRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateFindingFromAssessmentControlCommand(request.AssessmentControlId, request.Recommendation), ct);
                return Results.Created($"/api/v1/findings/{result.Id}", result);
            })
            .WithName("CreateFindingFromAssessmentControl")
            .Produces<FindingDetailDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.FindingsCreate);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetFindingByIdQuery(id), ct)))
            .WithName("GetFindingById")
            .Produces<FindingDetailDto>()
            .RequirePermission(PermissionKeys.FindingsRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateFindingRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateFindingCommand(
                    id, request.Title, request.Description, request.Severity, request.AssetReference, request.DueDate, request.Recommendation), ct)))
            .WithName("UpdateFinding")
            .Produces<FindingDetailDto>()
            .RequirePermission(PermissionKeys.FindingsCreate);

        group.MapPost("/{id:guid}/assign", async (Guid id, AssignFindingRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new AssignFindingCommand(id, request.OwnerUserId), ct)))
            .WithName("AssignFinding")
            .Produces<FindingDetailDto>()
            .RequirePermission(PermissionKeys.FindingsAssign);

        group.MapPost("/{id:guid}/status", async (Guid id, UpdateFindingStatusRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateFindingStatusCommand(id, request.Status), ct)))
            .WithName("UpdateFindingStatus")
            .Produces<FindingDetailDto>()
            .RequirePermission(PermissionKeys.FindingsAssign);

        group.MapPost("/{id:guid}/close", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new CloseFindingCommand(id), ct)))
            .WithName("CloseFinding")
            .Produces<FindingDetailDto>()
            .RequirePermission(PermissionKeys.FindingsClose);

        group.MapPost("/{id:guid}/accept-risk", async (Guid id, AcceptFindingRiskRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new AcceptFindingRiskCommand(id, request.Comments), ct)))
            .WithName("AcceptFindingRisk")
            .Produces<FindingDetailDto>()
            .RequirePermission(PermissionKeys.FindingsClose);

        group.MapPost("/{id:guid}/risk", async (Guid id, CreateRiskFromFindingRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new CreateRiskFromFindingCommand(id, request.Likelihood, request.Impact, request.TreatmentPlan), ct)))
            .WithName("CreateRiskFromFinding")
            .Produces<FindingDetailDto>()
            .RequirePermission(PermissionKeys.RisksManage);

        return app;
    }
}
