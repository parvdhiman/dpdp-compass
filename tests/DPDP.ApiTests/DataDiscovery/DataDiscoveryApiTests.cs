using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DPDP.ApiTests.DataDiscovery;

/// <summary>
/// Module 8 quality-gate tests: the full connector→metadata→classification
/// pipeline exercised end-to-end against a real local PostgreSQL schema
/// (scoped via DataSource.SchemaFilter to the fixture's own throwaway
/// schema, so the app's own tables are never touched or scanned), plus the
/// FileSystem connector against a real temp directory, authorization, and
/// tenant isolation. MySqlDiscoveryConnector/SqlServerDiscoveryConnector
/// have no live server in this environment and are therefore not covered
/// here — see the completion report.
/// </summary>
[Collection("DataDiscovery")]
public sealed class DataDiscoveryApiTests(DataDiscoveryApiFixture fixture)
{
    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    /// <summary>A fresh, empty, isolated directory — never the shared system temp root, whose ambient contents would make scan results non-deterministic and slow.</summary>
    private static string CreateEmptyTempDir() => Directory.CreateTempSubdirectory("dpdp-discovery-test-").FullName;

    private async Task<DiscoveryJobDto> WaitForJobToFinishAsync(HttpClient client, Guid jobId, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(30));
        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync($"/api/v1/discovery-jobs/{jobId}");
            response.EnsureSuccessStatusCode();
            var job = await response.Content.ReadFromJsonAsync<DiscoveryJobDto>();
            if (job!.Status is "COMPLETED" or "FAILED" or "CANCELLED")
            {
                return job;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException($"Discovery job {jobId} did not finish within the timeout.");
    }

