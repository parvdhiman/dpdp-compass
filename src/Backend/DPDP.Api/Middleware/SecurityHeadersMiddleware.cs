namespace DPDP.Api.Middleware;

/// <summary>
/// Baseline security response headers applied to every response —
/// see docs/SECURITY.md section 4. Kept intentionally small for Module 1;
/// nothing here depends on a business module.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
            return Task.CompletedTask;
        });

        await next(context);
    }
}
