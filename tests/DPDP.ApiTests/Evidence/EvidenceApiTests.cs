using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace DPDP.ApiTests.Evidence;

/// <summary>
/// Module 7 quality-gate tests: upload/versioning/review lifecycle, file
/// validation (extension/MIME/magic-byte), authorization, and tenant
/// isolation for Evidence Management, exercised end-to-end at the HTTP
/// layer. EvidenceStatusTransitions and EvidenceFileValidator's pure logic
/// are covered as fast unit tests instead (DPDP.UnitTests) since they need
/// no database; this class covers what genuinely needs the database, the
/// object-storage abstraction, and the HTTP pipeline.
/// </summary>
[Collection("Evidence")]
public sealed class EvidenceApiTests(EvidenceApiFixture fixture)
{
    private static readonly byte[] MinimalPdfBytes = Encoding.ASCII.GetBytes("%PDF-1.4\n%%EOF");

    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = fixture.CreateClient();
        var token = await fixture.LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    private static MultipartFormDataContent PdfUploadContent(string title, byte[]? pdfBytes = null, string? evidenceType = null)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(title), "title" },
            { new StringContent(evidenceType ?? "PDF"), "evidenceType" },
        };
        var fileContent = new ByteArrayContent(pdfBytes ?? MinimalPdfBytes);
        fileContent.Headers.ContentType = new("application/pdf");
        content.Add(fileContent, "file", "test-evidence.pdf");
        return content;
    }

    private async Task<EvidenceDetail> UploadPdfAsync(HttpClient client, string title)
    {
        var response = await client.PostAsync("/api/v1/evidence", PdfUploadContent(title));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<EvidenceDetail>())!;
    }

    [Fact]
    public async Task Full_lifecycle_upload_submit_approve_succeeds()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var reviewer = await AuthenticatedClientAsync(fixture.ReviewerAEmail);

        var evidence = await UploadPdfAsync(uploader, "Lifecycle Approve Test");
        Assert.Equal("UPLOADED", evidence.Status);
        Assert.Single(evidence.Versions);
        Assert.NotNull(evidence.Versions[0].ChecksumSha256);
        Assert.Equal(64, evidence.Versions[0].ChecksumSha256!.Length);

        var assignReviewer = await uploader.PutAsJsonAsync($"/api/v1/evidence/{evidence.Id}", new
        {
            title = evidence.Title,
            description = (string?)null,
            vendorReference = (string?)null,
            processingActivityReference = (string?)null,
            ownerUserId = (Guid?)null,
            reviewerUserId = fixture.ReviewerAUserId,
            expiryDate = (DateOnly?)null,
        });
        Assert.Equal(HttpStatusCode.OK, assignReviewer.StatusCode);

        var submit = await uploader.PostAsync($"/api/v1/evidence/{evidence.Id}/submit", null);
        Assert.Equal(HttpStatusCode.OK, submit.StatusCode);
        var submitted = await submit.Content.ReadFromJsonAsync<EvidenceDetail>();
        Assert.Equal("UNDER_REVIEW", submitted!.Status);

        var approve = await reviewer.PostAsJsonAsync($"/api/v1/evidence/{evidence.Id}/approve", new { comments = "Looks good." });
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);
        var approved = await approve.Content.ReadFromJsonAsync<EvidenceDetail>();
        Assert.Equal("APPROVED", approved!.Status);
        Assert.NotNull(approved.ApprovedAt);
        Assert.Single(approved.Reviews);
        Assert.Equal("APPROVED", approved.Reviews[0].Decision);
    }

    [Fact]
    public async Task Reject_then_new_version_then_resubmit_and_approve_succeeds()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var reviewer = await AuthenticatedClientAsync(fixture.ReviewerAEmail);

        var evidence = await UploadPdfAsync(uploader, "Reject Then Reupload Test");

        await uploader.PutAsJsonAsync($"/api/v1/evidence/{evidence.Id}", new
        {
            title = evidence.Title,
            description = (string?)null,
            vendorReference = (string?)null,
            processingActivityReference = (string?)null,
            ownerUserId = (Guid?)null,
            reviewerUserId = fixture.ReviewerAUserId,
            expiryDate = (DateOnly?)null,
        });
        await uploader.PostAsync($"/api/v1/evidence/{evidence.Id}/submit", null);

        var reject = await reviewer.PostAsJsonAsync($"/api/v1/evidence/{evidence.Id}/reject", new { rejectionReason = "Wrong document attached." });
        Assert.Equal(HttpStatusCode.OK, reject.StatusCode);
        var rejected = await reject.Content.ReadFromJsonAsync<EvidenceDetail>();
        Assert.Equal("REJECTED", rejected!.Status);
        Assert.Equal("Wrong document attached.", rejected.RejectionReason);

        var newVersionContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(MinimalPdfBytes);
        fileContent.Headers.ContentType = new("application/pdf");
        newVersionContent.Add(fileContent, "file", "corrected.pdf");
        var versionResponse = await uploader.PostAsync($"/api/v1/evidence/{evidence.Id}/versions", newVersionContent);
        Assert.Equal(HttpStatusCode.OK, versionResponse.StatusCode);
        var withNewVersion = await versionResponse.Content.ReadFromJsonAsync<EvidenceDetail>();
        Assert.Equal("UPLOADED", withNewVersion!.Status);
        Assert.Equal(2, withNewVersion.Versions.Count);

        await uploader.PostAsync($"/api/v1/evidence/{evidence.Id}/submit", null);
        var approve = await reviewer.PostAsJsonAsync($"/api/v1/evidence/{evidence.Id}/approve", new { comments = (string?)null });
        var approved = await approve.Content.ReadFromJsonAsync<EvidenceDetail>();
        Assert.Equal("APPROVED", approved!.Status);
    }

    [Fact]
    public async Task Download_returns_the_uploaded_file_bytes()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var evidence = await UploadPdfAsync(uploader, "Download Test");

        var download = await uploader.GetAsync($"/api/v1/evidence/{evidence.Id}/download");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        var bytes = await download.Content.ReadAsByteArrayAsync();
        Assert.Equal(MinimalPdfBytes, bytes);
    }

    [Fact]
    public async Task Wrong_extension_is_rejected()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var content = new MultipartFormDataContent
        {
            { new StringContent("Bad Extension Test"), "title" },
            { new StringContent("PDF"), "evidenceType" },
        };
        var fileContent = new ByteArrayContent(MinimalPdfBytes);
        fileContent.Headers.ContentType = new("application/pdf");
        content.Add(fileContent, "file", "malware.exe");

        var response = await uploader.PostAsync("/api/v1/evidence", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Content_type_mismatched_with_extension_is_rejected()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var content = new MultipartFormDataContent
        {
            { new StringContent("Mismatched Content Type Test"), "title" },
            { new StringContent("PDF"), "evidenceType" },
        };
        var fileContent = new ByteArrayContent(MinimalPdfBytes);
        fileContent.Headers.ContentType = new("image/png");
        content.Add(fileContent, "file", "test.pdf");

        var response = await uploader.PostAsync("/api/v1/evidence", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Corrupted_magic_bytes_are_rejected_even_with_matching_extension_and_content_type()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var content = new MultipartFormDataContent
        {
            { new StringContent("Corrupted Bytes Test"), "title" },
            { new StringContent("PDF"), "evidenceType" },
        };
        var fileContent = new ByteArrayContent(Encoding.ASCII.GetBytes("this is not actually a pdf file"));
        fileContent.Headers.ContentType = new("application/pdf");
        content.Add(fileContent, "file", "fake.pdf");

        var response = await uploader.PostAsync("/api/v1/evidence", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Uploader_role_cannot_review_and_reviewer_role_cannot_upload()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var reviewer = await AuthenticatedClientAsync(fixture.ReviewerAEmail);

        var evidence = await UploadPdfAsync(uploader, "Separation Of Duties Test");

        var reviewerTriesToUpload = await reviewer.PostAsync("/api/v1/evidence", PdfUploadContent("Reviewer Cannot Upload"));
        Assert.Equal(HttpStatusCode.Forbidden, reviewerTriesToUpload.StatusCode);

        var uploaderTriesToApprove = await uploader.PostAsJsonAsync($"/api/v1/evidence/{evidence.Id}/approve", new { comments = (string?)null });
        Assert.Equal(HttpStatusCode.Forbidden, uploaderTriesToApprove.StatusCode);
    }

    [Fact]
    public async Task Read_only_user_can_view_but_not_upload_or_review()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var readOnly = await AuthenticatedClientAsync(fixture.ReadOnlyAEmail);

        var evidence = await UploadPdfAsync(uploader, "Read Only Visibility Test");

        var view = await readOnly.GetAsync($"/api/v1/evidence/{evidence.Id}");
        Assert.Equal(HttpStatusCode.OK, view.StatusCode);

        var upload = await readOnly.PostAsync("/api/v1/evidence", PdfUploadContent("Read Only Cannot Upload"));
        Assert.Equal(HttpStatusCode.Forbidden, upload.StatusCode);

        var approve = await readOnly.PostAsJsonAsync($"/api/v1/evidence/{evidence.Id}/approve", new { comments = (string?)null });
        Assert.Equal(HttpStatusCode.Forbidden, approve.StatusCode);
    }

    [Fact]
    public async Task Organisation_b_cannot_see_or_download_organisation_as_evidence()
    {
        var uploaderA = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var uploaderB = await AuthenticatedClientAsync(fixture.UploaderBEmail);

        var evidence = await UploadPdfAsync(uploaderA, "Tenant Isolation Test");

        var crossTenantView = await uploaderB.GetAsync($"/api/v1/evidence/{evidence.Id}");
        Assert.Equal(HttpStatusCode.NotFound, crossTenantView.StatusCode);

        var crossTenantDownload = await uploaderB.GetAsync($"/api/v1/evidence/{evidence.Id}/download");
        Assert.Equal(HttpStatusCode.NotFound, crossTenantDownload.StatusCode);

        var listResponse = await uploaderB.GetAsync("/api/v1/evidence");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedEvidence>();
        Assert.DoesNotContain(page!.Items, e => e.Id == evidence.Id);
    }

    [Fact]
    public async Task Archived_evidence_cannot_be_edited_or_receive_new_versions()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var reviewer = await AuthenticatedClientAsync(fixture.ReviewerAEmail);

        var evidence = await UploadPdfAsync(uploader, "Archive Lock Test");
        await uploader.PutAsJsonAsync($"/api/v1/evidence/{evidence.Id}", new
        {
            title = evidence.Title,
            description = (string?)null,
            vendorReference = (string?)null,
            processingActivityReference = (string?)null,
            ownerUserId = (Guid?)null,
            reviewerUserId = fixture.ReviewerAUserId,
            expiryDate = (DateOnly?)null,
        });
        await uploader.PostAsync($"/api/v1/evidence/{evidence.Id}/submit", null);
        await reviewer.PostAsJsonAsync($"/api/v1/evidence/{evidence.Id}/approve", new { comments = (string?)null });

        var archive = await reviewer.PostAsync($"/api/v1/evidence/{evidence.Id}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);
        var archived = await archive.Content.ReadFromJsonAsync<EvidenceDetail>();
        Assert.Equal("ARCHIVED", archived!.Status);

        var newVersionContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(MinimalPdfBytes);
        fileContent.Headers.ContentType = new("application/pdf");
        newVersionContent.Add(fileContent, "file", "too-late.pdf");
        var versionAfterArchive = await uploader.PostAsync($"/api/v1/evidence/{evidence.Id}/versions", newVersionContent);
        Assert.Equal(HttpStatusCode.Conflict, versionAfterArchive.StatusCode);
    }

    [Fact]
    public async Task Url_evidence_requires_no_file_and_cannot_be_downloaded()
    {
        var uploader = await AuthenticatedClientAsync(fixture.UploaderAEmail);
        var content = new MultipartFormDataContent
        {
            { new StringContent("URL Evidence Test"), "title" },
            { new StringContent("URL"), "evidenceType" },
            { new StringContent("https://policies.example.com/data-retention"), "externalUrl" },
        };

        var response = await uploader.PostAsync("/api/v1/evidence", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var evidence = await response.Content.ReadFromJsonAsync<EvidenceDetail>();
        Assert.Equal("SKIPPED", evidence!.Versions[0].MalwareScanStatus);
        Assert.Null(evidence.Versions[0].ChecksumSha256);

        var download = await uploader.GetAsync($"/api/v1/evidence/{evidence.Id}/download");
        Assert.Equal(HttpStatusCode.Conflict, download.StatusCode);
    }

    private sealed record EvidenceDetail(
        Guid Id, string Title, string Status, DateTimeOffset? ApprovedAt, string? RejectionReason,
        List<EvidenceVersionEntry> Versions, List<EvidenceReviewEntry> Reviews);

    private sealed record EvidenceVersionEntry(int VersionNumber, string? ChecksumSha256, string MalwareScanStatus);

    private sealed record EvidenceReviewEntry(string Decision);

    private sealed record EvidenceSummaryEntry(Guid Id);

    private sealed record PagedEvidence(List<EvidenceSummaryEntry> Items);
}
