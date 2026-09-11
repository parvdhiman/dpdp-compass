namespace DPDP.Infrastructure.Storage;

/// <summary>Bound from configuration section "ObjectStorage" — see appsettings.json and docs/ARCHITECTURE.md Module 7 section.</summary>
public sealed class ObjectStorageOptions
{
    public const string SectionName = "ObjectStorage";

    /// <summary>Local-disk root used by FileSystemObjectStorageService. Swap the DI registration for a real S3/MinIO client to move off local disk — see docs/ARCHITECTURE.md Module 7 section.</summary>
    public string RootPath { get; set; } = "storage/evidence";
}
