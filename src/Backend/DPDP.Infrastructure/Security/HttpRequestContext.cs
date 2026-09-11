using DPDP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DPDP.Infrastructure.Security;

public sealed class HttpRequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
{
    private HttpContext? Context => httpContextAccessor.HttpContext;

    public string? IpAddress => Context?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => Context?.Request.Headers.UserAgent.ToString();

    public string? CorrelationId => Context?.Response.Headers["X-Correlation-Id"].ToString();
}
