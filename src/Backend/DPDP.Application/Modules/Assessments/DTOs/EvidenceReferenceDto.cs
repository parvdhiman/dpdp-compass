namespace DPDP.Application.Modules.Assessments.DTOs;

/// <summary>
/// One attached evidence reference for an AssessmentAnswer — a description
/// and/or a URL, no file upload/storage yet (see
/// docs/ARCHITECTURE.md Module 5 section). RequirementId links back to the
/// Compliance.EvidenceRequirement it's meant to satisfy, when applicable.
/// </summary>
public sealed record EvidenceReferenceDto(Guid? RequirementId, string? Description, string? Url);
