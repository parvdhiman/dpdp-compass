using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DPDP.ApiTests.Assessments;

/// <summary>
/// Module 5 quality-gate tests: workflow transitions, evidence validation,
/// authorization, and tenant isolation, exercised end-to-end at the HTTP
/// layer against the real seeded DPDP Act 2023 control library. Score
/// calculation / N/A handling / partial answers are covered as fast unit
/// tests instead (DPDP.UnitTests — DefaultComplianceScoringStrategyTests,
/// ControlStatusCalculatorTests) since they're pure functions; this class
/// covers everything that genuinely needs the database and HTTP pipeline.
/// </summary>
[Collection("Assessments")]
public sealed class AssessmentWorkflowApiTests(AssessmentApiFixture fixture)
{
    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    private async Task<AssessmentDetail> CreateAssessmentAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/v1/assessments", new
        {
            frameworkVersionId = fixture.FrameworkVersionId,
            name,
            description = (string?)null,
            assignedToUserId = (Guid?)null,
            dueDate = (DateOnly?)null,
            scopes = (object?)null,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AssessmentDetail>())!;
    }

    private static async Task<List<QuestionnaireControl>> GetQuestionnaireAsync(HttpClient client, Guid assessmentId) =>
        (await client.GetFromJsonAsync<List<QuestionnaireControl>>($"/api/v1/assessments/{assessmentId}/questionnaire"))!;

    private static async Task AnswerAllRequiredQuestionsAsync(HttpClient client, Guid assessmentId, string status = "PASS")
    {
        var controls = await GetQuestionnaireAsync(client, assessmentId);
        foreach (var control in controls)
        {
            foreach (var question in control.Questions.Where(q => q.IsRequired))
            {
                var hasMandatoryEvidence = question.EvidenceRequirements.Any(e => e.IsMandatory);
                var response = await client.PutAsJsonAsync($"/api/v1/assessments/answers/{question.AssessmentControlQuestionId}", new
                {
                    status,
                    answerValue = "true",
                    answerValues = (List<string>?)null,
                    comment = "answered by test",
                    evidence = hasMandatoryEvidence
                        ? new[] { new { requirementId = (Guid?)null, description = "test evidence", url = "https://example.test/evidence" } }
                        : null,
                    confidence = "HIGH",
                    assessedRiskLevel = (string?)null,
                    remediationNotes = (string?)null,
                });
                Assert.True(response.IsSuccessStatusCode, $"Failed to save answer: {await response.Content.ReadAsStringAsync()}");
            }
        }
    }

    [Fact]
    public async Task Full_lifecycle_create_answer_submit_review_approve_succeeds()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);

        var created = await CreateAssessmentAsync(client, $"Lifecycle Test {Guid.NewGuid():N}");
        Assert.Equal("DRAFT", created.Status);
        Assert.NotEmpty(created.Controls);

        await AnswerAllRequiredQuestionsAsync(client, created.Id);

        var afterAnswering = await client.GetFromJsonAsync<AssessmentDetail>($"/api/v1/assessments/{created.Id}");
        Assert.Equal("IN_PROGRESS", afterAnswering!.Status);

        var scoreAfterAnswering = await client.GetFromJsonAsync<AssessmentScore>($"/api/v1/assessments/{created.Id}/score");
        Assert.Equal(100.0, scoreAfterAnswering!.OverallScore);

        var submitResponse = await client.PostAsync($"/api/v1/assessments/{created.Id}/submit", null);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);
        var submitted = await submitResponse.Content.ReadFromJsonAsync<AssessmentDetail>();
        Assert.Equal("SUBMITTED", submitted!.Status);

        var reviewResponse = await client.PostAsJsonAsync($"/api/v1/assessments/{created.Id}/review", new { decision = "READY_FOR_APPROVAL", comments = "Looks good" });
        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);
        var reviewed = await reviewResponse.Content.ReadFromJsonAsync<AssessmentDetail>();
        Assert.Equal("UNDER_REVIEW", reviewed!.Status);
        Assert.Single(reviewed.Reviews);

        var approveResponse = await client.PostAsJsonAsync($"/api/v1/assessments/{created.Id}/approve", new { comments = "Approved" });
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        var approved = await approveResponse.Content.ReadFromJsonAsync<AssessmentDetail>();
        Assert.Equal("APPROVED", approved!.Status);
        Assert.Single(approved.Approvals);
        Assert.NotNull(approved.DecidedAt);

        var archiveResponse = await client.PostAsync($"/api/v1/assessments/{created.Id}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);
        var archived = await archiveResponse.Content.ReadFromJsonAsync<AssessmentDetail>();
        Assert.Equal("ARCHIVED", archived!.Status);
    }

    [Fact]
    public async Task Reject_then_reopen_returns_the_assessment_to_in_progress_for_resubmission()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);

        var created = await CreateAssessmentAsync(client, $"Reject Reopen Test {Guid.NewGuid():N}");
        await AnswerAllRequiredQuestionsAsync(client, created.Id);
        await client.PostAsync($"/api/v1/assessments/{created.Id}/submit", null);
        await client.PostAsJsonAsync($"/api/v1/assessments/{created.Id}/review", new { decision = "NEEDS_CHANGES", comments = "Please revise" });

        var rejectResponse = await client.PostAsJsonAsync($"/api/v1/assessments/{created.Id}/reject", new { comments = "Not sufficient evidence" });
        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);
        var rejected = await rejectResponse.Content.ReadFromJsonAsync<AssessmentDetail>();
        Assert.Equal("REJECTED", rejected!.Status);

        var reopenResponse = await client.PostAsync($"/api/v1/assessments/{created.Id}/reopen", null);
        Assert.Equal(HttpStatusCode.OK, reopenResponse.StatusCode);
        var reopened = await reopenResponse.Content.ReadFromJsonAsync<AssessmentDetail>();
        Assert.Equal("IN_PROGRESS", reopened!.Status);
        Assert.Null(reopened.SubmittedAt);
        Assert.Null(reopened.DecidedAt);

        // Cannot reopen a second time from IN_PROGRESS.
        var secondReopen = await client.PostAsync($"/api/v1/assessments/{created.Id}/reopen", null);
        Assert.Equal(HttpStatusCode.Conflict, secondReopen.StatusCode);
    }

    [Fact]
    public async Task Cannot_submit_while_required_questions_are_unanswered()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var created = await CreateAssessmentAsync(client, $"Incomplete Submit Test {Guid.NewGuid():N}");

        var response = await client.PostAsync($"/api/v1/assessments/{created.Id}/submit", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_approve_an_assessment_that_is_not_under_review()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var created = await CreateAssessmentAsync(client, $"Premature Approve Test {Guid.NewGuid():N}");

        var response = await client.PostAsJsonAsync($"/api/v1/assessments/{created.Id}/approve", new { comments = (string?)null });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Marking_a_question_pass_without_mandatory_evidence_is_rejected()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var created = await CreateAssessmentAsync(client, $"Evidence Validation Test {Guid.NewGuid():N}");

        var controls = await GetQuestionnaireAsync(client, created.Id);
        var questionRequiringEvidence = controls
            .SelectMany(c => c.Questions)
            .First(q => q.EvidenceRequirements.Any(e => e.IsMandatory));

        var response = await client.PutAsJsonAsync($"/api/v1/assessments/answers/{questionRequiringEvidence.AssessmentControlQuestionId}", new
        {
            status = "PASS",
            answerValue = "true",
            answerValues = (List<string>?)null,
            comment = (string?)null,
            evidence = (object?)null,
            confidence = (string?)null,
            assessedRiskLevel = (string?)null,
            remediationNotes = (string?)null,
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // Attaching evidence makes the same transition succeed.
        var withEvidenceResponse = await client.PutAsJsonAsync($"/api/v1/assessments/answers/{questionRequiringEvidence.AssessmentControlQuestionId}", new
        {
            status = "PASS",
            answerValue = "true",
            answerValues = (List<string>?)null,
            comment = (string?)null,
            evidence = new[] { new { requirementId = (Guid?)null, description = "policy document", url = "https://example.test/policy" } },
            confidence = (string?)null,
            assessedRiskLevel = (string?)null,
            remediationNotes = (string?)null,
        });

        Assert.Equal(HttpStatusCode.OK, withEvidenceResponse.StatusCode);
    }

    [Fact]
    public async Task Marking_a_question_not_applicable_never_requires_evidence()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var created = await CreateAssessmentAsync(client, $"NA No Evidence Test {Guid.NewGuid():N}");

        var controls = await GetQuestionnaireAsync(client, created.Id);
        var questionRequiringEvidence = controls
            .SelectMany(c => c.Questions)
            .First(q => q.EvidenceRequirements.Any(e => e.IsMandatory));

        var response = await client.PutAsJsonAsync($"/api/v1/assessments/answers/{questionRequiringEvidence.AssessmentControlQuestionId}", new
        {
            status = "NOT_APPLICABLE",
            answerValue = (string?)null,
            answerValues = (List<string>?)null,
            comment = "not applicable to us",
            evidence = (object?)null,
            confidence = (string?)null,
            assessedRiskLevel = (string?)null,
            remediationNotes = (string?)null,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Read_only_user_cannot_create_an_assessment()
    {
        var client = await AuthenticatedClientAsync(fixture.ReadOnlyAEmail);

        var response = await client.PostAsJsonAsync("/api/v1/assessments", new
        {
            frameworkVersionId = fixture.FrameworkVersionId,
            name = "Should not be created",
            description = (string?)null,
            assignedToUserId = (Guid?)null,
            dueDate = (DateOnly?)null,
            scopes = (object?)null,
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Read_only_user_can_read_but_not_approve()
    {
        var officerClient = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var created = await CreateAssessmentAsync(officerClient, $"ReadOnly Approve Test {Guid.NewGuid():N}");

        var readOnlyClient = await AuthenticatedClientAsync(fixture.ReadOnlyAEmail);

        var readResponse = await readOnlyClient.GetAsync($"/api/v1/assessments/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        var approveResponse = await readOnlyClient.PostAsJsonAsync($"/api/v1/assessments/{created.Id}/approve", new { comments = (string?)null });
        Assert.Equal(HttpStatusCode.Forbidden, approveResponse.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_request_is_rejected()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/v1/assessments");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Organisation_B_cannot_read_organisation_As_assessment()
    {
        var clientA = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var created = await CreateAssessmentAsync(clientA, $"Tenant Isolation Test {Guid.NewGuid():N}");

        var clientB = await AuthenticatedClientAsync(fixture.ComplianceOfficerBEmail);

        var response = await clientB.GetAsync($"/api/v1/assessments/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Organisation_B_does_not_see_organisation_As_assessment_in_the_list()
    {
        var clientA = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var created = await CreateAssessmentAsync(clientA, $"Tenant Isolation List Test {Guid.NewGuid():N}");

        var clientB = await AuthenticatedClientAsync(fixture.ComplianceOfficerBEmail);
        var list = await clientB.GetFromJsonAsync<PagedAssessments>("/api/v1/assessments?pageSize=100");

        Assert.DoesNotContain(list!.Items, a => a.Id == created.Id);
    }

    [Fact]
    public async Task Organisation_B_cannot_save_an_answer_on_organisation_As_assessment()
    {
        var clientA = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var created = await CreateAssessmentAsync(clientA, $"Tenant Isolation Answer Test {Guid.NewGuid():N}");
        var controls = await GetQuestionnaireAsync(clientA, created.Id);
        var firstQuestion = controls.SelectMany(c => c.Questions).First();

        var clientB = await AuthenticatedClientAsync(fixture.ComplianceOfficerBEmail);

        var response = await clientB.PutAsJsonAsync($"/api/v1/assessments/answers/{firstQuestion.AssessmentControlQuestionId}", new
        {
            status = "PASS",
            answerValue = "true",
            answerValues = (List<string>?)null,
            comment = "intruding",
            evidence = (object?)null,
            confidence = (string?)null,
            assessedRiskLevel = (string?)null,
            remediationNotes = (string?)null,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record AssessmentDetail(
        Guid Id, string Name, string Status, Guid FrameworkVersionId, DateTimeOffset? SubmittedAt, DateTimeOffset? DecidedAt,
        List<AssessmentControlSummary> Controls, List<object> Reviews, List<object> Approvals);

    private sealed record AssessmentControlSummary(Guid Id, string ControlBusinessId, string Status);
    private sealed record AssessmentScore(double? OverallScore, double? ControlScore, double? RiskAdjustedScore);
    private sealed record QuestionnaireControl(Guid AssessmentControlId, string ControlBusinessId, List<QuestionnaireQuestion> Questions);

    private sealed record QuestionnaireQuestion(
        Guid AssessmentControlQuestionId, bool IsRequired, List<EvidenceRequirement> EvidenceRequirements);

    private sealed record EvidenceRequirement(bool IsMandatory);
    private sealed record PagedAssessments(List<AssessmentListItem> Items);
    private sealed record AssessmentListItem(Guid Id);
}
