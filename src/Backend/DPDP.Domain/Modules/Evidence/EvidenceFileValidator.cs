namespace DPDP.Domain.Modules.Evidence;

/// <summary>
/// Pure file-type/MIME/signature validation — no database or I/O access,
/// unit-testable directly (Module 7's explicit "file type validation" /
/// "MIME validation" requirements). Deliberately checks the file's actual
/// leading bytes against a known signature where one exists, not just the
/// client-declared Content-Type header — a renamed .exe claiming to be a
/// PDF is rejected even though the header says "application/pdf". Text-
/// based formats (txt/csv/json/xml/yaml) have no fixed byte signature, so
/// only the extension/content-type pairing is checked for those.
/// </summary>
public static class EvidenceFileValidator
{
    private sealed record AllowedFileType(string ContentType, byte[]? Signature);

    private static readonly Dictionary<string, AllowedFileType> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = new AllowedFileType("application/pdf", "%PDF"u8.ToArray()),
        [".png"] = new AllowedFileType("image/png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        [".jpg"] = new AllowedFileType("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".jpeg"] = new AllowedFileType("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".gif"] = new AllowedFileType("image/gif", "GIF8"u8.ToArray()),
        [".docx"] = new AllowedFileType("application/vnd.openxmlformats-officedocument.wordprocessingml.document", [0x50, 0x4B, 0x03, 0x04]),
        [".xlsx"] = new AllowedFileType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", [0x50, 0x4B, 0x03, 0x04]),
        [".txt"] = new AllowedFileType("text/plain", null),
        [".csv"] = new AllowedFileType("text/csv", null),
        [".json"] = new AllowedFileType("application/json", null),
        [".xml"] = new AllowedFileType("application/xml", null),
        [".yaml"] = new AllowedFileType("application/x-yaml", null),
        [".yml"] = new AllowedFileType("application/x-yaml", null),
    };

    /// <summary>Content types safe to render inline in a browser without triggering a download or executing embedded content — never HTML/SVG/XML, which could carry script.</summary>
    private static readonly HashSet<string> PreviewSafeContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "image/png", "image/jpeg", "image/gif",
    };

    public static bool IsExtensionAllowed(string extension) => AllowedExtensions.ContainsKey(extension);

    public static bool IsContentTypeAllowedForExtension(string extension, string contentType) =>
        AllowedExtensions.TryGetValue(extension, out var allowed)
        && string.Equals(allowed.ContentType, contentType, StringComparison.OrdinalIgnoreCase);

    /// <summary>True if the extension has no known signature (text-based formats) or the header bytes match it.</summary>
    public static bool MatchesFileSignature(string extension, ReadOnlySpan<byte> header)
    {
        if (!AllowedExtensions.TryGetValue(extension, out var allowed) || allowed.Signature is null)
        {
            return true;
        }

        return header.Length >= allowed.Signature.Length && header[..allowed.Signature.Length].SequenceEqual(allowed.Signature);
    }

    public static bool IsPreviewSafe(string? contentType) => contentType is not null && PreviewSafeContentTypes.Contains(contentType);
}
