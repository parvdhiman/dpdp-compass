using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataDiscovery.Commands;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Application.Modules.DataDiscovery.Queries;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.DataDiscovery;

public static class DiscoveryJobsEndpoints
{
    public static IEndpointRouteBuilder MapDiscoveryJobsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/discovery-jobs")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Data Discovery")
            .RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, Guid? dataSourceId, string? status, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDiscoveryJobsQuery(page ?? 1, pageSize ?? 25, dataSourceId, status), ct)))
            .WithName("GetDiscoveryJobs")
            .Produces<PagedResult<DiscoveryJobDto>>()
            .RequirePermission(PermissionKeys.DiscoveryJobsRead);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetDiscoveryJobByIdQuery(id), ct)))
            .WithName("GetDiscoveryJobById")
            .Produces<DiscoveryJobDetailDto>()
            .RequirePermission(PermissionKeys.DiscoveryJobsRead);

        group.MapPost("/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new CancelDiscoveryJobCommand(id), ct)))
            .WithName("CancelDiscoveryJob")
            .Produces<DiscoveryJobDto>()
            .RequirePermission(PermissionKeys.DiscoveryJobsManage);

        return app;
    }
}
