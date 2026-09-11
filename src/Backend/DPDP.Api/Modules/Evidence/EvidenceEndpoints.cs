using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Evidence;
using DPDP.Application.Modules.Evidence.Commands;
using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Application.Modules.Evidence.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Evidence;

public sealed record UpdateEvidenceRequest(
    string Title, string? Description, string? VendorReference, string? ProcessingActivityReference,
    Guid? OwnerUserId, Guid? ReviewerUserId, DateOnly? ExpiryDate);

public sealed record RejectEvidenceRequest(string RejectionReason);
public sealed record ApproveEvidenceRequest(string? Comments);

public static class EvidenceEndpoints
{
    public static IEndpointRouteBuilder MapEvidenceEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/evidence")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Evidence")
            .RequireAuthorization();

        group.MapGet("/", async (
                int? page, int? pageSize, string? search, string? status, string? evidenceType,
                Guid? ownerUserId, Guid? reviewerUserId, Guid? assessmentId, Guid? controlId, Guid? findingId,
                bool? expiredOnly, string? sortBy, bool? sortDescending, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetEvidenceListQuery(
                    page ?? 1, pageSize ?? 25, search, status, evidenceType, ownerUserId, reviewerUserId,
                    assessmentId, controlId, findingId, expiredOnly, sortBy ?? "createdAt", sortDescending ?? true), ct)))
            .WithName("GetEvidenceList")
            .Produces<PagedResult<EvidenceSummaryDto>>()
            .RequirePermission(PermissionKeys.EvidenceRead);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetEvidenceByIdQuery(id), ct)))
            .WithName("GetEvidenceById")
            .Produces<EvidenceDetailDto>()
            .RequirePermission(PermissionKeys.EvidenceRead);

        group.MapPost("/", async (HttpRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = await BindUploadCommandAsync(request, ct);
                var result = await sender.Send(command, ct);
                return Results.Created($"/api/v1/evidence/{result.Id}", result);
            })
            .WithName("UploadEvidence")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<EvidenceDetailDto>(StatusCodes.Status201Created)
            .DisableAntiforgery()
            .RequirePermission(PermissionKeys.EvidenceUpload);

        group.MapPost("/{id:guid}/versions", async (Guid id, HttpRequest request, ISender sender, CancellationToken ct) =>
            {
                var form = await request.ReadFormAsync(ct);
                var externalUrl = form["externalUrl"].ToString() is { Length: > 0 } url ? url : null;
                var file = ToFileUpload(form.Files["file"]);
                var result = await sender.Send(new UploadEvidenceVersionCommand(id, externalUrl, file), ct);
                return Results.Ok(result);
            })
            .WithName("UploadEvidenceVersion")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<EvidenceDetailDto>()
            .DisableAntiforgery()
            .RequirePermission(PermissionKeys.EvidenceUpload);

        group.MapPut("/{id:guid}", async (Guid id, UpdateEvidenceRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateEvidenceCommand(
                    id, request.Title, request.Description, request.VendorReference, request.ProcessingActivityReference,
                    request.OwnerUserId, request.ReviewerUserId, request.ExpiryDate), ct)))
            .WithName("UpdateEvidence")
            .Produces<EvidenceDetailDto>()
            .RequirePermission(PermissionKeys.EvidenceUpload);

        group.MapPost("/{id:guid}/submit", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new SubmitEvidenceForReviewCommand(id), ct)))
            .WithName("SubmitEvidenceForReview")
            .Produces<EvidenceDetailDto>()
            .RequirePermission(PermissionKeys.EvidenceUpload);

        group.MapPost("/{id:guid}/approve", async (Guid id, ApproveEvidenceRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ApproveEvidenceCommand(id, request.Comments), ct)))
            .WithName("ApproveEvidence")
            .Produces<EvidenceDetailDto>()
            .RequirePermission(PermissionKeys.EvidenceReview);

        group.MapPost("/{id:guid}/reject", async (Guid id, RejectEvidenceRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new RejectEvidenceCommand(id, request.RejectionReason), ct)))
            .WithName("RejectEvidence")
            .Produces<EvidenceDetailDto>()
            .RequirePermission(PermissionKeys.EvidenceReview);

        group.MapPost("/{id:guid}/archive", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ArchiveEvidenceCommand(id), ct)))
            .WithName("ArchiveEvidence")
            .Produces<EvidenceDetailDto>()
            .RequirePermission(PermissionKeys.EvidenceReview);

        group.MapPost("/mark-expired", async (ISender sender, CancellationToken ct) =>
                Results.Ok(new { ExpiredCount = await sender.Send(new MarkEvidenceExpiredCommand(), ct) }))
            .WithName("MarkEvidenceExpired")
            .RequirePermission(PermissionKeys.EvidenceReview);

        group.MapGet("/{id:guid}/versions/{versionNumber:int}/download", async (Guid id, int versionNumber, ISender sender, CancellationToken ct) =>
            {
                var file = await sender.Send(new DownloadEvidenceVersionQuery(id, versionNumber), ct);
                return Results.File(file.Content, file.ContentType, file.FileName);
            })
            .WithName("DownloadEvidenceVersion")
            .RequirePermission(PermissionKeys.EvidenceRead);

        group.MapGet("/{id:guid}/download", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var file = await sender.Send(new DownloadEvidenceVersionQuery(id), ct);
                return Results.File(file.Content, file.ContentType, file.FileName);
            })
            .WithName("DownloadLatestEvidenceVersion")
            .RequirePermission(PermissionKeys.EvidenceRead);

        group.MapGet("/{id:guid}/versions/{versionNumber:int}/preview", async (Guid id, int versionNumber, ISender sender, CancellationToken ct) =>
            {
                var file = await sender.Send(new PreviewEvidenceVersionQuery(id, versionNumber), ct);
                return Results.File(file.Content, file.ContentType);
            })
            .WithName("PreviewEvidenceVersion")
            .RequirePermission(PermissionKeys.EvidenceRead);

        group.MapGet("/{id:guid}/preview", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var file = await sender.Send(new PreviewEvidenceVersionQuery(id), ct);
                return Results.File(file.Content, file.ContentType);
            })
            .WithName("PreviewLatestEvidenceVersion")
            .RequirePermission(PermissionKeys.EvidenceRead);

        return app;
    }

    private static async Task<UploadEvidenceCommand> BindUploadCommandAsync(HttpRequest request, CancellationToken ct)
    {
        var form = await request.ReadFormAsync(ct);

        Guid? ParseGuid(string key) => Guid.TryParse(form[key].ToString(), out var g) ? g : null;
        DateOnly? ParseDate(string key) => DateOnly.TryParse(form[key].ToString(), out var d) ? d : null;
        string? ParseString(string key) => form[key].ToString() is { Length: > 0 } value ? value : null;

        return new UploadEvidenceCommand(
            Title: form["title"].ToString(),
            Description: ParseString("description"),
            EvidenceType: form["evidenceType"].ToString(),
            AssessmentId: ParseGuid("assessmentId"),
            ControlId: ParseGuid("controlId"),
            FindingId: ParseGuid("findingId"),
            VendorReference: ParseString("vendorReference"),
            ProcessingActivityReference: ParseString("processingActivityReference"),
            OwnerUserId: ParseGuid("ownerUserId"),
            ReviewerUserId: ParseGuid("reviewerUserId"),
            ExpiryDate: ParseDate("expiryDate"),
            ExternalUrl: ParseString("externalUrl"),
            File: ToFileUpload(form.Files["file"]));
    }

    private static EvidenceFileUpload? ToFileUpload(IFormFile? file) =>
        file is null ? null : new EvidenceFileUpload(file.FileName, file.ContentType, file.Length, file.OpenReadStream());
}
