namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// "Store files in object storage abstraction... Support MinIO, AWS S3,
/// Azure Blob Storage through an abstraction layer" (MASTER_PROMPT
/// section 2 / Module 7 brief). The Application layer only ever sees this
/// interface — never a filesystem path, never a cloud SDK type. Keys are
/// always server-generated (see EvidenceItem/EvidenceVersion), never
/// derived from a client-supplied file name, so implementations never
/// need to defend against path traversal themselves — the caller already
/// guarantees a safe key. See docs/ARCHITECTURE.md Module 7 section for
/// the current implementation and how to swap in a real S3/MinIO client.
/// </summary>
public interface IObjectStorageService
{
    Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
