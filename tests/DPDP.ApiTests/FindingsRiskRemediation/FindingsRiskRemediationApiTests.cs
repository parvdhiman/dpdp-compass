using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DPDP.ApiTests.FindingsRiskRemediation;

/// <summary>
/// Module 6 quality-gate tests: workflow transitions, authorization, and
/// tenant isolation for Findings/Risk/Remediation, exercised end-to-end at
/// the HTTP layer. Risk-methodology configurability and the pure
/// FindingStatusTransitions/RemediationStatusTransitions rules are covered
/// as fast unit tests instead (DPDP.UnitTests) since they need no
/// database; this class covers what genuinely needs the database and HTTP
/// pipeline.
/// </summary>
[Collection("FindingsRiskRemediation")]
public sealed class FindingsRiskRemediationApiTests(FindingsRiskRemediationApiFixture fixture)
{
    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    private async Task<FindingDetail> CreateFindingAsync(HttpClient client, string title, string severity = "HIGH")
    {
        var response = await client.PostAsJsonAsync("/api/v1/findings", new
        {
            title,
            description = "Test finding description",
            severity,
            controlId = (Guid?)null,
            assetReference = (string?)null,
            ownerUserId = (Guid?)null,
            dueDate = (DateOnly?)null,
            recommendation = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<FindingDetail>())!;
    }

    /// <summary>Creates an assessment, marks one control's required question FAIL, and returns that AssessmentControl's id — the precondition for CreateFindingFromAssessmentControlCommand.</summary>
    private async Task<Guid> CreateFailedAssessmentControlAsync(HttpClient client)
    {
        var createResponse = await client.PostAsJsonAsync("/api/v1/assessments", new
        {
            frameworkVersionId = fixture.FrameworkVersionId,
            name = $"Finding Source Assessment {Guid.NewGuid():N}",
            description = (string?)null,
            assignedToUserId = (Guid?)null,
            dueDate = (DateOnly?)null,
            scopes = (object?)null,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var assessment = await createResponse.Content.ReadFromJsonAsync<CreatedAssessment>();

        var questionnaireResponse = await client.GetAsync($"/api/v1/assessments/{assessment!.Id}/questionnaire");
        var controls = await questionnaireResponse.Content.ReadFromJsonAsync<List<QuestionnaireControl>>();
        var firstControl = controls![0];
        var firstQuestion = firstControl.Questions[0];

        var answerResponse = await client.PutAsJsonAsync($"/api/v1/assessments/answers/{firstQuestion.AssessmentControlQuestionId}", new
        {
            status = "FAIL",
            answerValue = "false",
            answerValues = (List<string>?)null,
            comment = "Failing on purpose for the finding-generation test.",
            evidence = (object?)null,
            confidence = (string?)null,
            assessedRiskLevel = (string?)null,
            remediationNotes = (string?)null,
        });
        Assert.Equal(HttpStatusCode.OK, answerResponse.StatusCode);

        return firstControl.AssessmentControlId;
    }

    [Fact]
    public async Task Creating_a_finding_from_a_failed_assessment_control_populates_it_from_the_control_and_is_idempotent()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var assessmentControlId = await CreateFailedAssessmentControlAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/findings/from-assessment-control", new { assessmentControlId, recommendation = (string?)null });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var finding = await response.Content.ReadFromJsonAsync<FindingDetail>();
        Assert.Equal("OPEN", finding!.Status);
        Assert.NotNull(finding.ControlId);

        // Idempotent: a second attempt against the same control is rejected, not a duplicate.
        var secondResponse = await client.PostAsJsonAsync("/api/v1/findings/from-assessment-control", new { assessmentControlId, recommendation = (string?)null });
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Cannot_create_a_finding_from_a_control_that_has_not_failed_or_partially_failed()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);

        var createResponse = await client.PostAsJsonAsync("/api/v1/assessments", new
        {
            frameworkVersionId = fixture.FrameworkVersionId,
            name = $"Passing Assessment {Guid.NewGuid():N}",
            description = (string?)null,
            assignedToUserId = (Guid?)null,
            dueDate = (DateOnly?)null,
            scopes = (object?)null,
        });
        var assessment = await createResponse.Content.ReadFromJsonAsync<CreatedAssessment>();
        var questionnaireResponse = await client.GetAsync($"/api/v1/assessments/{assessment!.Id}/questionnaire");
        var controls = await questionnaireResponse.Content.ReadFromJsonAsync<List<QuestionnaireControl>>();
        var untouchedControlId = controls![0].AssessmentControlId;

        // Still NOT_ASSESSED — never answered.
        var response = await client.PostAsJsonAsync("/api/v1/findings/from-assessment-control", new { assessmentControlId = untouchedControlId, recommendation = (string?)null });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Full_finding_lifecycle_assign_progress_resolve_close_succeeds()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(client, $"Lifecycle Finding {Guid.NewGuid():N}");
        Assert.Equal("OPEN", finding.Status);

        var assignResponse = await client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/assign", new { ownerUserId = fixture.ComplianceOfficerAUserId });
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        var assigned = await assignResponse.Content.ReadFromJsonAsync<FindingDetail>();
        Assert.Equal("ASSIGNED", assigned!.Status);

        var inProgressResponse = await client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/status", new { status = "IN_PROGRESS" });
        Assert.Equal(HttpStatusCode.OK, inProgressResponse.StatusCode);

        var pendingResponse = await client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/status", new { status = "PENDING_VERIFICATION" });
        Assert.Equal(HttpStatusCode.OK, pendingResponse.StatusCode);

        var resolvedResponse = await client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/status", new { status = "RESOLVED" });
        Assert.Equal(HttpStatusCode.OK, resolvedResponse.StatusCode);
        var resolved = await resolvedResponse.Content.ReadFromJsonAsync<FindingDetail>();
        Assert.Equal("RESOLVED", resolved!.Status);

        var closeResponse = await client.PostAsync($"/api/v1/findings/{finding.Id}/close", null);
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        var closed = await closeResponse.Content.ReadFromJsonAsync<FindingDetail>();
        Assert.Equal("CLOSED", closed!.Status);
    }

    [Fact]
    public async Task Skipping_a_workflow_step_is_rejected()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(client, $"Skip Step Finding {Guid.NewGuid():N}");

        // OPEN -> RESOLVED directly is not a valid transition (must pass through ASSIGNED/IN_PROGRESS/PENDING_VERIFICATION).
        var response = await client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/status", new { status = "RESOLVED" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_close_a_finding_that_has_not_been_resolved()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(client, $"Premature Close Finding {Guid.NewGuid():N}");

        var response = await client.PostAsync($"/api/v1/findings/{finding.Id}/close", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Accepting_risk_is_reachable_from_an_open_finding_and_is_terminal()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(client, $"Accept Risk Finding {Guid.NewGuid():N}");

        var response = await client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/accept-risk", new { comments = "Business accepts this residual risk." });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accepted = await response.Content.ReadFromJsonAsync<FindingDetail>();
        Assert.Equal("ACCEPTED_RISK", accepted!.Status);

        var furtherResponse = await client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/status", new { status = "IN_PROGRESS" });
        Assert.Equal(HttpStatusCode.Conflict, furtherResponse.StatusCode);
    }

    [Fact]
    public async Task Creating_a_risk_from_a_finding_links_them_and_computes_a_score()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(client, $"Risk Link Finding {Guid.NewGuid():N}", severity: "CRITICAL");

        var response = await client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/risk", new { likelihood = "LIKELY", impact = "MAJOR", treatmentPlan = "Mitigate via encryption." });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var withRisk = await response.Content.ReadFromJsonAsync<FindingDetail>();
        Assert.NotNull(withRisk!.RiskId);
        Assert.NotNull(withRisk.RiskLevel);

        var riskResponse = await client.GetAsync($"/api/v1/risks/{withRisk.RiskId}");
        Assert.Equal(HttpStatusCode.OK, riskResponse.StatusCode);
        var risk = await riskResponse.Content.ReadFromJsonAsync<RiskDetail>();
        Assert.True(risk!.CalculatedRiskScore > 0);

        // Cannot link a second risk to the same finding.
        var secondLinkResponse = await client.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/risk", new { likelihood = "RARE", impact = "MINOR", treatmentPlan = (string?)null });
        Assert.Equal(HttpStatusCode.Conflict, secondLinkResponse.StatusCode);
    }

