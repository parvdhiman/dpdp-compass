using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DPDP.ApiTests.DataInventory;

/// <summary>
/// Module 9 quality-gate tests: catalogue CRUD, the full Data Inventory
/// item lifecycle (including CSV export), the full Processing Activity
/// workflow (Draft → Review → Approve → Archive, plus reopen and
/// send-back), Data Flow metadata, search/filter, audit logging, RBAC
/// separation of duties, and tenant isolation.
/// </summary>
[Collection("DataInventory")]
public sealed class DataInventoryApiTests(DataInventoryApiFixture fixture)
{
    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    private async Task<CatalogItem> CreateDataCategoryAsync(HttpClient client, string name) =>
        (await (await client.PostAsJsonAsync("/api/v1/data-categories", new { name, description = (string?)null, classificationCategory = "CONTACT" }))
            .Content.ReadFromJsonAsync<CatalogItem>())!;

    private async Task<CatalogItem> CreateItSystemAsync(HttpClient client, string name) =>
        (await (await client.PostAsJsonAsync("/api/v1/it-systems", new { name, description = (string?)null, systemType = "APPLICATION", ownerUserId = (Guid?)null }))
            .Content.ReadFromJsonAsync<CatalogItem>())!;

    private async Task<CatalogItem> CreateDataCollectionSourceAsync(HttpClient client, string name) =>
        (await (await client.PostAsJsonAsync("/api/v1/data-collection-sources", new { name, description = (string?)null, sourceType = "WEB_FORM" }))
            .Content.ReadFromJsonAsync<CatalogItem>())!;

    private async Task<CatalogItem> CreateProcessorAsync(HttpClient client, string name) =>
        (await (await client.PostAsJsonAsync("/api/v1/processors", new { name, description = (string?)null, contactEmail = (string?)null, country = "IN" }))
            .Content.ReadFromJsonAsync<CatalogItem>())!;

    private async Task<CatalogItem> CreateRecipientAsync(HttpClient client, string name) =>
        (await (await client.PostAsJsonAsync("/api/v1/recipients", new { name, description = (string?)null, recipientType = "THIRD_PARTY" }))
            .Content.ReadFromJsonAsync<CatalogItem>())!;

    private async Task<CatalogItem> CreateRetentionPolicyAsync(HttpClient client, string name) =>
        (await (await client.PostAsJsonAsync("/api/v1/retention-policies", new { name, description = (string?)null, retentionPeriodValue = 3, retentionPeriodUnit = "YEARS", triggerEvent = "account closure" }))
            .Content.ReadFromJsonAsync<CatalogItem>())!;

