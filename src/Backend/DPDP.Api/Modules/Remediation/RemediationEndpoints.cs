using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Remediation.Commands;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Application.Modules.Remediation.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Remediation;

public sealed record CreateRemediationTaskRequest(Guid FindingId, string Title, string? Description, Guid? OwnerUserId, DateOnly? DueDate);
public sealed record UpdateRemediationTaskRequest(string Title, string? Description, DateOnly? DueDate);
public sealed record AssignRemediationTaskRequest(Guid OwnerUserId);
public sealed record UpdateRemediationTaskStatusRequest(string Status);
public sealed record AddRemediationEvidenceRequest(IReadOnlyList<EvidenceReferenceDto> Evidence);
public sealed record AddRemediationCommentRequest(string Comment);
public sealed record VerifyRemediationTaskRequest(string? VerificationNotes);

public static class RemediationEndpoints
{
    public static IEndpointRouteBuilder MapRemediationEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/remediation-tasks")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Remediation")
            .RequireAuthorization();

        group.MapGet("/", async (
                int? page, int? pageSize, string? search, string? status, Guid? ownerUserId, Guid? findingId,
                bool? overdueOnly, string? sortBy, bool? sortDescending, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRemediationTasksQuery(
                    page ?? 1, pageSize ?? 25, search, status, ownerUserId, findingId, overdueOnly,
                    sortBy ?? "dueDate", sortDescending ?? false), ct)))
            .WithName("GetRemediationTasks")
            .Produces<PagedResult<RemediationTaskSummaryDto>>()
            .RequirePermission(PermissionKeys.RemediationRead);

        group.MapPost("/", async (CreateRemediationTaskRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateRemediationTaskCommand(
                    request.FindingId, request.Title, request.Description, request.OwnerUserId, request.DueDate), ct);
                return Results.Created($"/api/v1/remediation-tasks/{result.Id}", result);
            })
            .WithName("CreateRemediationTask")
            .Produces<RemediationTaskDetailDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.RemediationManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRemediationTaskByIdQuery(id), ct)))
            .WithName("GetRemediationTaskById")
            .Produces<RemediationTaskDetailDto>()
            .RequirePermission(PermissionKeys.RemediationRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateRemediationTaskRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateRemediationTaskCommand(id, request.Title, request.Description, request.DueDate), ct)))
            .WithName("UpdateRemediationTask")
            .Produces<RemediationTaskDetailDto>()
            .RequirePermission(PermissionKeys.RemediationManage);

        group.MapPost("/{id:guid}/assign", async (Guid id, AssignRemediationTaskRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new AssignRemediationTaskCommand(id, request.OwnerUserId), ct)))
            .WithName("AssignRemediationTask")
            .Produces<RemediationTaskDetailDto>()
            .RequirePermission(PermissionKeys.RemediationManage);

        group.MapPost("/{id:guid}/status", async (Guid id, UpdateRemediationTaskStatusRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateRemediationTaskStatusCommand(id, request.Status), ct)))
            .WithName("UpdateRemediationTaskStatus")
            .Produces<RemediationTaskDetailDto>()
            .RequirePermission(PermissionKeys.RemediationManage);

        group.MapPost("/{id:guid}/evidence", async (Guid id, AddRemediationEvidenceRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new AddRemediationEvidenceCommand(id, request.Evidence), ct)))
            .WithName("AddRemediationEvidence")
            .Produces<RemediationTaskDetailDto>()
            .RequirePermission(PermissionKeys.RemediationManage);

        group.MapPost("/{id:guid}/comments", async (Guid id, AddRemediationCommentRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new AddRemediationCommentCommand(id, request.Comment), ct);
                return Results.Created($"/api/v1/remediation-tasks/{id}", result);
            })
            .WithName("AddRemediationComment")
            .Produces<RemediationCommentDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.RemediationManage);

        group.MapPost("/{id:guid}/verify", async (Guid id, VerifyRemediationTaskRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new VerifyRemediationTaskCommand(id, request.VerificationNotes), ct)))
            .WithName("VerifyRemediationTask")
            .Produces<RemediationTaskDetailDto>()
            .RequirePermission(PermissionKeys.RemediationManage);

        group.MapPost("/{id:guid}/close", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new CloseRemediationTaskCommand(id), ct)))
            .WithName("CloseRemediationTask")
            .Produces<RemediationTaskDetailDto>()
            .RequirePermission(PermissionKeys.RemediationManage);

        return app;
    }
}
