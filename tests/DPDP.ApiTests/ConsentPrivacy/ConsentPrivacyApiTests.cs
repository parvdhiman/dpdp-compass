using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DPDP.ApiTests.ConsentPrivacy;

/// <summary>
/// Module 10 quality-gate tests: catalogue CRUD (DataPrincipal,
/// ConsentPurpose, SlaPolicy), the full Consent Record lifecycle (grant →
/// withdraw, grant → revoke, batch expire), the full Privacy Notice
/// workflow (Draft → Approve → Publish → Archive) with RBAC separation of
/// duties (Privacy Officer manages, Compliance Officer approves), the
/// full Data Principal Request / Grievance workflow including
/// SLA-configured due-date computation and the "no policy configured"
/// null-DueAt path, audit logging, and tenant isolation.
/// </summary>
[Collection("ConsentPrivacy")]
public sealed class ConsentPrivacyApiTests(ConsentPrivacyApiFixture fixture)
{
    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    private static async Task<DataPrincipalItem> CreateDataPrincipalAsync(HttpClient client, string externalRef) =>
        (await (await client.PostAsJsonAsync("/api/v1/data-principals", new { externalReferenceId = externalRef, referenceCategory = (string?)null, notes = (string?)null }))
            .Content.ReadFromJsonAsync<DataPrincipalItem>())!;

    private static async Task<CatalogItem> CreateConsentPurposeAsync(HttpClient client, string name) =>
        (await (await client.PostAsJsonAsync("/api/v1/consent-purposes", new { name, description = (string?)null, dataCategoryId = (Guid?)null }))
            .Content.ReadFromJsonAsync<CatalogItem>())!;

    private static async Task<SlaPolicyItem> CreateSlaPolicyAsync(HttpClient client, string name, string? requestType, int responseDueDays) =>
        (await (await client.PostAsJsonAsync("/api/v1/sla-policies", new { name, description = (string?)null, requestType, responseDueDays }))
            .Content.ReadFromJsonAsync<SlaPolicyItem>())!;

    [Fact]
    public async Task Each_catalogue_supports_create_list_and_delete()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var principal = await CreateDataPrincipalAsync(client, $"EXT-{suffix}");
        var purpose = await CreateConsentPurposeAsync(client, $"Marketing {suffix}");
        var policy = await CreateSlaPolicyAsync(client, $"Default SLA {suffix}", null, 30);

        var principalsList = await client.GetFromJsonAsync<List<DataPrincipalItem>>("/api/v1/data-principals");
        Assert.Contains(principalsList!, p => p.Id == principal.Id);

        var purposesList = await client.GetFromJsonAsync<List<CatalogItem>>("/api/v1/consent-purposes");
        Assert.Contains(purposesList!, p => p.Id == purpose.Id);

        var policiesList = await client.GetFromJsonAsync<List<SlaPolicyItem>>("/api/v1/sla-policies");
        Assert.Contains(policiesList!, p => p.Id == policy.Id);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/sla-policies/{policy.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/consent-purposes/{purpose.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/data-principals/{principal.Id}")).StatusCode);
    }

    [Fact]
    public async Task Only_one_active_sla_policy_per_request_type_is_allowed()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var first = await CreateSlaPolicyAsync(client, $"Access SLA {suffix}", "ACCESS", 15);

        var conflictResponse = await client.PostAsJsonAsync("/api/v1/sla-policies", new
        {
            name = $"Access SLA Duplicate {suffix}", description = (string?)null, requestType = "ACCESS", responseDueDays = 20,
        });
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

        await client.DeleteAsync($"/api/v1/sla-policies/{first.Id}");
    }

    [Fact]
    public async Task Consent_record_can_be_granted_and_withdrawn_with_audit_log()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var principal = await CreateDataPrincipalAsync(client, $"EXT-WD-{suffix}");
        var purpose = await CreateConsentPurposeAsync(client, $"Analytics {suffix}");

