using Asp.Versioning.Builder;
using DPDP.Application.Modules.System.Queries.GetSystemInfo;
using MediatR;

namespace DPDP.Api.Modules.System;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(
        this IEndpointRouteBuilder app,
        ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/system")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("System");

        group.MapGet("/info", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetSystemInfoQuery(), cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetSystemInfo")
            .WithSummary("Application version and environment. Never returns secrets.")
            .Produces<SystemInfoDto>();

        return app;
    }
}
