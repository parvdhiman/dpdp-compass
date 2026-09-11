namespace DPDP.Application.Modules.Evidence;

/// <summary>
/// The Application layer's view of an uploaded file — a plain BCL Stream,
/// never Microsoft.AspNetCore.Http.IFormFile, so this layer never
/// references ASP.NET Core (Clean Architecture boundary). The API layer
/// converts an IFormFile into this record.
/// </summary>
public sealed record EvidenceFileUpload(string FileName, string ContentType, long Length, Stream Content);