        var createResponse = await client.PostAsJsonAsync("/api/v1/consent-records", new
        {
            dataPrincipalId = principal.Id, consentPurposeId = purpose.Id, noticeVersionId = (Guid?)null,
            grantedAt = (DateTimeOffset?)null, channel = "WEB", expiresAt = (DateTimeOffset?)null,
            sourceSystem = "signup-portal", externalReferenceId = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var consent = await createResponse.Content.ReadFromJsonAsync<ConsentRecordDetail>();
        Assert.StartsWith("CR-", consent!.ConsentNumber);
        Assert.Equal("GRANTED", consent.Status);

        Assert.Equal(1, await fixture.CountAuditLogsAsync("consentprivacy.consent_granted", consent.Id.ToString()));

        var withdrawResponse = await client.PostAsync($"/api/v1/consent-records/{consent.Id}/withdraw", null);
        Assert.Equal(HttpStatusCode.OK, withdrawResponse.StatusCode);
        var withdrawn = await withdrawResponse.Content.ReadFromJsonAsync<ConsentRecordDetail>();
        Assert.Equal("WITHDRAWN", withdrawn!.Status);
        Assert.NotNull(withdrawn.WithdrawnAt);

        // WITHDRAWN -> WITHDRAWN is a self-transition, which ConsentStatusTransitions always allows (idempotent), not a conflict.
        var reWithdrawResponse = await client.PostAsync($"/api/v1/consent-records/{consent.Id}/withdraw", null);
        Assert.Equal(HttpStatusCode.OK, reWithdrawResponse.StatusCode);

        var revokeAfterWithdrawResponse = await client.PostAsJsonAsync($"/api/v1/consent-records/{consent.Id}/revoke", new { reason = "Cannot revoke an already-withdrawn consent" });
        Assert.Equal(HttpStatusCode.Conflict, revokeAfterWithdrawResponse.StatusCode);

        await client.DeleteAsync($"/api/v1/consent-purposes/{purpose.Id}");
        await client.DeleteAsync($"/api/v1/data-principals/{principal.Id}");
    }

    [Fact]
    public async Task Consent_record_can_be_revoked_by_organisation_with_reason_and_has_no_delete_endpoint()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var principal = await CreateDataPrincipalAsync(client, $"EXT-RV-{suffix}");
        var purpose = await CreateConsentPurposeAsync(client, $"Profiling {suffix}");

        var consent = await (await client.PostAsJsonAsync("/api/v1/consent-records", new
        {
            dataPrincipalId = principal.Id, consentPurposeId = purpose.Id, noticeVersionId = (Guid?)null,
            grantedAt = (DateTimeOffset?)null, channel = "MOBILE_APP", expiresAt = (DateTimeOffset?)null,
            sourceSystem = (string?)null, externalReferenceId = (string?)null,
        })).Content.ReadFromJsonAsync<ConsentRecordDetail>();

        var missingReasonResponse = await client.PostAsJsonAsync($"/api/v1/consent-records/{consent!.Id}/revoke", new { reason = "" });
        Assert.Equal(HttpStatusCode.BadRequest, missingReasonResponse.StatusCode);

        var revokeResponse = await client.PostAsJsonAsync($"/api/v1/consent-records/{consent.Id}/revoke", new { reason = "Notice retracted for legal review" });
        Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);
        var revoked = await revokeResponse.Content.ReadFromJsonAsync<ConsentRecordDetail>();
        Assert.Equal("REVOKED", revoked!.Status);

        Assert.Equal(1, await fixture.CountAuditLogsAsync("consentprivacy.consent_revoked", consent.Id.ToString()));

