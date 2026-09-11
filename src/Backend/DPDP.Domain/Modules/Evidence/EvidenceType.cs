namespace DPDP.Domain.Modules.Evidence;

/// <summary>What the evidence represents, not its file format — a Contract or a Policy can each be a PDF or a DOCX. URL is the one type that never has an uploaded file.</summary>
public enum EvidenceType
{
    PDF,
    DOCX,
    XLSX,
    IMAGE,
    URL,
    POLICY,
    SCREENSHOT,
    CONFIGURATION,
    AUDIT_RECORD,
    CONTRACT,
    APPROVAL_RECORD,
}
