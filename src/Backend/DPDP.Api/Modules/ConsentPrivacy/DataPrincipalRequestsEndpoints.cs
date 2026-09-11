using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Application.Modules.ConsentPrivacy.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.ConsentPrivacy;

public sealed record CreateDataPrincipalRequestRequest(
    string RequestType, string RequesterName, string? RequesterContactEmail, string? RequesterContactPhone,
    string? ExternalReferenceId, Guid? DataPrincipalId, Guid? RelatedConsentId, string? Description);

public sealed record AssignDataPrincipalRequestRequest(Guid AssignedToUserId);
public sealed record VerifyDataPrincipalRequestIdentityRequest(Guid? MatchedDataPrincipalId);
public sealed record UpdateDataPrincipalRequestStatusRequest(string Status);
public sealed record CompleteDataPrincipalRequestRequest(string? ResolutionNotes);
public sealed record RejectDataPrincipalRequestRequest(string RejectionReason);

public static class DataPrincipalRequestsEndpoints
{
    public static IEndpointRouteBuilder MapDataPrincipalRequestsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/data-principal-requests")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Consent & Privacy Operations")
            .RequireAuthorization();

        group.MapGet("/", async (
                int? page, int? pageSize, string? status, string? requestType, Guid? assignedToUserId, bool? overdueOnly,
                ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataPrincipalRequestsQuery(page ?? 1, pageSize ?? 25, status, requestType, assignedToUserId, overdueOnly), ct)))
            .WithName("GetDataPrincipalRequests").Produces<PagedResult<DataPrincipalRequestSummaryDto>>().RequirePermission(PermissionKeys.DataRequestsRead);

        group.MapGet("/sla-summary", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataPrincipalRequestSlaSummaryQuery(), ct)))
            .WithName("GetDataPrincipalRequestSlaSummary").Produces<SlaSummaryDto>().RequirePermission(PermissionKeys.DataRequestsRead);

        group.MapPost("/", async (CreateDataPrincipalRequestRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateDataPrincipalRequestCommand(
                    request.RequestType, request.RequesterName, request.RequesterContactEmail, request.RequesterContactPhone,
                    request.ExternalReferenceId, request.DataPrincipalId, request.RelatedConsentId, request.Description), ct);
                return Results.Created($"/api/v1/data-principal-requests/{result.Id}", result);
            })
            .WithName("CreateDataPrincipalRequest").Produces<DataPrincipalRequestDetailDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.DataRequestsManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataPrincipalRequestByIdQuery(id), ct)))
            .WithName("GetDataPrincipalRequestById").Produces<DataPrincipalRequestDetailDto>().RequirePermission(PermissionKeys.DataRequestsRead);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteDataPrincipalRequestCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteDataPrincipalRequest").RequirePermission(PermissionKeys.DataRequestsManage);

        group.MapPost("/{id:guid}/assign", async (Guid id, AssignDataPrincipalRequestRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new AssignDataPrincipalRequestCommand(id, request.AssignedToUserId), ct)))
            .WithName("AssignDataPrincipalRequest").Produces<DataPrincipalRequestDetailDto>().RequirePermission(PermissionKeys.DataRequestsManage);

        group.MapPost("/{id:guid}/verify-identity", async (Guid id, VerifyDataPrincipalRequestIdentityRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new VerifyDataPrincipalRequestIdentityCommand(id, request.MatchedDataPrincipalId), ct)))
            .WithName("VerifyDataPrincipalRequestIdentity").Produces<DataPrincipalRequestDetailDto>().RequirePermission(PermissionKeys.DataRequestsManage);

        group.MapPost("/{id:guid}/status", async (Guid id, UpdateDataPrincipalRequestStatusRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateDataPrincipalRequestStatusCommand(id, request.Status), ct)))
            .WithName("UpdateDataPrincipalRequestStatus").Produces<DataPrincipalRequestDetailDto>().RequirePermission(PermissionKeys.DataRequestsManage);

        group.MapPost("/{id:guid}/complete", async (Guid id, CompleteDataPrincipalRequestRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new CompleteDataPrincipalRequestCommand(id, request.ResolutionNotes), ct)))
            .WithName("CompleteDataPrincipalRequest").Produces<DataPrincipalRequestDetailDto>().RequirePermission(PermissionKeys.DataRequestsManage);

        group.MapPost("/{id:guid}/reject", async (Guid id, RejectDataPrincipalRequestRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new RejectDataPrincipalRequestCommand(id, request.RejectionReason), ct)))
            .WithName("RejectDataPrincipalRequest").Produces<DataPrincipalRequestDetailDto>().RequirePermission(PermissionKeys.DataRequestsManage);

        group.MapPost("/{id:guid}/close", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new CloseDataPrincipalRequestCommand(id), ct)))
            .WithName("CloseDataPrincipalRequest").Produces<DataPrincipalRequestDetailDto>().RequirePermission(PermissionKeys.DataRequestsManage);

        return app;
    }
}
