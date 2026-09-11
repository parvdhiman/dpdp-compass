using DPDP.Application.Common.Interfaces;
using MediatR;

namespace DPDP.Application.Modules.System.Queries.GetSystemInfo;

public sealed class GetSystemInfoQueryHandler(
    IApplicationInfo applicationInfo,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetSystemInfoQuery, SystemInfoDto>
{
    public Task<SystemInfoDto> Handle(GetSystemInfoQuery request, CancellationToken cancellationToken)
    {
        var result = new SystemInfoDto(
            applicationInfo.ApplicationName,
            applicationInfo.Version,
            applicationInfo.EnvironmentName,
            dateTimeProvider.UtcNow);

        return Task.FromResult(result);
    }
}
