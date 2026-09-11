using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Assessments.Commands.ApproveAssessment;
using DPDP.Application.Modules.Assessments.Commands.ArchiveAssessment;
using DPDP.Application.Modules.Assessments.Commands.AssignAssessment;
using DPDP.Application.Modules.Assessments.Commands.CreateAssessment;
using DPDP.Application.Modules.Assessments.Commands.RejectAssessment;
using DPDP.Application.Modules.Assessments.Commands.ReopenAssessment;
using DPDP.Application.Modules.Assessments.Commands.ReviewAssessment;
using DPDP.Application.Modules.Assessments.Commands.ReviewAssessmentAnswer;
using DPDP.Application.Modules.Assessments.Commands.SaveAssessmentAnswer;
using DPDP.Application.Modules.Assessments.Commands.SubmitAssessment;
using DPDP.Application.Modules.Assessments.Commands.UpdateAssessment;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Application.Modules.Assessments.Queries.GetAssessmentById;
using DPDP.Application.Modules.Assessments.Queries.GetAssessmentQuestionnaire;
using DPDP.Application.Modules.Assessments.Queries.GetAssessmentScore;
using DPDP.Application.Modules.Assessments.Queries.GetAssessments;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Assessments;

public sealed record CreateAssessmentRequest(
    Guid FrameworkVersionId, string Name, string? Description, Guid? AssignedToUserId, DateOnly? DueDate,
    IReadOnlyList<AssessmentScopeInput>? Scopes);

public sealed record UpdateAssessmentRequest(string Name, string? Description, DateOnly? DueDate);
public sealed record AssignAssessmentRequest(Guid? AssignedToUserId);
public sealed record ReviewAssessmentRequest(string Decision, string? Comments);
public sealed record ApproveAssessmentRequest(string? Comments);
public sealed record RejectAssessmentRequest(string Comments);

public sealed record SaveAssessmentAnswerRequest(
    string Status, string? AnswerValue, IReadOnlyList<string>? AnswerValues, string? Comment,
    IReadOnlyList<EvidenceReferenceDto>? Evidence, string? Confidence, string? AssessedRiskLevel, string? RemediationNotes);

public sealed record ReviewAssessmentAnswerRequest(string? ReviewComment, bool FlagForReview);

public static class AssessmentsEndpoints
{
    public static IEndpointRouteBuilder MapAssessmentsEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/assessments")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Assessments")
            .RequireAuthorization();

        group.MapGet("/", async (
                int? page, int? pageSize, string? search, string? status, Guid? frameworkVersionId, Guid? assignedToUserId,
                string? sortBy, bool? sortDescending, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetAssessmentsQuery(
                    page ?? 1, pageSize ?? 25, search, status, frameworkVersionId, assignedToUserId,
                    sortBy ?? "createdAt", sortDescending ?? true), ct)))
            .WithName("GetAssessments")
            .Produces<PagedResult<AssessmentSummaryDto>>()
            .RequirePermission(PermissionKeys.AssessmentsRead);

        group.MapPost("/", async (CreateAssessmentRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateAssessmentCommand(
                    request.FrameworkVersionId, request.Name, request.Description, request.AssignedToUserId,
                    request.DueDate, request.Scopes), ct);
                return Results.Created($"/api/v1/assessments/{result.Id}", result);
            })
            .WithName("CreateAssessment")
            .Produces<AssessmentDetailDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.AssessmentsCreate);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetAssessmentByIdQuery(id), ct)))
            .WithName("GetAssessmentById")
            .Produces<AssessmentDetailDto>()
            .RequirePermission(PermissionKeys.AssessmentsRead);

        group.MapPut("/{id:guid}", async (Guid id, UpdateAssessmentRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateAssessmentCommand(id, request.Name, request.Description, request.DueDate), ct)))
            .WithName("UpdateAssessment")
            .Produces<AssessmentDetailDto>()
            .RequirePermission(PermissionKeys.AssessmentsCreate);

        group.MapPost("/{id:guid}/assign", async (Guid id, AssignAssessmentRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new AssignAssessmentCommand(id, request.AssignedToUserId), ct)))
            .WithName("AssignAssessment")
            .Produces<AssessmentDetailDto>()
            .RequirePermission(PermissionKeys.AssessmentsCreate);

        group.MapGet("/{id:guid}/questionnaire", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetAssessmentQuestionnaireQuery(id), ct)))
            .WithName("GetAssessmentQuestionnaire")
            .Produces<IReadOnlyList<AssessmentControlQuestionnaireDto>>()
            .RequirePermission(PermissionKeys.AssessmentsRead);

        group.MapGet("/{id:guid}/score", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetAssessmentScoreQuery(id), ct)))
            .WithName("GetAssessmentScore")
            .Produces<AssessmentScoreDto>()
            .RequirePermission(PermissionKeys.AssessmentsRead);

        group.MapPost("/{id:guid}/submit", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new SubmitAssessmentCommand(id), ct)))
            .WithName("SubmitAssessment")
            .Produces<AssessmentDetailDto>()
            .RequirePermission(PermissionKeys.AssessmentsCreate);

        group.MapPost("/{id:guid}/review", async (Guid id, ReviewAssessmentRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ReviewAssessmentCommand(id, request.Decision, request.Comments), ct)))
            .WithName("ReviewAssessment")
            .Produces<AssessmentDetailDto>()
            .RequirePermission(PermissionKeys.AssessmentsReview);

        group.MapPost("/{id:guid}/approve", async (Guid id, ApproveAssessmentRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ApproveAssessmentCommand(id, request.Comments), ct)))
            .WithName("ApproveAssessment")
            .Produces<AssessmentDetailDto>()
            .RequirePermission(PermissionKeys.AssessmentsApprove);

        group.MapPost("/{id:guid}/reject", async (Guid id, RejectAssessmentRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new RejectAssessmentCommand(id, request.Comments), ct)))
            .WithName("RejectAssessment")
            .Produces<AssessmentDetailDto>()
            .RequirePermission(PermissionKeys.AssessmentsApprove);

        group.MapPost("/{id:guid}/reopen", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ReopenAssessmentCommand(id), ct)))
            .WithName("ReopenAssessment")
            .Produces<AssessmentDetailDto>()
            .RequirePermission(PermissionKeys.AssessmentsCreate);

        group.MapPost("/{id:guid}/archive", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ArchiveAssessmentCommand(id), ct)))
            .WithName("ArchiveAssessment")
            .Produces<AssessmentDetailDto>()
            .RequirePermission(PermissionKeys.AssessmentsApprove);

        group.MapPut("/answers/{assessmentControlQuestionId:guid}", async (Guid assessmentControlQuestionId, SaveAssessmentAnswerRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new SaveAssessmentAnswerCommand(
                    assessmentControlQuestionId, request.Status, request.AnswerValue, request.AnswerValues, request.Comment,
                    request.Evidence, request.Confidence, request.AssessedRiskLevel, request.RemediationNotes), ct)))
            .WithName("SaveAssessmentAnswer")
            .Produces<AssessmentAnswerDto>()
            .RequirePermission(PermissionKeys.AssessmentsCreate);

        group.MapPost("/answers/{assessmentControlQuestionId:guid}/review", async (Guid assessmentControlQuestionId, ReviewAssessmentAnswerRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ReviewAssessmentAnswerCommand(assessmentControlQuestionId, request.ReviewComment, request.FlagForReview), ct)))
            .WithName("ReviewAssessmentAnswer")
            .Produces<AssessmentAnswerDto>()
            .RequirePermission(PermissionKeys.AssessmentsReview);

        return app;
    }
}