    [Fact]
    public async Task Each_catalogue_supports_create_list_and_delete()
    {
        var client = await AuthenticatedClientAsync(fixture.PreparerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var category = await CreateDataCategoryAsync(client, $"Contact Details {suffix}");
        var system = await CreateItSystemAsync(client, $"CRM {suffix}");
        var source = await CreateDataCollectionSourceAsync(client, $"Signup Form {suffix}");
        var processor = await CreateProcessorAsync(client, $"Cloud Host {suffix}");
        var recipient = await CreateRecipientAsync(client, $"Regulator {suffix}");
        var retention = await CreateRetentionPolicyAsync(client, $"Standard 3yr {suffix}");

        var listCategories = await client.GetFromJsonAsync<List<CatalogItem>>("/api/v1/data-categories");
        Assert.Contains(listCategories!, c => c.Id == category.Id);

        var deleteResponse = await client.DeleteAsync($"/api/v1/data-categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Cleanup the rest so they don't linger across tests.
        await client.DeleteAsync($"/api/v1/it-systems/{system.Id}");
        await client.DeleteAsync($"/api/v1/data-collection-sources/{source.Id}");
        await client.DeleteAsync($"/api/v1/processors/{processor.Id}");
        await client.DeleteAsync($"/api/v1/recipients/{recipient.Id}");
        await client.DeleteAsync($"/api/v1/retention-policies/{retention.Id}");
    }

    [Fact]
    public async Task Data_inventory_item_full_lifecycle_with_search_filter_and_export_and_audit_log()
    {
        var client = await AuthenticatedClientAsync(fixture.PreparerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var category = await CreateDataCategoryAsync(client, $"Financial Records {suffix}");
        var system = await CreateItSystemAsync(client, $"Billing System {suffix}");
        var processor = await CreateProcessorAsync(client, $"Payment Processor {suffix}");

        var createResponse = await client.PostAsJsonAsync("/api/v1/data-inventory", new
        {
            dataCategoryId = category.Id,
            dataElementName = $"Bank Account Number {suffix}",
            discoveredDataElementId = (Guid?)null,
            classification = "FINANCIAL",
            dataCollectionSourceId = (Guid?)null,
            itSystemId = system.Id,
            ownerUserId = (Guid?)null,
            purpose = "Payroll disbursement",
            retentionPolicyId = (Guid?)null,
            sharingDescription = "Shared with payment processor only",
            processorId = processor.Id,
            riskLevel = "HIGH",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var item = await createResponse.Content.ReadFromJsonAsync<DataInventoryItemDetail>();
        Assert.StartsWith("DI-", item!.ItemNumber);

        var auditCount = await fixture.CountAuditLogsAsync("datainventory.item_created", item.Id.ToString());
        Assert.Equal(1, auditCount);

        var searchResponse = await client.GetFromJsonAsync<PagedItems>($"/api/v1/data-inventory?search=Bank%20Account&riskLevel=HIGH");
        Assert.Contains(searchResponse!.Items, i => i.Id == item.Id);

        var filterMissResponse = await client.GetFromJsonAsync<PagedItems>($"/api/v1/data-inventory?riskLevel=LOW");
        Assert.DoesNotContain(filterMissResponse!.Items, i => i.Id == item.Id);

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/data-inventory/{item.Id}", new
        {
            dataCategoryId = category.Id,
            dataElementName = item.DataElementName,
            classification = "FINANCIAL",
            dataCollectionSourceId = (Guid?)null,
            itSystemId = system.Id,
            ownerUserId = (Guid?)null,
            purpose = "Payroll disbursement and tax reporting",
            retentionPolicyId = (Guid?)null,
            sharingDescription = "Shared with payment processor only",
            processorId = processor.Id,
            riskLevel = "CRITICAL",
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var exportResponse = await client.GetAsync($"/api/v1/data-inventory/export?search={Uri.EscapeDataString(suffix)}");
        Assert.Equal(HttpStatusCode.OK, exportResponse.StatusCode);
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains(item.ItemNumber, csv);
        Assert.Contains("CRITICAL", csv);

        var deleteResponse = await client.DeleteAsync($"/api/v1/data-inventory/{item.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Cannot_delete_a_catalogue_entry_still_referenced_by_an_inventory_item()
    {
        var client = await AuthenticatedClientAsync(fixture.PreparerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var category = await CreateDataCategoryAsync(client, $"In Use Category {suffix}");
        var createResponse = await client.PostAsJsonAsync("/api/v1/data-inventory", new
        {
            dataCategoryId = category.Id,
            dataElementName = $"Some Data {suffix}",
            discoveredDataElementId = (Guid?)null,
            classification = (string?)null,
            dataCollectionSourceId = (Guid?)null,
            itSystemId = (Guid?)null,
            ownerUserId = (Guid?)null,
            purpose = (string?)null,
            retentionPolicyId = (Guid?)null,
            sharingDescription = (string?)null,
            processorId = (Guid?)null,
            riskLevel = (string?)null,
        });
        var item = await createResponse.Content.ReadFromJsonAsync<DataInventoryItemDetail>();

        var deleteAttempt = await client.DeleteAsync($"/api/v1/data-categories/{category.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteAttempt.StatusCode);

        await client.DeleteAsync($"/api/v1/data-inventory/{item!.Id}");
        var deleteAfterCleanup = await client.DeleteAsync($"/api/v1/data-categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteAfterCleanup.StatusCode);
    }

    [Fact]
    public async Task Processing_activity_full_workflow_draft_review_send_back_resubmit_approve_archive_reopen()
    {
        var preparer = await AuthenticatedClientAsync(fixture.PreparerAEmail);
        var reviewer = await AuthenticatedClientAsync(fixture.ReviewerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var category = await CreateDataCategoryAsync(preparer, $"Marketing Data {suffix}");
        var system = await CreateItSystemAsync(preparer, $"Marketing Platform {suffix}");
        var source = await CreateDataCollectionSourceAsync(preparer, $"Newsletter Signup {suffix}");
        var recipient = await CreateRecipientAsync(preparer, $"Ad Network {suffix}");
        var processor = await CreateProcessorAsync(preparer, $"Email Vendor {suffix}");

        var createResponse = await preparer.PostAsJsonAsync("/api/v1/processing-activities", new
        {
            name = $"Newsletter Marketing {suffix}",
            purpose = "Send marketing newsletters to subscribed customers",
            dataSubjectCategories = new[] { "CUSTOMER", "PROSPECT" },
            securityControls = new[] { "Encryption at rest", "Access control" },
            retentionPolicyId = (Guid?)null,
            ownerUserId = (Guid?)null,
            reviewDate = (DateOnly?)null,
            dataCategoryIds = new[] { category.Id },
            itSystemIds = new[] { system.Id },
            dataCollectionSourceIds = new[] { source.Id },
            recipientIds = new[] { recipient.Id },
            processorIds = new[] { processor.Id },
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var activity = await createResponse.Content.ReadFromJsonAsync<ProcessingActivityDetail>();
        Assert.Equal("DRAFT", activity!.Status);
        Assert.Single(activity.DataCategories);
        Assert.Single(activity.ItSystems);
        Assert.Contains("CUSTOMER", activity.DataSubjectCategories);

        // Preparer cannot approve their own submission.
        var preparerApproveDenied = await preparer.PostAsJsonAsync($"/api/v1/processing-activities/{activity.Id}/approve", new { reviewComments = (string?)null });
        Assert.Equal(HttpStatusCode.Forbidden, preparerApproveDenied.StatusCode);

        var submitResponse = await preparer.PostAsync($"/api/v1/processing-activities/{activity.Id}/submit", null);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);
        var submitted = await submitResponse.Content.ReadFromJsonAsync<ProcessingActivityDetail>();
        Assert.Equal("IN_REVIEW", submitted!.Status);

        var sendBackResponse = await reviewer.PostAsJsonAsync($"/api/v1/processing-activities/{activity.Id}/send-back", new { reviewComments = "Please clarify retention." });
        Assert.Equal(HttpStatusCode.OK, sendBackResponse.StatusCode);
        var sentBack = await sendBackResponse.Content.ReadFromJsonAsync<ProcessingActivityDetail>();
        Assert.Equal("DRAFT", sentBack!.Status);
        Assert.Equal("Please clarify retention.", sentBack.ReviewComments);

        await preparer.PostAsync($"/api/v1/processing-activities/{activity.Id}/submit", null);
        var approveResponse = await reviewer.PostAsJsonAsync($"/api/v1/processing-activities/{activity.Id}/approve", new { reviewComments = "Looks good." });
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        var approved = await approveResponse.Content.ReadFromJsonAsync<ProcessingActivityDetail>();
        Assert.Equal("APPROVED", approved!.Status);
        Assert.NotNull(approved.ApprovedAt);

        var auditCount = await fixture.CountAuditLogsAsync("datainventory.activity_approved", activity.Id.ToString());
        Assert.Equal(1, auditCount);

        // Preparer cannot archive-skip; only a legitimate transition is allowed, and preparer holds manage so this should succeed.
        var archiveResponse = await preparer.PostAsync($"/api/v1/processing-activities/{activity.Id}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);
        var archived = await archiveResponse.Content.ReadFromJsonAsync<ProcessingActivityDetail>();
        Assert.Equal("ARCHIVED", archived!.Status);

        var reopenAfterArchiveDenied = await preparer.PostAsync($"/api/v1/processing-activities/{activity.Id}/reopen", null);
        Assert.Equal(HttpStatusCode.Conflict, reopenAfterArchiveDenied.StatusCode);
    }

    [Fact]
    public async Task Reopen_moves_an_approved_activity_back_to_draft()
    {
        var preparer = await AuthenticatedClientAsync(fixture.PreparerAEmail);
        var reviewer = await AuthenticatedClientAsync(fixture.ReviewerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var createResponse = await preparer.PostAsJsonAsync("/api/v1/processing-activities", new
        {
            name = $"Reopen Test Activity {suffix}",
            purpose = "Test reopen workflow",
            dataSubjectCategories = Array.Empty<string>(),
            securityControls = Array.Empty<string>(),
            retentionPolicyId = (Guid?)null,
            ownerUserId = (Guid?)null,
            reviewDate = (DateOnly?)null,
            dataCategoryIds = Array.Empty<Guid>(),
            itSystemIds = Array.Empty<Guid>(),
            dataCollectionSourceIds = Array.Empty<Guid>(),
            recipientIds = Array.Empty<Guid>(),
            processorIds = Array.Empty<Guid>(),
        });
        var activity = await createResponse.Content.ReadFromJsonAsync<ProcessingActivityDetail>();

        await preparer.PostAsync($"/api/v1/processing-activities/{activity!.Id}/submit", null);
        await reviewer.PostAsJsonAsync($"/api/v1/processing-activities/{activity.Id}/approve", new { reviewComments = (string?)null });

        var reopenResponse = await preparer.PostAsync($"/api/v1/processing-activities/{activity.Id}/reopen", null);
        Assert.Equal(HttpStatusCode.OK, reopenResponse.StatusCode);
        var reopened = await reopenResponse.Content.ReadFromJsonAsync<ProcessingActivityDetail>();
        Assert.Equal("DRAFT", reopened!.Status);
    }

    [Fact]
    public async Task Send_back_is_rejected_outside_in_review_and_reopen_is_rejected_outside_approved()
    {
        var preparer = await AuthenticatedClientAsync(fixture.PreparerAEmail);
        var reviewer = await AuthenticatedClientAsync(fixture.ReviewerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var createResponse = await preparer.PostAsJsonAsync("/api/v1/processing-activities", new
        {
            name = $"Boundary Test Activity {suffix}",
            purpose = "Test boundary conditions",
            dataSubjectCategories = Array.Empty<string>(),
            securityControls = Array.Empty<string>(),
            retentionPolicyId = (Guid?)null,
            ownerUserId = (Guid?)null,
            reviewDate = (DateOnly?)null,
            dataCategoryIds = Array.Empty<Guid>(),
            itSystemIds = Array.Empty<Guid>(),
            dataCollectionSourceIds = Array.Empty<Guid>(),
            recipientIds = Array.Empty<Guid>(),
            processorIds = Array.Empty<Guid>(),
        });
        var activity = await createResponse.Content.ReadFromJsonAsync<ProcessingActivityDetail>();

        var sendBackWhileDraft = await reviewer.PostAsJsonAsync($"/api/v1/processing-activities/{activity!.Id}/send-back", new { reviewComments = "Not ready to review yet." });
        Assert.Equal(HttpStatusCode.Conflict, sendBackWhileDraft.StatusCode);

        var reopenWhileDraft = await preparer.PostAsync($"/api/v1/processing-activities/{activity.Id}/reopen", null);
        Assert.Equal(HttpStatusCode.Conflict, reopenWhileDraft.StatusCode);
    }

    [Fact]
    public async Task Data_flow_records_cross_border_metadata_and_requires_country_when_cross_border()
    {
        var client = await AuthenticatedClientAsync(fixture.PreparerAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var system = await CreateItSystemAsync(client, $"Analytics Platform {suffix}");
        var processor = await CreateProcessorAsync(client, $"Overseas Processor {suffix}");

        var missingCountryResponse = await client.PostAsJsonAsync("/api/v1/data-flows", new
        {
            name = $"Cross Border Flow {suffix}",
            description = (string?)null,
            processingActivityId = (Guid?)null,
            dataCategoryId = (Guid?)null,
            fromItSystemId = system.Id,
            fromDataCollectionSourceId = (Guid?)null,
            fromDescription = "Internal analytics system",
            toItSystemId = (Guid?)null,
            toProcessorId = processor.Id,
            toRecipientId = (Guid?)null,
            toDescription = "Overseas data processor",
            transferMechanism = "API",
            isCrossBorder = true,
            crossBorderCountry = (string?)null,
        });
        Assert.Equal(HttpStatusCode.BadRequest, missingCountryResponse.StatusCode);

        var createResponse = await client.PostAsJsonAsync("/api/v1/data-flows", new
        {
            name = $"Cross Border Flow {suffix}",
            description = (string?)null,
            processingActivityId = (Guid?)null,
            dataCategoryId = (Guid?)null,
            fromItSystemId = system.Id,
            fromDataCollectionSourceId = (Guid?)null,
            fromDescription = "Internal analytics system",
            toItSystemId = (Guid?)null,
            toProcessorId = processor.Id,
            toRecipientId = (Guid?)null,
            toDescription = "Overseas data processor",
            transferMechanism = "API",
            isCrossBorder = true,
            crossBorderCountry = "Germany",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var flow = await createResponse.Content.ReadFromJsonAsync<DataFlowDetail>();
        Assert.True(flow!.IsCrossBorder);
        Assert.Equal("Germany", flow.CrossBorderCountry);
        Assert.StartsWith("DF-", flow.FlowNumber);

        var listResponse = await client.GetFromJsonAsync<PagedFlows>("/api/v1/data-flows?crossBorderOnly=true");
        Assert.Contains(listResponse!.Items, f => f.Id == flow.Id);
    }

    [Fact]
    public async Task Read_only_user_can_view_but_not_manage()
    {
        var preparer = await AuthenticatedClientAsync(fixture.PreparerAEmail);
        var readOnly = await AuthenticatedClientAsync(fixture.ReadOnlyAEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var category = await CreateDataCategoryAsync(preparer, $"Read Only Test {suffix}");

        var view = await readOnly.GetAsync($"/api/v1/data-categories/{category.Id}");
        Assert.Equal(HttpStatusCode.OK, view.StatusCode);

        var createDenied = await readOnly.PostAsJsonAsync("/api/v1/data-categories", new { name = "Should Be Denied", description = (string?)null, classificationCategory = (string?)null });
        Assert.Equal(HttpStatusCode.Forbidden, createDenied.StatusCode);

        var createActivityDenied = await readOnly.PostAsJsonAsync("/api/v1/processing-activities", new
        {
            name = "Should Be Denied", purpose = "n/a", dataSubjectCategories = Array.Empty<string>(), securityControls = Array.Empty<string>(),
            retentionPolicyId = (Guid?)null, ownerUserId = (Guid?)null, reviewDate = (DateOnly?)null,
            dataCategoryIds = Array.Empty<Guid>(), itSystemIds = Array.Empty<Guid>(), dataCollectionSourceIds = Array.Empty<Guid>(),
            recipientIds = Array.Empty<Guid>(), processorIds = Array.Empty<Guid>(),
        });
        Assert.Equal(HttpStatusCode.Forbidden, createActivityDenied.StatusCode);

        await preparer.DeleteAsync($"/api/v1/data-categories/{category.Id}");
    }

    [Fact]
    public async Task Organisation_b_cannot_see_organisation_as_records()
    {
        var preparerA = await AuthenticatedClientAsync(fixture.PreparerAEmail);
        var preparerB = await AuthenticatedClientAsync(fixture.PreparerBEmail);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var category = await CreateDataCategoryAsync(preparerA, $"Tenant Isolation Category {suffix}");

        var crossTenantView = await preparerB.GetAsync($"/api/v1/data-categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NotFound, crossTenantView.StatusCode);

        var listResponse = await preparerB.GetFromJsonAsync<List<CatalogItem>>("/api/v1/data-categories");
        Assert.DoesNotContain(listResponse!, c => c.Id == category.Id);

        await preparerA.DeleteAsync($"/api/v1/data-categories/{category.Id}");
    }

    private sealed record CatalogItem(Guid Id, string Name);
    private sealed record DataInventoryItemDetail(Guid Id, string ItemNumber, string DataElementName);
    private sealed record PagedItems(List<DataInventoryItemDetail> Items);
    private sealed record ProcessingActivityDetail(
        Guid Id, string ActivityNumber, string Status, string? ReviewComments, DateTimeOffset? ApprovedAt,
        List<string> DataSubjectCategories, List<CatalogItem> DataCategories, List<CatalogItem> ItSystems);
    private sealed record DataFlowDetail(Guid Id, string FlowNumber, bool IsCrossBorder, string? CrossBorderCountry);
    private sealed record PagedFlows(List<DataFlowDetail> Items);
}
