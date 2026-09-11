using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Application.Modules.ConsentPrivacy.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.ConsentPrivacy;

public sealed record CreatePrivacyNoticeRequest(
    string Code, string Title, string Version, string Language, string Purpose,
    DateOnly? PublishedDate, DateOnly? EffectiveDate, IReadOnlyList<Guid> DataCategoryIds);

public sealed record UpdatePrivacyNoticeRequest(
    string Title, string Language, string Purpose,
    DateOnly? PublishedDate, DateOnly? EffectiveDate, IReadOnlyList<Guid> DataCategoryIds);

public static class PrivacyNoticesEndpoints
{
    public static IEndpointRouteBuilder MapPrivacyNoticesEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/privacy-notices")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Consent & Privacy Operations")
            .RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, string? search, string? status, string? code, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetPrivacyNoticesQuery(page ?? 1, pageSize ?? 25, search, status, code), ct)))
            .WithName("GetPrivacyNotices").Produces<PagedResult<PrivacyNoticeSummaryDto>>().RequirePermission(PermissionKeys.PrivacyNoticesRead);

        group.MapPost("/", async (CreatePrivacyNoticeRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreatePrivacyNoticeCommand(
                    request.Code, request.Title, request.Version, request.Language, request.Purpose,
                    request.PublishedDate, request.EffectiveDate, request.DataCategoryIds), ct);
                return Results.Created($"/api/v1/privacy-notices/{result.Id}", result);
            })
            .WithName("CreatePrivacyNotice").Produces<PrivacyNoticeDetailDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.PrivacyNoticesManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetPrivacyNoticeByIdQuery(id), ct)))
            .WithName("GetPrivacyNoticeById").Produces<PrivacyNoticeDetailDto>().RequirePermission(PermissionKeys.PrivacyNoticesRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdatePrivacyNoticeRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdatePrivacyNoticeCommand(
                    id, request.Title, request.Language, request.Purpose,
                    request.PublishedDate, request.EffectiveDate, request.DataCategoryIds), ct)))
            .WithName("UpdatePrivacyNotice").Produces<PrivacyNoticeDetailDto>().RequirePermission(PermissionKeys.PrivacyNoticesManage);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeletePrivacyNoticeCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeletePrivacyNotice").RequirePermission(PermissionKeys.PrivacyNoticesManage);

        group.MapPost("/{id:guid}/approve", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ApprovePrivacyNoticeCommand(id), ct)))
            .WithName("ApprovePrivacyNotice").Produces<PrivacyNoticeDetailDto>().RequirePermission(PermissionKeys.PrivacyNoticesApprove);

        group.MapPost("/{id:guid}/publish", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new PublishPrivacyNoticeCommand(id), ct)))
            .WithName("PublishPrivacyNotice").Produces<PrivacyNoticeDetailDto>().RequirePermission(PermissionKeys.PrivacyNoticesManage);

        group.MapPost("/{id:guid}/archive", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ArchivePrivacyNoticeCommand(id), ct)))
            .WithName("ArchivePrivacyNotice").Produces<PrivacyNoticeDetailDto>().RequirePermission(PermissionKeys.PrivacyNoticesManage);

        return app;
    }
}
