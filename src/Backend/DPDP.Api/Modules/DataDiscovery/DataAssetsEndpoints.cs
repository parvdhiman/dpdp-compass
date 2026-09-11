using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataDiscovery.Commands;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Application.Modules.DataDiscovery.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataDiscovery;

public sealed record UpdateDataElementClassificationRequest(string? Category);

public static class DataAssetsEndpoints
{
    public static IEndpointRouteBuilder MapDataAssetsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var assets = app.MapGroup("/api/v{version:apiVersion}/data-assets")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Discovery")
            .RequireAuthorization();

        assets.MapGet("/", async (int? page, int? pageSize, string? search, Guid? dataSourceId, string? assetType, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataAssetsQuery(page ?? 1, pageSize ?? 25, search, dataSourceId, assetType), ct)))
            .WithName("GetDataAssets")
            .Produces<PagedResult<DataAssetSummaryDto>>()
            .RequirePermission(PermissionKeys.DataAssetsRead);

        assets.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataAssetByIdQuery(id), ct)))
            .WithName("GetDataAssetById")
            .Produces<DataAssetDetailDto>()
            .RequirePermission(PermissionKeys.DataAssetsRead);

        var elements = app.MapGroup("/api/v{version:apiVersion}/data-elements")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Discovery")
            .RequireAuthorization();

        elements.MapGet("/", async (
                int? page, int? pageSize, Guid? dataAssetId, string? category, bool? unclassifiedOnly, bool? lowConfidenceOnly,
                ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDataElementsQuery(page ?? 1, pageSize ?? 25, dataAssetId, category, unclassifiedOnly, lowConfidenceOnly), ct)))
            .WithName("GetDataElements")
            .Produces<PagedResult<DataElementDto>>()
            .RequirePermission(PermissionKeys.DataAssetsRead);

        elements.MapGet("/classification-summary", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetClassificationSummaryQuery(), ct)))
            .WithName("GetClassificationSummary")
            .Produces<ClassificationSummaryDto>()
            .RequirePermission(PermissionKeys.DataAssetsRead);

        elements.MapPost("/{id:guid}/classification", async (Guid id, UpdateDataElementClassificationRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateDataElementClassificationCommand(id, request.Category), ct)))
            .WithName("UpdateDataElementClassification")
            .Produces<DataElementDto>()
            .RequirePermission(PermissionKeys.ClassificationReview);

        return app;
    }
}
