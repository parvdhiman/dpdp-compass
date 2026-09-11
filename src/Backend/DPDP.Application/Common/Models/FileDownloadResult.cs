namespace DPDP.Application.Common.Models;

public sealed record FileDownloadResult(Stream Content, string ContentType, string FileName);
