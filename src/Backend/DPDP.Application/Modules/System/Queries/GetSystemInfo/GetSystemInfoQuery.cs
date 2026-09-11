using MediatR;

namespace DPDP.Application.Modules.System.Queries.GetSystemInfo;

public sealed record GetSystemInfoQuery : IRequest<SystemInfoDto>;
