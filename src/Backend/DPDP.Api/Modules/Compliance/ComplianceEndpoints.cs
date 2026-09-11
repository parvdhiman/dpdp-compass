using Asp.Versioning.Builder;
using DPDP.Api.Security;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Compliance.Commands.ActivateControl;
using DPDP.Application.Modules.Compliance.Commands.ActivateFrameworkVersion;
using DPDP.Application.Modules.Compliance.Commands.CreateAssessmentQuestion;
using DPDP.Application.Modules.Compliance.Commands.CreateControl;
using DPDP.Application.Modules.Compliance.Commands.CreateControlCategory;
using DPDP.Application.Modules.Compliance.Commands.CreateControlMapping;
using DPDP.Application.Modules.Compliance.Commands.CreateEvidenceRequirement;
using DPDP.Application.Modules.Compliance.Commands.CreateFramework;
using DPDP.Application.Modules.Compliance.Commands.CreateFrameworkVersion;
using DPDP.Application.Modules.Compliance.Commands.CreateLegalReference;
using DPDP.Application.Modules.Compliance.Commands.CreateRequirement;
using DPDP.Application.Modules.Compliance.Commands.DeleteAssessmentQuestion;
using DPDP.Application.Modules.Compliance.Commands.DeleteControlMapping;
using DPDP.Application.Modules.Compliance.Commands.DeleteEvidenceRequirement;
using DPDP.Application.Modules.Compliance.Commands.RetireControl;
using DPDP.Application.Modules.Compliance.Commands.UpdateAssessmentQuestion;
using DPDP.Application.Modules.Compliance.Commands.UpdateControl;
using DPDP.Application.Modules.Compliance.Commands.UpdateControlCategory;
using DPDP.Application.Modules.Compliance.Commands.UpdateEvidenceRequirement;
using DPDP.Application.Modules.Compliance.Commands.UpdateFramework;
using DPDP.Application.Modules.Compliance.Commands.UpdateFrameworkVersion;
using DPDP.Application.Modules.Compliance.Commands.UpdateLegalReference;
using DPDP.Application.Modules.Compliance.Commands.UpdateRequirement;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Application.Modules.Compliance.Queries.GetAnswerStatuses;
using DPDP.Application.Modules.Compliance.Queries.GetControlById;
using DPDP.Application.Modules.Compliance.Queries.GetControlCategories;
using DPDP.Application.Modules.Compliance.Queries.GetControls;
using DPDP.Application.Modules.Compliance.Queries.GetEvidenceRequirements;
using DPDP.Application.Modules.Compliance.Queries.GetFrameworks;
using DPDP.Application.Modules.Compliance.Queries.GetFrameworkVersionDetail;
using DPDP.Application.Modules.Compliance.Queries.GetLegalReferences;
using DPDP.Application.Modules.Compliance.Queries.GetQuestions;
using DPDP.Application.Modules.Compliance.Queries.GetRequirements;
using DPDP.Domain.Modules.Identity;
using MediatR;

namespace DPDP.Api.Modules.Compliance;

public sealed record CreateFrameworkRequest(string Name, string Code, string Jurisdiction, string IssuingAuthority, string? Description);
public sealed record UpdateFrameworkRequest(string Name, string Jurisdiction, string IssuingAuthority, string? Description);

public sealed record CreateFrameworkVersionRequest(string VersionLabel, string? OfficialCitation, DateOnly? PublicationDate, DateOnly? EffectiveDate, string? SourceUrl, string? ChangeSummary);
public sealed record UpdateFrameworkVersionRequest(string? OfficialCitation, DateOnly? PublicationDate, DateOnly? EffectiveDate, string? SourceUrl, string? ChangeSummary, string ReviewStatus);

public sealed record CreateLegalReferenceRequest(Guid FrameworkVersionId, string Citation, string Title, string? Chapter, string? SummaryText, string SourceCitation);
public sealed record UpdateLegalReferenceRequest(string Title, string? Chapter, string? SummaryText, string SourceCitation, string ReviewStatus);

public sealed record CreateRequirementRequest(Guid LegalReferenceId, string Code, string Title, string Description);
public sealed record UpdateRequirementRequest(string Title, string Description, string ReviewStatus);

public sealed record CreateControlCategoryRequest(string Name, string? Description, int SortOrder);
public sealed record UpdateControlCategoryRequest(string Name, string? Description, int SortOrder);