    [Fact]
    public async Task Full_pipeline_discovers_the_real_postgres_schema_masks_samples_and_classifies_columns()
    {
        var operatorClient = await AuthenticatedClientAsync(fixture.OperatorAEmail);
        var reviewerClient = await AuthenticatedClientAsync(fixture.ReviewerAEmail);
        var conn = fixture.AppConnectionStringBuilder;

        var createResponse = await operatorClient.PostAsJsonAsync("/api/v1/data-sources", new
        {
            name = "Postgres E2E Test Source",
            description = (string?)null,
            sourceType = "POSTGRESQL",
            host = conn.Host,
            port = conn.Port,
            databaseName = conn.Database,
            username = conn.Username,
            password = conn.Password,
            rootPath = (string?)null,
            schemaFilter = fixture.TestSchemaName,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var dataSource = await createResponse.Content.ReadFromJsonAsync<DataSourceDetail>();
        Assert.Equal("POSTGRESQL", dataSource!.SourceType);
        var body = await createResponse.Content.ReadAsStringAsync();
        Assert.NotNull(conn.Password);
        Assert.DoesNotContain(conn.Password!, body);

        var testConnectionResponse = await operatorClient.PostAsync($"/api/v1/data-sources/{dataSource.Id}/test-connection", null);
        Assert.Equal(HttpStatusCode.OK, testConnectionResponse.StatusCode);
        var testResult = await testConnectionResponse.Content.ReadFromJsonAsync<TestConnectionResult>();
        Assert.True(testResult!.Succeeded, testResult.ErrorMessage);

        var startJobResponse = await operatorClient.PostAsync($"/api/v1/data-sources/{dataSource.Id}/discovery-jobs", null);
        Assert.Equal(HttpStatusCode.Created, startJobResponse.StatusCode);
        var startedJob = await startJobResponse.Content.ReadFromJsonAsync<DiscoveryJobDto>();
        Assert.Equal("PENDING", startedJob!.Status);

        var finishedJob = await WaitForJobToFinishAsync(operatorClient, startedJob.Id);
        Assert.Equal("COMPLETED", finishedJob.Status);
        Assert.True(finishedJob.AssetsDiscoveredCount >= 1, finishedJob.ErrorMessage);
        Assert.True(finishedJob.ElementsDiscoveredCount >= 4);

        var assetsResponse = await operatorClient.GetAsync($"/api/v1/data-assets?dataSourceId={dataSource.Id}");
        var assets = await assetsResponse.Content.ReadFromJsonAsync<PagedAssets>();
        var customersAsset = assets!.Items.Single(a => a.AssetName == "customers");
        Assert.Equal("TABLE", customersAsset.AssetType);
        Assert.True(customersAsset.EstimatedRowCount is null or >= 0);

        var assetDetailResponse = await operatorClient.GetAsync($"/api/v1/data-assets/{customersAsset.Id}");
        var assetDetail = await assetDetailResponse.Content.ReadFromJsonAsync<DataAssetDetail>();
        Assert.Contains("ix_customers_email", assetDetail!.Indexes);

        var emailElement = assetDetail.Elements.Single(e => e.ColumnName == "customer_email");
        Assert.Equal("CONTACT", emailElement.ClassificationCategory);
        Assert.True(emailElement.ClassificationConfidence > 0);
        Assert.Equal("SYSTEM", emailElement.ClassificationSource);
        Assert.NotNull(emailElement.SampleMaskedValue);
        Assert.DoesNotContain("jane.doe@example.com", emailElement.SampleMaskedValue);
        Assert.Contains('*', emailElement.SampleMaskedValue!);

        var nationalIdElement = assetDetail.Elements.Single(e => e.ColumnName == "national_id");
        Assert.Equal("IDENTIFIER", nationalIdElement.ClassificationCategory);
        Assert.Equal("98******10", nationalIdElement.SampleMaskedValue);

        var notesElement = assetDetail.Elements.Single(e => e.ColumnName == "notes");
        Assert.Null(notesElement.ClassificationCategory);

        // Human correction on the "notes" column, which the system left
        // unclassified — done by the Reviewer (classification.review),
        // not the Operator who registered the source (datasources.manage)
        // — separation of duties, the same shape Module 7 established.
        var correctResponse = await reviewerClient.PostAsJsonAsync($"/api/v1/data-elements/{notesElement.Id}/classification", new { category = "OTHER_PERSONAL_DATA" });
        Assert.Equal(HttpStatusCode.OK, correctResponse.StatusCode);
        var corrected = await correctResponse.Content.ReadFromJsonAsync<DataElementDetail>();
        Assert.Equal("OTHER_PERSONAL_DATA", corrected!.ClassificationCategory);
        Assert.Equal("HUMAN", corrected.ClassificationSource);
        Assert.True(corrected.IsHumanCorrected);
        Assert.Equal(100m, corrected.ClassificationConfidence);

        // Re-running discovery must not clobber the human correction.
        var secondJobResponse = await operatorClient.PostAsync($"/api/v1/data-sources/{dataSource.Id}/discovery-jobs", null);
        var secondJob = await secondJobResponse.Content.ReadFromJsonAsync<DiscoveryJobDto>();
        await WaitForJobToFinishAsync(operatorClient, secondJob!.Id);

        var reReadAssetDetail = await (await operatorClient.GetAsync($"/api/v1/data-assets/{customersAsset.Id}")).Content.ReadFromJsonAsync<DataAssetDetail>();
        var reReadNotes = reReadAssetDetail!.Elements.Single(e => e.ColumnName == "notes");
        Assert.Equal("OTHER_PERSONAL_DATA", reReadNotes.ClassificationCategory);
        Assert.True(reReadNotes.IsHumanCorrected);

        // Cannot cancel an already-terminal job.
        var cancelResponse = await operatorClient.PostAsync($"/api/v1/discovery-jobs/{finishedJob.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.Conflict, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task File_system_connector_discovers_a_real_csv_file_and_masks_its_sample()
    {
        var operatorClient = await AuthenticatedClientAsync(fixture.OperatorAEmail);
        var tempDir = Directory.CreateTempSubdirectory("dpdp-discovery-test-");
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir.FullName, "contacts.csv"), "id,email,phone\n1,alice@example.com,9876543210\n");

            var createResponse = await operatorClient.PostAsJsonAsync("/api/v1/data-sources", new
            {
                name = "FileSystem E2E Test Source",
                description = (string?)null,
                sourceType = "FILE_SYSTEM",
                host = (string?)null,
                port = (int?)null,
                databaseName = (string?)null,
                username = (string?)null,
                password = (string?)null,
                rootPath = tempDir.FullName,
                schemaFilter = (string?)null,
            });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var dataSource = await createResponse.Content.ReadFromJsonAsync<DataSourceDetail>();

            var startJobResponse = await operatorClient.PostAsync($"/api/v1/data-sources/{dataSource!.Id}/discovery-jobs", null);
            var startedJob = await startJobResponse.Content.ReadFromJsonAsync<DiscoveryJobDto>();
            var finishedJob = await WaitForJobToFinishAsync(operatorClient, startedJob!.Id);

            Assert.Equal("COMPLETED", finishedJob.Status);
            Assert.Equal(1, finishedJob.AssetsDiscoveredCount);

            var assetsResponse = await operatorClient.GetAsync($"/api/v1/data-assets?dataSourceId={dataSource.Id}");
            var assets = await assetsResponse.Content.ReadFromJsonAsync<PagedAssets>();
            var csvAsset = assets!.Items.Single();
            Assert.Equal("FILE", csvAsset.AssetType);
            Assert.Equal(1, csvAsset.EstimatedRowCount);

            var assetDetail = await (await operatorClient.GetAsync($"/api/v1/data-assets/{csvAsset.Id}")).Content.ReadFromJsonAsync<DataAssetDetail>();
            var emailElement = assetDetail!.Elements.Single(e => e.ColumnName == "email");
            Assert.Equal("CONTACT", emailElement.ClassificationCategory);
            Assert.DoesNotContain("alice@example.com", emailElement.SampleMaskedValue);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Read_only_user_can_view_but_not_manage_sources_or_jobs()
    {
        var operatorClient = await AuthenticatedClientAsync(fixture.OperatorAEmail);
        var readOnlyClient = await AuthenticatedClientAsync(fixture.ReadOnlyAEmail);

        var createResponse = await operatorClient.PostAsJsonAsync("/api/v1/data-sources", new
        {
            name = "Read Only Visibility Test Source",
            description = (string?)null,
            sourceType = "FILE_SYSTEM",
            host = (string?)null,
            port = (int?)null,
            databaseName = (string?)null,
            username = (string?)null,
            password = (string?)null,
            rootPath = CreateEmptyTempDir(),
            schemaFilter = (string?)null,
        });
        var dataSource = await createResponse.Content.ReadFromJsonAsync<DataSourceDetail>();

        var view = await readOnlyClient.GetAsync($"/api/v1/data-sources/{dataSource!.Id}");
        Assert.Equal(HttpStatusCode.OK, view.StatusCode);

        var createDenied = await readOnlyClient.PostAsJsonAsync("/api/v1/data-sources", new
        {
            name = "Should Be Denied", description = (string?)null, sourceType = "FILE_SYSTEM",
            host = (string?)null, port = (int?)null, databaseName = (string?)null, username = (string?)null,
            password = (string?)null, rootPath = CreateEmptyTempDir(), schemaFilter = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Forbidden, createDenied.StatusCode);

        var startJobDenied = await readOnlyClient.PostAsync($"/api/v1/data-sources/{dataSource.Id}/discovery-jobs", null);
        Assert.Equal(HttpStatusCode.Forbidden, startJobDenied.StatusCode);
    }

    [Fact]
    public async Task Reviewer_role_can_correct_classification_but_cannot_manage_sources()
    {
        var operatorClient = await AuthenticatedClientAsync(fixture.OperatorAEmail);
        var reviewerClient = await AuthenticatedClientAsync(fixture.ReviewerAEmail);

        var createDenied = await reviewerClient.PostAsJsonAsync("/api/v1/data-sources", new
        {
            name = "Reviewer Should Not Manage", description = (string?)null, sourceType = "FILE_SYSTEM",
            host = (string?)null, port = (int?)null, databaseName = (string?)null, username = (string?)null,
            password = (string?)null, rootPath = CreateEmptyTempDir(), schemaFilter = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Forbidden, createDenied.StatusCode);

        var rootPath = CreateEmptyTempDir();
        await File.WriteAllTextAsync(Path.Combine(rootPath, "people.csv"), "id,email\n1,bob@example.com\n");

        var createResponse = await operatorClient.PostAsJsonAsync("/api/v1/data-sources", new
        {
            name = "Reviewer Classification Test Source", description = (string?)null, sourceType = "FILE_SYSTEM",
            host = (string?)null, port = (int?)null, databaseName = (string?)null, username = (string?)null,
            password = (string?)null, rootPath, schemaFilter = (string?)null,
        });
        var dataSource = await createResponse.Content.ReadFromJsonAsync<DataSourceDetail>();
        var startJobResponse = await operatorClient.PostAsync($"/api/v1/data-sources/{dataSource!.Id}/discovery-jobs", null);
        var startedJob = await startJobResponse.Content.ReadFromJsonAsync<DiscoveryJobDto>();
        await WaitForJobToFinishAsync(operatorClient, startedJob!.Id);

        var assetsResponse = await operatorClient.GetAsync($"/api/v1/data-assets?dataSourceId={dataSource.Id}");
        var assets = await assetsResponse.Content.ReadFromJsonAsync<PagedAssets>();
        var assetDetail = await (await operatorClient.GetAsync($"/api/v1/data-assets/{assets!.Items.Single().Id}")).Content.ReadFromJsonAsync<DataAssetDetail>();
        var emailElement = assetDetail!.Elements.Single(e => e.ColumnName == "email");

        var correctResponse = await reviewerClient.PostAsJsonAsync($"/api/v1/data-elements/{emailElement.Id}/classification", new { category = "OTHER_PERSONAL_DATA" });
        Assert.Equal(HttpStatusCode.OK, correctResponse.StatusCode);
        var corrected = await correctResponse.Content.ReadFromJsonAsync<DataElementDetail>();
        Assert.Equal("OTHER_PERSONAL_DATA", corrected!.ClassificationCategory);
        Assert.True(corrected.IsHumanCorrected);
    }

    [Fact]
    public async Task Organisation_b_cannot_see_organisation_as_data_source_or_assets()
    {
        var operatorA = await AuthenticatedClientAsync(fixture.OperatorAEmail);
        var operatorB = await AuthenticatedClientAsync(fixture.OperatorBEmail);

        var createResponse = await operatorA.PostAsJsonAsync("/api/v1/data-sources", new
        {
            name = "Tenant Isolation Test Source", description = (string?)null, sourceType = "FILE_SYSTEM",
            host = (string?)null, port = (int?)null, databaseName = (string?)null, username = (string?)null,
            password = (string?)null, rootPath = CreateEmptyTempDir(), schemaFilter = (string?)null,
        });
        var dataSource = await createResponse.Content.ReadFromJsonAsync<DataSourceDetail>();

        var crossTenantView = await operatorB.GetAsync($"/api/v1/data-sources/{dataSource!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, crossTenantView.StatusCode);

        var crossTenantJobStart = await operatorB.PostAsync($"/api/v1/data-sources/{dataSource.Id}/discovery-jobs", null);
        Assert.Equal(HttpStatusCode.NotFound, crossTenantJobStart.StatusCode);

        var listResponse = await operatorB.GetAsync("/api/v1/data-sources");
        var page = await listResponse.Content.ReadFromJsonAsync<PagedSources>();
        Assert.DoesNotContain(page!.Items, s => s.Id == dataSource.Id);
    }

    [Fact]
    public async Task An_inactive_data_source_refuses_to_start_a_new_job()
    {
        var operatorClient = await AuthenticatedClientAsync(fixture.OperatorAEmail);

        var createResponse = await operatorClient.PostAsJsonAsync("/api/v1/data-sources", new
        {
            name = "Inactive Source Test", description = (string?)null, sourceType = "FILE_SYSTEM",
            host = (string?)null, port = (int?)null, databaseName = (string?)null, username = (string?)null,
            password = (string?)null, rootPath = CreateEmptyTempDir(), schemaFilter = (string?)null,
        });
        var dataSource = await createResponse.Content.ReadFromJsonAsync<DataSourceDetail>();

        var updateResponse = await operatorClient.PutAsJsonAsync($"/api/v1/data-sources/{dataSource!.Id}", new
        {
            name = dataSource.Name, description = (string?)null, host = (string?)null, port = (int?)null,
            databaseName = (string?)null, username = (string?)null, password = (string?)null,
            rootPath = CreateEmptyTempDir(), schemaFilter = (string?)null, isActive = false,
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var startJobResponse = await operatorClient.PostAsync($"/api/v1/data-sources/{dataSource.Id}/discovery-jobs", null);
        Assert.Equal(HttpStatusCode.Conflict, startJobResponse.StatusCode);
    }

    private sealed record DataSourceDetail(Guid Id, string Name, string SourceType);
    private sealed record TestConnectionResult(bool Succeeded, string? ErrorMessage);
    private sealed record DiscoveryJobDto(Guid Id, string Status, int AssetsDiscoveredCount, int ElementsDiscoveredCount, string? ErrorMessage);
    private sealed record DataAssetSummary(Guid Id, string AssetName, string AssetType, long? EstimatedRowCount);
    private sealed record PagedAssets(List<DataAssetSummary> Items);
    private sealed record PagedSources(List<DataSourceDetail> Items);
    private sealed record DataElementDetail(Guid Id, string ColumnName, string? ClassificationCategory, decimal? ClassificationConfidence, string? ClassificationSource, bool IsHumanCorrected, string? SampleMaskedValue);
    private sealed record DataAssetDetail(Guid Id, string AssetName, List<string> Indexes, List<DataElementDetail> Elements);
}
