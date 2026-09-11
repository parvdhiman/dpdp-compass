using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Application.Modules.ConsentPrivacy.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.ConsentPrivacy;

public sealed record CreateConsentRequest(
    Guid DataPrincipalId, Guid ConsentPurposeId, Guid? NoticeVersionId, DateTimeOffset? GrantedAt,
    string Channel, DateTimeOffset? ExpiresAt, string? SourceSystem, string? ExternalReferenceId);

public sealed record RevokeConsentRequest(string Reason);

public static class ConsentRecordsEndpoints
{
    public static IEndpointRouteBuilder MapConsentRecordsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/consent-records")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Consent & Privacy Operations")
            .RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, string? status, Guid? dataPrincipalId, Guid? consentPurposeId, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetConsentRecordsQuery(page ?? 1, pageSize ?? 25, status, dataPrincipalId, consentPurposeId), ct)))
            .WithName("GetConsentRecords").Produces<PagedResult<ConsentRecordDto>>().RequirePermission(PermissionKeys.ConsentRead);

        group.MapPost("/", async (CreateConsentRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateConsentCommand(
                    request.DataPrincipalId, request.ConsentPurposeId, request.NoticeVersionId, request.GrantedAt,
                    request.Channel, request.ExpiresAt, request.SourceSystem, request.ExternalReferenceId), ct);
                return Results.Created($"/api/v1/consent-records/{result.Id}", result);
            })
            .WithName("CreateConsent").Produces<ConsentRecordDto>(StatusCodes.Status201Created).RequirePermission(PermissionKeys.ConsentManage);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetConsentRecordByIdQuery(id), ct)))
            .WithName("GetConsentRecordById").Produces<ConsentRecordDto>().RequirePermission(PermissionKeys.ConsentRead);

        group.MapPost("/{id:guid}/withdraw", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new WithdrawConsentCommand(id), ct)))
            .WithName("WithdrawConsent").Produces<ConsentRecordDto>().RequirePermission(PermissionKeys.ConsentManage);

        group.MapPost("/{id:guid}/revoke", async (Guid id, RevokeConsentRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new RevokeConsentCommand(id, request.Reason), ct)))
            .WithName("RevokeConsent").Produces<ConsentRecordDto>().RequirePermission(PermissionKeys.ConsentManage);

        group.MapPost("/mark-expired", async (ISender sender, CancellationToken ct) =>
                Results.Ok(new { ExpiredCount = await sender.Send(new MarkConsentExpiredCommand(), ct) }))
            .WithName("MarkConsentExpired").RequirePermission(PermissionKeys.ConsentManage);

        return app;
    }
}