    [Fact]
    public async Task Remediation_task_lifecycle_create_assign_evidence_verify_close_succeeds()
    {
        var client = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(client, $"Remediation Finding {Guid.NewGuid():N}");

        var createResponse = await client.PostAsJsonAsync("/api/v1/remediation-tasks", new
        {
            findingId = finding.Id,
            title = "Patch the vulnerable dependency",
            description = "Upgrade to the fixed version.",
            ownerUserId = fixture.ComplianceOfficerAUserId,
            dueDate = (DateOnly?)null,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var task = await createResponse.Content.ReadFromJsonAsync<RemediationTaskDetail>();
        Assert.Equal("OPEN", task!.Status);

        // Creating a remediation task moves the finding into progress.
        var findingAfterTask = await client.GetFromJsonAsync<FindingDetail>($"/api/v1/findings/{finding.Id}");
        Assert.Equal("IN_PROGRESS", findingAfterTask!.Status);

        var inProgressResponse = await client.PostAsJsonAsync($"/api/v1/remediation-tasks/{task.Id}/status", new { status = "IN_PROGRESS" });
        Assert.Equal(HttpStatusCode.OK, inProgressResponse.StatusCode);

        var evidenceResponse = await client.PostAsJsonAsync($"/api/v1/remediation-tasks/{task.Id}/evidence", new
        {
            evidence = new[] { new { description = "Upgrade PR merged", url = "https://example.test/pr/1" } },
        });
        Assert.Equal(HttpStatusCode.OK, evidenceResponse.StatusCode);
        var withEvidence = await evidenceResponse.Content.ReadFromJsonAsync<RemediationTaskDetail>();
        Assert.Equal("PENDING_VERIFICATION", withEvidence!.Status);
        Assert.NotEmpty(withEvidence.Evidence);

        var commentResponse = await client.PostAsJsonAsync($"/api/v1/remediation-tasks/{task.Id}/comments", new { comment = "Looks good to verify." });
        Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

        var verifyResponse = await client.PostAsJsonAsync($"/api/v1/remediation-tasks/{task.Id}/verify", new { verificationNotes = "Confirmed the fix is deployed." });
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        var verified = await verifyResponse.Content.ReadFromJsonAsync<RemediationTaskDetail>();
        Assert.Equal("VERIFIED", verified!.Status);
        Assert.NotNull(verified.VerifiedAt);

        var closeResponse = await client.PostAsync($"/api/v1/remediation-tasks/{task.Id}/close", null);
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        var closed = await closeResponse.Content.ReadFromJsonAsync<RemediationTaskDetail>();
        Assert.Equal("CLOSED", closed!.Status);

        // Verifying/closing the remediation task does NOT auto-close the finding —
        // that still requires a separate findings.close-gated action.
        var findingAfterClose = await client.GetFromJsonAsync<FindingDetail>($"/api/v1/findings/{finding.Id}");
        Assert.Equal("IN_PROGRESS", findingAfterClose!.Status);
    }

    [Fact]
    public async Task Read_only_user_cannot_create_a_finding_or_a_risk()
    {
        var client = await AuthenticatedClientAsync(fixture.ReadOnlyAEmail);

        var findingResponse = await client.PostAsJsonAsync("/api/v1/findings", new
        {
            title = "Should not be created",
            description = "n/a",
            severity = "LOW",
            controlId = (Guid?)null,
            assetReference = (string?)null,
            ownerUserId = (Guid?)null,
            dueDate = (DateOnly?)null,
            recommendation = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Forbidden, findingResponse.StatusCode);

        var riskResponse = await client.PostAsJsonAsync("/api/v1/risks", new
        {
            title = "Should not be created",
            description = "n/a",
            likelihood = "POSSIBLE",
            impact = "MODERATE",
            dataSensitivity = "LOW",
            exposure = "LOW",
            ownerUserId = (Guid?)null,
            treatmentPlan = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Forbidden, riskResponse.StatusCode);
    }

    [Fact]
    public async Task Read_only_user_can_read_findings_but_cannot_close_them()
    {
        var officerClient = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(officerClient, $"ReadOnly Close Finding {Guid.NewGuid():N}");

        var readOnlyClient = await AuthenticatedClientAsync(fixture.ReadOnlyAEmail);
        var readResponse = await readOnlyClient.GetAsync($"/api/v1/findings/{finding.Id}");
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        var closeResponse = await readOnlyClient.PostAsync($"/api/v1/findings/{finding.Id}/close", null);
        Assert.Equal(HttpStatusCode.Forbidden, closeResponse.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_request_is_rejected()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/v1/findings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Organisation_B_cannot_read_organisation_As_finding()
    {
        var clientA = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(clientA, $"Tenant Isolation Finding {Guid.NewGuid():N}");

        var clientB = await AuthenticatedClientAsync(fixture.ComplianceOfficerBEmail);
        var response = await clientB.GetAsync($"/api/v1/findings/{finding.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Organisation_B_does_not_see_organisation_As_finding_in_the_list()
    {
        var clientA = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(clientA, $"Tenant Isolation List Finding {Guid.NewGuid():N}");

        var clientB = await AuthenticatedClientAsync(fixture.ComplianceOfficerBEmail);
        var list = await clientB.GetFromJsonAsync<PagedFindings>("/api/v1/findings?pageSize=100");

        Assert.DoesNotContain(list!.Items, f => f.Id == finding.Id);
    }

    [Fact]
    public async Task Organisation_B_cannot_assign_organisation_As_finding()
    {
        var clientA = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var finding = await CreateFindingAsync(clientA, $"Tenant Isolation Assign Finding {Guid.NewGuid():N}");

        var clientB = await AuthenticatedClientAsync(fixture.ComplianceOfficerBEmail);
        var response = await clientB.PostAsJsonAsync($"/api/v1/findings/{finding.Id}/assign", new { ownerUserId = fixture.ComplianceOfficerAUserId });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record FindingDetail(Guid Id, string Status, Guid? RiskId, string? RiskLevel, Guid? ControlId);
    private sealed record RiskDetail(Guid Id, double CalculatedRiskScore);
    private sealed record RemediationTaskDetail(Guid Id, string Status, DateTimeOffset? VerifiedAt, List<object> Evidence);
    private sealed record PagedFindings(List<FindingListItem> Items);
    private sealed record FindingListItem(Guid Id);
    private sealed record CreatedAssessment(Guid Id);
    private sealed record QuestionnaireControl(Guid AssessmentControlId, List<QuestionnaireQuestion> Questions);
    private sealed record QuestionnaireQuestion(Guid AssessmentControlQuestionId);
}
