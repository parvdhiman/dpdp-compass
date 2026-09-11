using DPDP.Domain.Modules.Evidence;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class EvidenceFileValidatorTests
{
    [Theory]
    [InlineData(".pdf")]
    [InlineData(".PDF")]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".gif")]
    [InlineData(".docx")]
    [InlineData(".xlsx")]
    [InlineData(".txt")]
    [InlineData(".csv")]
    [InlineData(".json")]
    public void Allow_listed_extensions_are_allowed(string extension)
    {
        Assert.True(EvidenceFileValidator.IsExtensionAllowed(extension));
    }

    [Theory]
    [InlineData(".exe")]
    [InlineData(".sh")]
    [InlineData(".html")]
    [InlineData(".svg")]
    [InlineData(".zip")]
    [InlineData("")]
    public void Non_allow_listed_extensions_are_rejected(string extension)
    {
        Assert.False(EvidenceFileValidator.IsExtensionAllowed(extension));
    }

    [Theory]
    [InlineData(".pdf", "application/pdf", true)]
    [InlineData(".pdf", "image/png", false)]
    [InlineData(".png", "image/png", true)]
    [InlineData(".jpg", "image/jpeg", true)]
    [InlineData(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", true)]
    [InlineData(".docx", "application/pdf", false)]
    public void Content_type_must_match_extension(string extension, string contentType, bool expected)
    {
        Assert.Equal(expected, EvidenceFileValidator.IsContentTypeAllowedForExtension(extension, contentType));
    }

    [Fact]
    public void Pdf_signature_must_match_magic_bytes()
    {
        var validHeader = "%PDF-1.4"u8.ToArray();
        var invalidHeader = "MZ\x90\x00"u8.ToArray();

        Assert.True(EvidenceFileValidator.MatchesFileSignature(".pdf", validHeader));
        Assert.False(EvidenceFileValidator.MatchesFileSignature(".pdf", invalidHeader));
    }

    [Fact]
    public void Png_signature_must_match_magic_bytes()
    {
        var validHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
        var invalidHeader = "%PDF"u8.ToArray();

        Assert.True(EvidenceFileValidator.MatchesFileSignature(".png", validHeader));
        Assert.False(EvidenceFileValidator.MatchesFileSignature(".png", invalidHeader));
    }

    [Fact]
    public void An_extension_with_no_defined_signature_always_matches()
    {
        Assert.True(EvidenceFileValidator.MatchesFileSignature(".txt", "anything at all"u8.ToArray()));
        Assert.True(EvidenceFileValidator.MatchesFileSignature(".csv", []));
    }

    [Fact]
    public void A_header_shorter_than_the_signature_does_not_match()
    {
        Assert.False(EvidenceFileValidator.MatchesFileSignature(".pdf", "%P"u8.ToArray()));
    }

    [Theory]
    [InlineData("application/pdf", true)]
    [InlineData("image/png", true)]
    [InlineData("image/jpeg", true)]
    [InlineData("image/gif", true)]
    [InlineData("text/html", false)]
    [InlineData("image/svg+xml", false)]
    [InlineData("application/xml", false)]
    [InlineData(null, false)]
    public void Only_the_small_preview_safe_allow_list_is_previewable(string? contentType, bool expected)
    {
        Assert.Equal(expected, EvidenceFileValidator.IsPreviewSafe(contentType));
    }
}
