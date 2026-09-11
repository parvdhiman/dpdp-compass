using Serilog.Context;

namespace DPDP.Api.Middleware;

/// <summary>
/// Ensures every request/response carries X-Correlation-Id (caller-supplied
/// or server-generated), and pushes it into the Serilog log context for the
/// lifetime of the request — see docs/API.md section 1.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var provided)
            && !string.IsNullOrWhiteSpace(provided)
                ? provided.ToString()
                : Guid.NewGuid().ToString();

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