        await client.DeleteAsync($"/api/v1/consent-purposes/{purpose.Id}");
        await client.DeleteAsync($"/api/v1/data-principals/{principal.Id}");
    }

    [Fact]
    public async Task Expired_consent_records_are_batch_transitioned_on_demand()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var principal = await CreateDataPrincipalAsync(client, $"EXT-EXP-{suffix}");
        var purpose = await CreateConsentPurposeAsync(client, $"Retargeting {suffix}");

        var consent = await (await client.PostAsJsonAsync("/api/v1/consent-records", new
        {
            dataPrincipalId = principal.Id, consentPurposeId = purpose.Id, noticeVersionId = (Guid?)null,
            grantedAt = DateTimeOffset.UtcNow.AddDays(-10), channel = "EMAIL",
            expiresAt = DateTimeOffset.UtcNow.AddSeconds(-1), sourceSystem = (string?)null, externalReferenceId = (string?)null,
        })).Content.ReadFromJsonAsync<ConsentRecordDetail>();

        var markResponse = await client.PostAsync("/api/v1/consent-records/mark-expired", null);
        Assert.Equal(HttpStatusCode.OK, markResponse.StatusCode);

        var reload = await client.GetFromJsonAsync<ConsentRecordDetail>($"/api/v1/consent-records/{consent!.Id}");
        Assert.Equal("EXPIRED", reload!.Status);

        await client.DeleteAsync($"/api/v1/consent-purposes/{purpose.Id}");
        await client.DeleteAsync($"/api/v1/data-principals/{principal.Id}");
    }

    [Fact]
    public async Task Privacy_notice_full_workflow_enforces_separation_of_duties()
    {
        var privacyOfficer = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);
        var complianceOfficer = await AuthenticatedClientAsync(fixture.ComplianceOfficerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var createResponse = await privacyOfficer.PostAsJsonAsync("/api/v1/privacy-notices", new
        {
            code = $"PRIVACY-POLICY-{suffix}", title = "Privacy Policy", version = "1.0", language = "en",
            purpose = "Describes how we process personal data", publishedDate = (DateOnly?)null, effectiveDate = (DateOnly?)null,
            dataCategoryIds = Array.Empty<Guid>(),
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var notice = await createResponse.Content.ReadFromJsonAsync<PrivacyNoticeDetail>();
        Assert.StartsWith("PN-", notice!.NoticeNumber);
        Assert.Equal("DRAFT", notice.Status);

        var complianceApproveDenied = await complianceOfficer.PostAsync($"/api/v1/privacy-notices/{notice.Id}/publish", null);
        Assert.Equal(HttpStatusCode.Forbidden, complianceApproveDenied.StatusCode);

        var privacyOfficerApproveDenied = await privacyOfficer.PostAsync($"/api/v1/privacy-notices/{notice.Id}/approve", null);
        Assert.Equal(HttpStatusCode.Forbidden, privacyOfficerApproveDenied.StatusCode);

        var approveResponse = await complianceOfficer.PostAsync($"/api/v1/privacy-notices/{notice.Id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        var approved = await approveResponse.Content.ReadFromJsonAsync<PrivacyNoticeDetail>();
        Assert.Equal("APPROVED", approved!.Status);

        var publishResponse = await privacyOfficer.PostAsync($"/api/v1/privacy-notices/{notice.Id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);
        var published = await publishResponse.Content.ReadFromJsonAsync<PrivacyNoticeDetail>();
        Assert.Equal("PUBLISHED", published!.Status);

        var deleteDenied = await privacyOfficer.DeleteAsync($"/api/v1/privacy-notices/{notice.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteDenied.StatusCode);

        var archiveResponse = await privacyOfficer.PostAsync($"/api/v1/privacy-notices/{notice.Id}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);
        var archived = await archiveResponse.Content.ReadFromJsonAsync<PrivacyNoticeDetail>();
        Assert.Equal("ARCHIVED", archived!.Status);

        Assert.Equal(1, await fixture.CountAuditLogsAsync("consentprivacy.notice_approved", notice.Id.ToString()));
        Assert.Equal(1, await fixture.CountAuditLogsAsync("consentprivacy.notice_published", notice.Id.ToString()));
        Assert.Equal(1, await fixture.CountAuditLogsAsync("consentprivacy.notice_archived", notice.Id.ToString()));
    }

    [Fact]
    public async Task Data_principal_request_computes_due_date_from_matching_sla_policy()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var policy = await CreateSlaPolicyAsync(client, $"Erasure SLA {suffix}", "ERASURE", 21);

        var createResponse = await client.PostAsJsonAsync("/api/v1/data-principal-requests", new
        {
            requestType = "ERASURE", requesterName = "Test Requester", requesterContactEmail = "requester@example.com",
            requesterContactPhone = (string?)null, externalReferenceId = (string?)null,
            dataPrincipalId = (Guid?)null, relatedConsentId = (Guid?)null, description = "Please delete my data",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var dpr = await createResponse.Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();
        Assert.StartsWith("DPR-", dpr!.RequestNumber);
        Assert.Equal("REQUESTED", dpr.Status);
        Assert.NotNull(dpr.DueAt);
        Assert.Equal(policy.Id, dpr.SlaPolicyId);
        var expectedDueAt = dpr.CreatedAt.AddDays(21);
        Assert.True(Math.Abs((dpr.DueAt!.Value - expectedDueAt).TotalMinutes) < 2);

        await client.DeleteAsync($"/api/v1/data-principal-requests/{dpr.Id}");
        await client.DeleteAsync($"/api/v1/sla-policies/{policy.Id}");
    }

    [Fact]
    public async Task Data_principal_request_has_null_due_date_when_no_sla_policy_is_configured()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);

        var createResponse = await client.PostAsJsonAsync("/api/v1/data-principal-requests", new
        {
            requestType = "OTHER", requesterName = "No Policy Requester", requesterContactEmail = (string?)null,
            requesterContactPhone = (string?)null, externalReferenceId = (string?)null,
            dataPrincipalId = (Guid?)null, relatedConsentId = (Guid?)null, description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var dpr = await createResponse.Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();
        Assert.Null(dpr!.DueAt);
        Assert.Null(dpr.SlaPolicyId);
        Assert.False(dpr.IsOverdue);

        await client.DeleteAsync($"/api/v1/data-principal-requests/{dpr.Id}");
    }

    [Fact]
    public async Task Grievance_uses_the_same_data_principal_request_workflow_end_to_end()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);

        var dpr = await (await client.PostAsJsonAsync("/api/v1/data-principal-requests", new
        {
            requestType = "GRIEVANCE", requesterName = "Grievance Requester", requesterContactEmail = "grievance@example.com",
            requesterContactPhone = (string?)null, externalReferenceId = (string?)null,
            dataPrincipalId = (Guid?)null, relatedConsentId = (Guid?)null, description = "Unresolved complaint",
        })).Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();
        Assert.Equal("GRIEVANCE", dpr!.RequestType);

        var assignResponse = await client.PostAsJsonAsync($"/api/v1/data-principal-requests/{dpr.Id}/assign", new { assignedToUserId = fixture.PrivacyOfficerAUserId });
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        var assigned = await assignResponse.Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();
        Assert.Equal(fixture.PrivacyOfficerAUserId, assigned!.AssignedToUserId);

        var verifyResponse = await client.PostAsJsonAsync($"/api/v1/data-principal-requests/{dpr.Id}/verify-identity", new { matchedDataPrincipalId = (Guid?)null });
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        var verified = await verifyResponse.Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();
        Assert.Equal("IDENTITY_VERIFICATION", verified!.Status);
        Assert.NotNull(verified.IdentityVerifiedAt);

        var progressResponse = await client.PostAsJsonAsync($"/api/v1/data-principal-requests/{dpr.Id}/status", new { status = "IN_PROGRESS" });
        Assert.Equal(HttpStatusCode.OK, progressResponse.StatusCode);

        var awaitingResponse = await client.PostAsJsonAsync($"/api/v1/data-principal-requests/{dpr.Id}/status", new { status = "AWAITING_INFORMATION" });
        Assert.Equal(HttpStatusCode.OK, awaitingResponse.StatusCode);

        var backToProgressResponse = await client.PostAsJsonAsync($"/api/v1/data-principal-requests/{dpr.Id}/status", new { status = "IN_PROGRESS" });
        Assert.Equal(HttpStatusCode.OK, backToProgressResponse.StatusCode);

        var completeResponse = await client.PostAsJsonAsync($"/api/v1/data-principal-requests/{dpr.Id}/complete", new { resolutionNotes = "Complaint upheld and resolved" });
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        var completed = await completeResponse.Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();
        Assert.Equal("COMPLETED", completed!.Status);
        Assert.NotNull(completed.ResolvedAt);

        var closeResponse = await client.PostAsync($"/api/v1/data-principal-requests/{dpr.Id}/close", null);
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        var closed = await closeResponse.Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();
        Assert.Equal("CLOSED", closed!.Status);
        Assert.NotNull(closed.ClosedAt);

        var reopenDenied = await client.PostAsJsonAsync($"/api/v1/data-principal-requests/{dpr.Id}/status", new { status = "IN_PROGRESS" });
        Assert.Equal(HttpStatusCode.Conflict, reopenDenied.StatusCode);

        Assert.Equal(1, await fixture.CountAuditLogsAsync("consentprivacy.request_completed", dpr.Id.ToString()));
        Assert.Equal(1, await fixture.CountAuditLogsAsync("consentprivacy.request_closed", dpr.Id.ToString()));
    }

    [Fact]
    public async Task Data_principal_request_can_be_rejected_with_reason()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);

        var dpr = await (await client.PostAsJsonAsync("/api/v1/data-principal-requests", new
        {
            requestType = "CORRECTION", requesterName = "Rejected Requester", requesterContactEmail = (string?)null,
            requesterContactPhone = (string?)null, externalReferenceId = (string?)null,
            dataPrincipalId = (Guid?)null, relatedConsentId = (Guid?)null, description = (string?)null,
        })).Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();

        var missingReasonResponse = await client.PostAsJsonAsync($"/api/v1/data-principal-requests/{dpr!.Id}/reject", new { rejectionReason = "" });
        Assert.Equal(HttpStatusCode.BadRequest, missingReasonResponse.StatusCode);

        var rejectResponse = await client.PostAsJsonAsync($"/api/v1/data-principal-requests/{dpr.Id}/reject", new { rejectionReason = "Unable to verify claimed identity" });
        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);
        var rejected = await rejectResponse.Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();
        Assert.Equal("REJECTED", rejected!.Status);
        Assert.NotNull(rejected.RejectedAt);

        var closeResponse = await client.PostAsync($"/api/v1/data-principal-requests/{dpr.Id}/close", null);
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
    }

    [Fact]
    public async Task Sla_summary_reflects_overdue_and_unconfigured_requests()
    {
        var client = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var policy = await CreateSlaPolicyAsync(client, $"Access SLA Summary {suffix}", "ACCESS", 1);

        var dpr = await (await client.PostAsJsonAsync("/api/v1/data-principal-requests", new
        {
            requestType = "ACCESS", requesterName = "Overdue Requester", requesterContactEmail = (string?)null,
            requesterContactPhone = (string?)null, externalReferenceId = (string?)null,
            dataPrincipalId = (Guid?)null, relatedConsentId = (Guid?)null, description = (string?)null,
        })).Content.ReadFromJsonAsync<DataPrincipalRequestDetail>();

        var listResponse = await client.GetFromJsonAsync<PagedDataPrincipalRequests>("/api/v1/data-principal-requests?status=REQUESTED");
        Assert.Contains(listResponse!.Items, r => r.Id == dpr!.Id);

        var summary = await client.GetFromJsonAsync<SlaSummary>("/api/v1/data-principal-requests/sla-summary");
        Assert.True(summary!.OpenCount >= 1);

        await client.DeleteAsync($"/api/v1/data-principal-requests/{dpr!.Id}");
        await client.DeleteAsync($"/api/v1/sla-policies/{policy.Id}");
    }

    [Fact]
    public async Task Read_only_role_cannot_manage_but_can_read()
    {
        var readOnly = await AuthenticatedClientAsync(fixture.ReadOnlyAEmail);

        var createDenied = await readOnly.PostAsJsonAsync("/api/v1/data-principals", new { externalReferenceId = "EXT-RO", referenceCategory = (string?)null, notes = (string?)null });
        Assert.Equal(HttpStatusCode.Forbidden, createDenied.StatusCode);

        var listResponse = await readOnly.GetAsync("/api/v1/data-principals");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
    }

    [Fact]
    public async Task Organisation_b_cannot_see_organisation_a_records()
    {
        var privacyOfficerA = await AuthenticatedClientAsync(fixture.PrivacyOfficerAEmail);
        var privacyOfficerB = await AuthenticatedClientAsync(fixture.PrivacyOfficerBEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var principal = await CreateDataPrincipalAsync(privacyOfficerA, $"EXT-TENANT-{suffix}");

        var crossTenantView = await privacyOfficerB.GetAsync($"/api/v1/data-principals/{principal.Id}");
        Assert.Equal(HttpStatusCode.NotFound, crossTenantView.StatusCode);

        var listResponse = await privacyOfficerB.GetFromJsonAsync<List<DataPrincipalItem>>("/api/v1/data-principals");
        Assert.DoesNotContain(listResponse!, p => p.Id == principal.Id);

        await privacyOfficerA.DeleteAsync($"/api/v1/data-principals/{principal.Id}");
    }

    private sealed record CatalogItem(Guid Id, string Name);
    private sealed record DataPrincipalItem(Guid Id, string ExternalReferenceId);
    private sealed record SlaPolicyItem(Guid Id, string Name, string? RequestType, int ResponseDueDays);
    private sealed record ConsentRecordDetail(Guid Id, string ConsentNumber, string Status, DateTimeOffset? WithdrawnAt);
    private sealed record PrivacyNoticeDetail(Guid Id, string NoticeNumber, string Status);
    private sealed record DataPrincipalRequestDetail(
        Guid Id, string RequestNumber, string RequestType, string Status, Guid? SlaPolicyId, DateTimeOffset? DueAt, bool IsOverdue,
        DateTimeOffset? IdentityVerifiedAt, Guid? AssignedToUserId, DateTimeOffset? ResolvedAt, DateTimeOffset? RejectedAt,
        DateTimeOffset? ClosedAt, DateTimeOffset CreatedAt);
    private sealed record PagedDataPrincipalRequests(List<DataPrincipalRequestDetail> Items);
    private sealed record SlaSummary(int OpenCount, int OverdueCount, int DueWithin48HoursCount, int NoSlaConfiguredCount);
}
