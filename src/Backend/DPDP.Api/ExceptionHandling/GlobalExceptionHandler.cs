using DPDP.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace DPDP.Api.ExceptionHandling;

/// <summary>
/// Single point mapping every unhandled exception to an RFC 7807
/// ProblemDetails response. Never surfaces exception messages or stack
/// traces to the client outside Development — see docs/SECURITY.md
/// section 9. The full exception is always logged server-side with the
/// request's correlation id so it stays diagnosable.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Response.Headers["X-Correlation-Id"].ToString();

        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "Authentication failed"),
            AccountLockedException => (StatusCodes.Status423Locked, "Account locked"),
            ForbiddenException or AccountDisabledException or UnauthorizedAccessException
                => (StatusCodes.Status403Forbidden, "Forbidden"),
            NotFoundException or KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception. CorrelationId={CorrelationId}",
                correlationId);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Handled exception mapped to {StatusCode}. CorrelationId={CorrelationId}",
                statusCode,
                correlationId);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = statusCode,
                Title = title,
                // A 5xx message could be an internal detail (SQL, stack
                // info) — hide it outside Development. A 4xx message here
                // is always one of our own intentional, user-facing
                // messages (e.g. "Invalid email or password"), safe to
                // return in every environment.
                Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? (environment.IsDevelopment() ? exception.Message : null)
                    : exception.Message,
                Type = $"https://dpdp-compass/errors/{title.Replace(' ', '-').ToLowerInvariant()}",
            },
        };

        // correlationId is added to every ProblemDetails response (this path and
        // UseStatusCodePages alike) by the CustomizeProblemDetails hook in Program.cs.

        if (exception is ValidationException validationException)
        {
            problemDetailsContext.ProblemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        return await problemDetailsService.TryWriteAsync(problemDetailsContext);
    }
}