public sealed record CreateControlRequest(
    string ControlId, string Name, string Description, string Objective, Guid ControlCategoryId, string RiskLevel,
    string? ApplicableConditions, string? EvidenceRequirementsSummary, string? Guidance, string SourceReference,
    DateOnly? EffectiveDate, DateOnly? ReviewDate);

public sealed record UpdateControlRequest(
    string Name, string Description, string Objective, Guid ControlCategoryId, string RiskLevel,
    string? ApplicableConditions, string? EvidenceRequirementsSummary, string? Guidance, string SourceReference,
    DateOnly? EffectiveDate, DateOnly? ReviewDate, string ReviewStatus);

public sealed record CreateControlMappingRequest(Guid ControlId, Guid RequirementId, string? MappingNotes);

public sealed record CreateAssessmentQuestionRequest(Guid ControlId, string Code, string Text, string? HelpText, string QuestionType, IReadOnlyList<string>? Options, bool IsRequired, int SortOrder);
public sealed record UpdateAssessmentQuestionRequest(string Text, string? HelpText, string QuestionType, IReadOnlyList<string>? Options, bool IsRequired, int SortOrder);

public sealed record CreateEvidenceRequirementRequest(Guid AssessmentQuestionId, string Name, string? Description, bool IsMandatory, string? AcceptableFormats);
public sealed record UpdateEvidenceRequirementRequest(string Name, string? Description, bool IsMandatory, string? AcceptableFormats);

public static class ComplianceEndpoints
{
    public static IEndpointRouteBuilder MapComplianceEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/compliance")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .WithTags("Compliance")
            .RequireAuthorization();

