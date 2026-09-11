using System.Text;
using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Application.Modules.DataInventory.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataInventory;

public sealed record CreateProcessingActivityRequest(
    string Name, string Purpose, IReadOnlyList<string> DataSubjectCategories, IReadOnlyList<string> SecurityControls,
    Guid? RetentionPolicyId, Guid? OwnerUserId, DateOnly? ReviewDate,
    IReadOnlyList<Guid> DataCategoryIds, IReadOnlyList<Guid> ItSystemIds, IReadOnlyList<Guid> DataCollectionSourceIds,
    IReadOnlyList<Guid> RecipientIds, IReadOnlyList<Guid> ProcessorIds);

public sealed record UpdateProcessingActivityRequest(
    string Name, string Purpose, IReadOnlyList<string> DataSubjectCategories, IReadOnlyList<string> SecurityControls,
    Guid? RetentionPolicyId, Guid? OwnerUserId, DateOnly? ReviewDate,
    IReadOnlyList<Guid> DataCategoryIds, IReadOnlyList<Guid> ItSystemIds, IReadOnlyList<Guid> DataCollectionSourceIds,
    IReadOnlyList<Guid> RecipientIds, IReadOnlyList<Guid> ProcessorIds);

public sealed record ApproveProcessingActivityRequest(string? ReviewComments);
public sealed record SendProcessingActivityBackToDraftRequest(string ReviewComments);

public static class ProcessingActivitiesEndpoints
{
    public static IEndpointRouteBuilder MapProcessingActivitiesEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/processing-activities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (
                int? page, int? pageSize, string? search, string? status, Guid? ownerUserId, Guid? dataCategoryId, Guid? itSystemId,
                ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetProcessingActivitiesQuery(page ?? 1, pageSize ?? 25, search, status, ownerUserId, dataCategoryId, itSystemId), ct)))
            .WithName("GetProcessingActivities").Produces<PagedResult<ProcessingActivitySummaryDto>>().RequirePermission(PermissionKeys.ProcessingActivitiesRead);

        group.MapGet("/export", async (string? search, string? status, Guid? ownerUserId, ISender sender, CancellationToken ct) =>
            {
                var csv = await sender.Send(new ExportProcessingActivitiesQuery(search, status, ownerUserId), ct);
                return Results.File(Encoding.UTF8.GetBytes(csv), "text/csv", $"processing-activities-{DateTime.UtcNow:yyyyMMdd}.csv");
            })
            .WithName("ExportProcessingActivities").RequirePermission(PermissionKeys.ProcessingActivitiesRead);

        group.MapPost("/", async (CreateProcessingActivityRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateProcessingActivityCommand(
                    request.Name, request.Purpose, request.DataSubjectCategories, request.SecurityControls,
                    request.RetentionPolicyId, request.OwnerUserId, request.ReviewDate,
                    request.DataCategoryIds, request.ItSystemIds, request.DataCollectionSourceIds, request.RecipientIds, request.ProcessorIds), ct);
                return Results.Created($"/api/v1/processing-activities/{result.Id}", result);
            })
            .WithName("CreateProcessingActivity").Produces<ProcessingActivityDetailDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.ProcessingActivitiesManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetProcessingActivityByIdQuery(id), ct)))
            .WithName("GetProcessingActivityById").Produces<ProcessingActivityDetailDto>().RequirePermission(PermissionKeys.ProcessingActivitiesRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProcessingActivityRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateProcessingActivityCommand(
                    id, request.Name, request.Purpose, request.DataSubjectCategories, request.SecurityControls,
                    request.RetentionPolicyId, request.OwnerUserId, request.ReviewDate,
                    request.DataCategoryIds, request.ItSystemIds, request.DataCollectionSourceIds, request.RecipientIds, request.ProcessorIds), ct)))
            .WithName("UpdateProcessingActivity").Produces<ProcessingActivityDetailDto>().RequirePermission(PermissionKeys.ProcessingActivitiesManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteProcessingActivityCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteProcessingActivity").RequirePermission(PermissionKeys.ProcessingActivitiesManage);

        group.MapPost("/{id:guid}/submit", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new SubmitProcessingActivityForReviewCommand(id), ct)))
            .WithName("SubmitProcessingActivityForReview").Produces<ProcessingActivityDetailDto>().RequirePermission(PermissionKeys.ProcessingActivitiesManage);

        group.MapPost("/{id:guid}/approve", async (Guid id, ApproveProcessingActivityRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ApproveProcessingActivityCommand(id, request.ReviewComments), ct)))
            .WithName("ApproveProcessingActivity").Produces<ProcessingActivityDetailDto>().RequirePermission(PermissionKeys.ProcessingActivitiesApprove);

        group.MapPost("/{id:guid}/send-back", async (Guid id, SendProcessingActivityBackToDraftRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new SendProcessingActivityBackToDraftCommand(id, request.ReviewComments), ct)))
            .WithName("SendProcessingActivityBackToDraft").Produces<ProcessingActivityDetailDto>().RequirePermission(PermissionKeys.ProcessingActivitiesReview);

        group.MapPost("/{id:guid}/archive", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ArchiveProcessingActivityCommand(id), ct)))
            .WithName("ArchiveProcessingActivity").Produces<ProcessingActivityDetailDto>().RequirePermission(PermissionKeys.ProcessingActivitiesManage);

        group.MapPost("/{id:guid}/reopen", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ReopenProcessingActivityCommand(id), ct)))
            .WithName("ReopenProcessingActivity").Produces<ProcessingActivityDetailDto>().RequirePermission(PermissionKeys.ProcessingActivitiesManage);

        return app;
    }
}
