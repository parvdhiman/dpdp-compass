using Asp.Versioning.Builder;
using DPDP.Application.Modules.Identity.Commands.ChangePassword;
using DPDP.Application.Modules.Identity.Commands.ForgotPassword;
using DPDP.Application.Modules.Identity.Commands.Login;
using DPDP.Application.Modules.Identity.Commands.Logout;
using DPDP.Application.Modules.Identity.Commands.RefreshToken;
using DPDP.Application.Modules.Identity.Commands.ResetPassword;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Application.Modules.Identity.Queries.GetCurrentUser;
using MediatR;

namespace DPDP.Api.Modules.Auth;

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword);

/// <summary>
/// ForgotPassword's response only ever carries a reset token outside
/// Production — there is no email delivery module yet (Notifications is
/// Phase 7). See docs/SECURITY.md and the Module 2 completion report.
/// </summary>
public sealed record ForgotPasswordResponse(string? DevOnlyResetToken);

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new LoginCommand(request.Email, request.Password), ct)))
            .WithName("Login")
            .Produces<AuthResultDto>()
            .RequireRateLimiting("auth");

        group.MapPost("/refresh", async (RefreshRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new RefreshTokenCommand(request.RefreshToken), ct)))
            .WithName("RefreshToken")
            .Produces<AuthResultDto>()
            .RequireRateLimiting("auth");

        group.MapPost("/logout", async (LogoutRequest request, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new LogoutCommand(request.RefreshToken), ct);
                return Results.NoContent();
            })
            .WithName("Logout")
            .RequireAuthorization();

        group.MapPost("/forgot-password", async (ForgotPasswordRequest request, ISender sender, IHostEnvironment env, CancellationToken ct) =>
            {
                var result = await sender.Send(new ForgotPasswordCommand(request.Email), ct);
                // Never expose the reset token outside Development — see docs/SECURITY.md.
                var devToken = env.IsDevelopment() ? result.ResetToken : null;
                return Results.Ok(new ForgotPasswordResponse(devToken));
            })
            .WithName("ForgotPassword")
            .Produces<ForgotPasswordResponse>()
            .RequireRateLimiting("auth");

        group.MapPost("/reset-password", async (ResetPasswordRequest request, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new ResetPasswordCommand(request.Token, request.NewPassword), ct);
                return Results.NoContent();
            })
            .WithName("ResetPassword")
            .RequireRateLimiting("auth");

        group.MapPost("/change-password", async (ChangePasswordRequest request, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword), ct);
                return Results.NoContent();
            })
            .WithName("ChangePassword")
            .RequireAuthorization()
            // HIGH finding, docs/ARCHITECTURE_REVIEW.md: every other endpoint
            // that verifies a password (login, reset) is rate-limited; this
            // one wasn't, letting anyone holding a valid access token guess
            // the current password an unlimited number of times.
            .RequireRateLimiting("auth");

        group.MapGet("/me", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetCurrentUserQuery(), ct)))
            .WithName("GetCurrentUser")
            .Produces<MeDto>()
            .RequireAuthorization();

        return app;
    }
}