        // ---- Frameworks ----
        group.MapGet("/frameworks", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetFrameworksQuery(), ct)))
            .WithName("GetFrameworks")
            .Produces<IReadOnlyList<FrameworkDto>>()
            .RequirePermission(PermissionKeys.ControlsRead);

        group.MapPost("/frameworks", async (CreateFrameworkRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateFrameworkCommand(request.Name, request.Code, request.Jurisdiction, request.IssuingAuthority, request.Description), ct);
                return Results.Created($"/api/v1/compliance/frameworks/{result.Id}", result);
            })
            .WithName("CreateFramework")
            .Produces<FrameworkDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPut("/frameworks/{id:guid}", async (Guid id, UpdateFrameworkRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateFrameworkCommand(id, request.Name, request.Jurisdiction, request.IssuingAuthority, request.Description), ct)))
            .WithName("UpdateFramework")
            .Produces<FrameworkDto>()
            .RequirePermission(PermissionKeys.ControlsManage);

        // ---- Framework Versions ----
        group.MapGet("/framework-versions/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetFrameworkVersionDetailQuery(id), ct)))
            .WithName("GetFrameworkVersionDetail")
            .Produces<FrameworkVersionDto>()
            .RequirePermission(PermissionKeys.ControlsRead);

        group.MapPost("/frameworks/{frameworkId:guid}/versions", async (Guid frameworkId, CreateFrameworkVersionRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateFrameworkVersionCommand(
                    frameworkId, request.VersionLabel, request.OfficialCitation, request.PublicationDate, request.EffectiveDate, request.SourceUrl, request.ChangeSummary), ct);
                return Results.Created($"/api/v1/compliance/framework-versions/{result.Id}", result);
            })
            .WithName("CreateFrameworkVersion")
            .Produces<FrameworkVersionDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPut("/framework-versions/{id:guid}", async (Guid id, UpdateFrameworkVersionRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateFrameworkVersionCommand(
                    id, request.OfficialCitation, request.PublicationDate, request.EffectiveDate, request.SourceUrl, request.ChangeSummary, request.ReviewStatus), ct)))
            .WithName("UpdateFrameworkVersion")
            .Produces<FrameworkVersionDto>()
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPost("/framework-versions/{id:guid}/activate", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new ActivateFrameworkVersionCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("ActivateFrameworkVersion")
            .RequirePermission(PermissionKeys.ControlsManage);

        // ---- Legal References ----
        group.MapGet("/legal-references", async (Guid? frameworkVersionId, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetLegalReferencesQuery(frameworkVersionId), ct)))
            .WithName("GetLegalReferences")
            .Produces<IReadOnlyList<LegalReferenceDto>>()
            .RequirePermission(PermissionKeys.ControlsRead);

        group.MapPost("/legal-references", async (CreateLegalReferenceRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateLegalReferenceCommand(
                    request.FrameworkVersionId, request.Citation, request.Title, request.Chapter, request.SummaryText, request.SourceCitation), ct);
                return Results.Created($"/api/v1/compliance/legal-references/{result.Id}", result);
            })
            .WithName("CreateLegalReference")
            .Produces<LegalReferenceDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPut("/legal-references/{id:guid}", async (Guid id, UpdateLegalReferenceRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateLegalReferenceCommand(id, request.Title, request.Chapter, request.SummaryText, request.SourceCitation, request.ReviewStatus), ct)))
            .WithName("UpdateLegalReference")
            .Produces<LegalReferenceDto>()
            .RequirePermission(PermissionKeys.ControlsManage);

        // ---- Requirements ----
        group.MapGet("/requirements", async (Guid? legalReferenceId, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetRequirementsQuery(legalReferenceId), ct)))
            .WithName("GetRequirements")
            .Produces<IReadOnlyList<RequirementDto>>()
            .RequirePermission(PermissionKeys.ControlsRead);

        group.MapPost("/requirements", async (CreateRequirementRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateRequirementCommand(request.LegalReferenceId, request.Code, request.Title, request.Description), ct);
                return Results.Created($"/api/v1/compliance/requirements/{result.Id}", result);
            })
            .WithName("CreateRequirement")
            .Produces<RequirementDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPut("/requirements/{id:guid}", async (Guid id, UpdateRequirementRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateRequirementCommand(id, request.Title, request.Description, request.ReviewStatus), ct)))
            .WithName("UpdateRequirement")
            .Produces<RequirementDto>()
            .RequirePermission(PermissionKeys.ControlsManage);

        // ---- Control Categories ----
        group.MapGet("/control-categories", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetControlCategoriesQuery(), ct)))
            .WithName("GetControlCategories")
            .Produces<IReadOnlyList<ControlCategoryDto>>()
            .RequirePermission(PermissionKeys.ControlsRead);

        group.MapPost("/control-categories", async (CreateControlCategoryRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateControlCategoryCommand(request.Name, request.Description, request.SortOrder), ct);
                return Results.Created($"/api/v1/compliance/control-categories/{result.Id}", result);
            })
            .WithName("CreateControlCategory")
            .Produces<ControlCategoryDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPut("/control-categories/{id:guid}", async (Guid id, UpdateControlCategoryRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateControlCategoryCommand(id, request.Name, request.Description, request.SortOrder), ct)))
            .WithName("UpdateControlCategory")
            .Produces<ControlCategoryDto>()
            .RequirePermission(PermissionKeys.ControlsManage);

        // ---- Controls ----
        group.MapGet("/controls", async (
                int? page, int? pageSize, string? search, Guid? categoryId, string? riskLevel, string? status,
                Guid? frameworkVersionId, bool? hasApplicableConditions, string? sortBy, bool? sortDescending,
                ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetControlsQuery(
                    page ?? 1, pageSize ?? 25, search, categoryId, riskLevel, status, frameworkVersionId,
                    hasApplicableConditions, sortBy ?? "controlId", sortDescending ?? false), ct)))
            .WithName("GetControls")
            .Produces<PagedResult<ControlSummaryDto>>()
            .RequirePermission(PermissionKeys.ControlsRead);

        group.MapGet("/controls/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetControlByIdQuery(id), ct)))
            .WithName("GetControlById")
            .Produces<ControlDetailDto>()
            .RequirePermission(PermissionKeys.ControlsRead);

        group.MapPost("/controls", async (CreateControlRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateControlCommand(
                    request.ControlId, request.Name, request.Description, request.Objective, request.ControlCategoryId, request.RiskLevel,
                    request.ApplicableConditions, request.EvidenceRequirementsSummary, request.Guidance, request.SourceReference,
                    request.EffectiveDate, request.ReviewDate), ct);
                return Results.Created($"/api/v1/compliance/controls/{result.Id}", result);
            })
            .WithName("CreateControl")
            .Produces<ControlDetailDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPut("/controls/{id:guid}", async (Guid id, UpdateControlRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateControlCommand(
                    id, request.Name, request.Description, request.Objective, request.ControlCategoryId, request.RiskLevel,
                    request.ApplicableConditions, request.EvidenceRequirementsSummary, request.Guidance, request.SourceReference,
                    request.EffectiveDate, request.ReviewDate, request.ReviewStatus), ct)))
            .WithName("UpdateControl")
            .Produces<ControlDetailDto>()
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPost("/controls/{id:guid}/activate", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new ActivateControlCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("ActivateControl")
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPost("/controls/{id:guid}/retire", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RetireControlCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("RetireControl")
            .RequirePermission(PermissionKeys.ControlsManage);

        // ---- Control Mappings ----
        group.MapPost("/control-mappings", async (CreateControlMappingRequest request, ISender sender, CancellationToken ct) =>
            {
                var id = await sender.Send(new CreateControlMappingCommand(request.ControlId, request.RequirementId, request.MappingNotes), ct);
                return Results.Created($"/api/v1/compliance/control-mappings/{id}", new { id });
            })
            .WithName("CreateControlMapping")
            .Produces(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapDelete("/control-mappings/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteControlMappingCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteControlMapping")
            .RequirePermission(PermissionKeys.ControlsManage);

        // ---- Assessment Questions ----
        group.MapGet("/questions", async (int? page, int? pageSize, string? search, Guid? controlId, string? questionType, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetQuestionsQuery(page ?? 1, pageSize ?? 25, search, controlId, questionType), ct)))
            .WithName("GetQuestions")
            .Produces<PagedResult<AssessmentQuestionDto>>()
            .RequirePermission(PermissionKeys.ControlsRead);

        group.MapPost("/questions", async (CreateAssessmentQuestionRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateAssessmentQuestionCommand(
                    request.ControlId, request.Code, request.Text, request.HelpText, request.QuestionType, request.Options, request.IsRequired, request.SortOrder), ct);
                return Results.Created($"/api/v1/compliance/questions/{result.Id}", result);
            })
            .WithName("CreateAssessmentQuestion")
            .Produces<AssessmentQuestionDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPut("/questions/{id:guid}", async (Guid id, UpdateAssessmentQuestionRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateAssessmentQuestionCommand(
                    id, request.Text, request.HelpText, request.QuestionType, request.Options, request.IsRequired, request.SortOrder), ct)))
            .WithName("UpdateAssessmentQuestion")
            .Produces<AssessmentQuestionDto>()
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapDelete("/questions/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteAssessmentQuestionCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteAssessmentQuestion")
            .RequirePermission(PermissionKeys.ControlsManage);

        // ---- Evidence Requirements ----
        group.MapGet("/evidence-requirements", async (int? page, int? pageSize, string? search, Guid? assessmentQuestionId, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetEvidenceRequirementsQuery(page ?? 1, pageSize ?? 25, search, assessmentQuestionId), ct)))
            .WithName("GetEvidenceRequirements")
            .Produces<PagedResult<EvidenceRequirementDto>>()
            .RequirePermission(PermissionKeys.ControlsRead);

        group.MapPost("/evidence-requirements", async (CreateEvidenceRequirementRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateEvidenceRequirementCommand(
                    request.AssessmentQuestionId, request.Name, request.Description, request.IsMandatory, request.AcceptableFormats), ct);
                return Results.Created($"/api/v1/compliance/evidence-requirements/{result.Id}", result);
            })
            .WithName("CreateEvidenceRequirement")
            .Produces<EvidenceRequirementDto>(StatusCodes.Status201Created)
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapPut("/evidence-requirements/{id:guid}", async (Guid id, UpdateEvidenceRequirementRequest request, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new UpdateEvidenceRequirementCommand(id, request.Name, request.Description, request.IsMandatory, request.AcceptableFormats), ct)))
            .WithName("UpdateEvidenceRequirement")
            .Produces<EvidenceRequirementDto>()
            .RequirePermission(PermissionKeys.ControlsManage);

        group.MapDelete("/evidence-requirements/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteEvidenceRequirementCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteEvidenceRequirement")
            .RequirePermission(PermissionKeys.ControlsManage);

        // ---- Reference data ----
        group.MapGet("/answer-statuses", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetAnswerStatusesQuery(), ct)))
            .WithName("GetAnswerStatuses")
            .Produces<IReadOnlyList<string>>()
            .RequirePermission(PermissionKeys.ControlsRead);

        return app;
    }
}
