using DPDP.Domain.Common;

namespace DPDP.Infrastructure.Persistence.Seed;

/// <summary>
/// The single source of legal-content seed data for Module 4, so it can be
/// reviewed as one file rather than scattered across nine entity
/// configurations. See docs/COMPLIANCE_CONTENT_GOVERNANCE.md for how this
/// content was sourced and what it deliberately does NOT claim.
///
/// Every SummaryText/Description below is a plain-language paraphrase
/// written for this system — never claimed to be verbatim statutory text.
/// Every citation is to a section that genuinely exists in the Digital
/// Personal Data Protection Act, 2023 (Act No. 22 of 2023), which received
/// Presidential assent on 11 August 2023. Section numbering and the
/// existence of each section are treated as verified given how widely and
/// consistently reported the Act's structure is; the interpretive content
/// under each section is explicitly marked ContentReviewStatus.DRAFT and
/// must be reviewed by qualified counsel before being relied upon.
///
/// The Digital Personal Data Protection Rules, 2025 are represented only
/// as an empty Framework/FrameworkVersion shell (see
/// DpdpRulesFrameworkId/DpdpRulesVersionId below) — no rule-level content
/// is seeded, because the exact numbering of the finally-notified Rules
/// was not something this system could verify with confidence. Populate
/// that framework only from the official Gazette notification.
/// </summary>
internal static class DpdpActSeedData
{
    // --- Frameworks & versions -------------------------------------------

    public static readonly Guid DpdpActFrameworkId = DeterministicGuid.Create("framework:dpdpa-2023");
    public static readonly Guid DpdpRulesFrameworkId = DeterministicGuid.Create("framework:dpdpr-2025");
    public static readonly Guid DpdpActVersionId = DeterministicGuid.Create("framework-version:dpdpa-2023:2023");
    public static readonly Guid DpdpRulesVersionId = DeterministicGuid.Create("framework-version:dpdpr-2025:2025");

    // --- Legal references (all under the Act, 2023 version) -------------

    public sealed record LegalRefSeed(string Key, string Citation, string Title, string Chapter, string Summary);

    public static readonly LegalRefSeed[] LegalReferences =
    [
        new("sec-4", "Section 4", "Grounds for processing personal data", "Chapter II — Obligations of Data Fiduciary",
            "Provides that a Data Fiduciary may process a Data Principal's personal data only in accordance with the Act, and only for a lawful purpose — either with the Data Principal's consent or for certain legitimate uses specified in the Act."),
        new("sec-5", "Section 5", "Notice", "Chapter II — Obligations of Data Fiduciary",
            "Requires a Data Fiduciary to give the Data Principal notice — before or at the time of requesting consent — describing the personal data to be collected and the purpose of processing, in clear and plain language, along with information on how to exercise rights and lodge complaints."),
        new("sec-6", "Section 6", "Consent", "Chapter II — Obligations of Data Fiduciary",
            "Requires that consent be free, specific, informed, unconditional, and unambiguous, given through clear affirmative action, and limited to the personal data necessary for the specified purpose. A Data Principal must be able to withdraw consent at any time as easily as it was given."),
        new("sec-7", "Section 7", "Certain legitimate uses", "Chapter II — Obligations of Data Fiduciary",
            "Lists specified situations in which a Data Fiduciary may process personal data without obtaining consent, such as where the Data Principal has voluntarily provided the data for a specified purpose, for the performance of functions of the State, medical emergencies, or employment-related purposes, among others set out in the Act."),
        new("sec-8", "Section 8", "General obligations of Data Fiduciary", "Chapter II — Obligations of Data Fiduciary",
            "Sets out the Data Fiduciary's general obligations, including maintaining accuracy of personal data, implementing reasonable security safeguards to prevent personal data breaches, notifying the Data Protection Board of India and affected Data Principals of a breach, erasing personal data when the purpose is no longer served (subject to legal retention requirements), and publishing the contact details of a person able to answer Data Principal questions about the processing of their personal data."),
        new("sec-9", "Section 9", "Processing of personal data of children", "Chapter II — Obligations of Data Fiduciary",
            "Requires a Data Fiduciary to obtain verifiable consent of a parent or lawful guardian before processing the personal data of a child, and prohibits processing likely to cause a detrimental effect on a child's wellbeing or tracking, behavioural monitoring, or targeted advertising directed at children, subject to exemptions the Central Government may notify."),
        new("sec-10", "Section 10", "Additional obligations of Significant Data Fiduciary", "Chapter II — Obligations of Data Fiduciary",
            "Empowers the Central Government to notify certain Data Fiduciaries as Significant Data Fiduciaries based on factors such as the volume and sensitivity of personal data processed, and imposes additional obligations on them, including appointing a Data Protection Officer based in India, appointing an independent data auditor, and undertaking periodic Data Protection Impact Assessments and audits."),
        new("sec-11", "Section 11", "Right to access information about personal data", "Chapter III — Rights and Duties of Data Principal",
            "Gives a Data Principal the right to obtain from a Data Fiduciary a summary of personal data being processed and the processing activities undertaken, subject to exceptions set out in the Act."),
        new("sec-12", "Section 12", "Right to correction and erasure of personal data", "Chapter III — Rights and Duties of Data Principal",
            "Gives a Data Principal the right to request correction, completion, updating, and erasure of personal data, which the Data Fiduciary must act upon unless retention is necessary for a specified purpose or under law."),
        new("sec-13", "Section 13", "Right of grievance redressal", "Chapter III — Rights and Duties of Data Principal",
            "Gives a Data Principal the right to have readily available means of grievance redressal provided by a Data Fiduciary or Consent Manager in respect of any act or omission regarding the performance of obligations under the Act."),
        new("sec-16", "Section 16", "Processing of personal data outside India", "Chapter IV — Special Provisions",
            "Empowers the Central Government to restrict, by notification, the transfer of personal data by a Data Fiduciary for processing to specific countries or territories outside India."),
    ];

    // --- Requirements ------------------------------------------------------

    public sealed record RequirementSeed(string Code, string LegalRefKey, string Title, string Description);

    public static readonly RequirementSeed[] Requirements =
    [
        new("REQ-NOTICE-01", "sec-5", "Provide itemised notice before or at consent",
            "Provide Data Principals with a clear, itemised notice — describing the personal data collected and the purpose(s) of processing, in plain language — before or at the time consent is sought, and make available the means to exercise rights under the Act and to lodge a complaint with the Data Protection Board."),
        new("REQ-CONSENT-01", "sec-6", "Operate a valid consent mechanism",
            "Ensure that consent is captured through clear affirmative action, is specific to the stated purpose, and is not bundled with unrelated purposes. Provide a mechanism for a Data Principal to withdraw consent at any time, at least as easily as it was given, and stop processing (subject to legal exceptions) upon withdrawal."),
        new("REQ-LEGITUSE-01", "sec-7", "Document reliance on a legitimate use ground",
            "Where personal data is processed without consent in reliance on a legitimate use under Section 7, document and be able to demonstrate which specific ground is relied upon and why it applies."),
        new("REQ-SEC-01", "sec-8", "Implement reasonable security safeguards",
            "Implement appropriate technical and organisational measures to protect personal data against unauthorised processing, accidental loss, destruction, or damage — proportionate to the volume and sensitivity of personal data processed."),
        new("REQ-BREACH-01", "sec-8", "Notify personal data breaches",
            "Maintain a documented process to detect, assess, and notify the Data Protection Board of India and affected Data Principals of a personal data breach without undue delay."),
        new("REQ-ERASURE-01", "sec-8", "Erase personal data when no longer needed",
            "Erase personal data upon withdrawal of consent or as soon as the specified purpose is no longer being served, unless retention is required by law, and require the same of any data processor engaged."),
        new("REQ-CONTACT-01", "sec-8", "Publish contact details for Data Principal questions",
            "Publish the name and contact details of a Data Protection Officer or other person able to answer, on behalf of the Data Fiduciary, questions about the processing of a Data Principal's personal data."),
        new("REQ-CHILD-01", "sec-9", "Safeguard children's personal data",
            "Obtain verifiable consent from a parent or lawful guardian before processing a child's personal data, and do not undertake tracking, behavioural monitoring, or targeted advertising directed at children, unless a Central Government exemption applies."),
        new("REQ-SDF-01", "sec-10", "Meet Significant Data Fiduciary obligations",
            "If notified as a Significant Data Fiduciary, appoint a Data Protection Officer based in India, engage an independent data auditor, and conduct periodic Data Protection Impact Assessments and audits."),
        new("REQ-ACCESS-01", "sec-11", "Respond to access requests",
            "Provide Data Principals a mechanism to request and receive a summary of their personal data being processed and the related processing activities, within a reasonable, published timeframe."),
        new("REQ-CORRECT-01", "sec-12", "Respond to correction and erasure requests",
            "Provide Data Principals a mechanism to request correction, completion, updating, and erasure of their personal data, and act on valid requests unless a lawful ground for continued retention exists."),
        new("REQ-GRIEVANCE-01", "sec-13", "Operate a grievance redressal mechanism",
            "Provide a readily accessible grievance redressal mechanism, publish the contact details of a person able to address Data Principal questions and complaints, and respond within a reasonable, published timeframe."),
        new("REQ-TRANSFER-01", "sec-16", "Monitor cross-border transfer restrictions",
            "Monitor and comply with any Central Government notification restricting transfer of personal data to specific countries or territories, before transferring personal data outside India."),
    ];

    // --- Control categories -------------------------------------------------

    public sealed record CategorySeed(string Key, string Name, string Description, int SortOrder);

    public static readonly CategorySeed[] Categories =
    [
        new("cat-notice-consent", "Notice & Consent", "Controls ensuring Data Principals receive proper notice and that consent is validly obtained, recorded, and withdrawable.", 1),
        new("cat-security-breach", "Security & Breach Management", "Controls covering technical/organisational security safeguards and personal data breach detection and notification.", 2),
        new("cat-retention-erasure", "Data Retention & Erasure", "Controls governing how long personal data is retained and how it is erased.", 3),
        new("cat-governance", "Data Fiduciary Governance", "Controls covering general accountability obligations such as publishing contact details for Data Principal questions.", 4),
        new("cat-children", "Children's Data Protection", "Controls specific to processing the personal data of children.", 5),
        new("cat-sdf", "Significant Data Fiduciary Governance", "Additional governance controls applicable only to organisations notified as Significant Data Fiduciaries.", 6),
        new("cat-dp-rights", "Data Principal Rights", "Controls covering an organisation's handling of Data Principal rights requests (access, correction, erasure, grievance redressal).", 7),
        new("cat-transfer", "Cross-Border Data Transfer", "Controls covering the transfer of personal data outside India.", 8),
    ];

    // --- Controls ---------------------------------------------------------

    public sealed record ControlSeed(
        string ControlId,
        string CategoryKey,
        string Name,
        string Description,
        string Objective,
        string RiskLevel,
        string? ApplicableConditions,
        string EvidenceSummary,
        string Guidance,
        string SourceReference,
        string RequirementCode);

    public static readonly ControlSeed[] Controls =
    [
        new("DPDP-CTRL-001", "cat-notice-consent", "Provide Notice Prior to Consent",
            "The organisation provides Data Principals with a clear, itemised notice describing the personal data collected and the purpose(s) of processing before or at the time consent is requested.",
            "Ensure Data Principals are properly informed before their personal data is collected, enabling meaningful consent.",
            "HIGH", "Applies whenever personal data is collected directly from a Data Principal with consent as the processing ground.",
            "Notice template(s) or screenshots shown to Data Principals at the point of data collection.",
            "Notices should be available in plain language, itemise the categories of personal data collected and each purpose of processing, and describe how to exercise rights and lodge a grievance.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 5", "REQ-NOTICE-01"),

        new("DPDP-CTRL-002", "cat-notice-consent", "Consent Management Mechanism",
            "The organisation captures consent through clear affirmative action, limits it to the stated purpose, and provides an easy withdrawal mechanism.",
            "Ensure consent relied upon as a processing ground is valid, specific, and revocable.",
            "HIGH", "Applies whenever consent is the ground relied upon for processing.",
            "Consent capture UI/flow evidence and withdrawal mechanism evidence.",
            "Avoid bundled or pre-ticked consent. Provide a withdrawal path at least as easy as the original consent action.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 6", "REQ-CONSENT-01"),

        new("DPDP-CTRL-003", "cat-notice-consent", "Legitimate Use Documentation",
            "Where personal data is processed without consent, the organisation documents which legitimate use ground under Section 7 applies and why.",
            "Demonstrate a lawful basis for processing that does not rely on consent.",
            "MEDIUM", "Applies only where processing relies on a Section 7 legitimate use rather than consent.",
            "Written justification memo identifying the specific legitimate use ground relied upon.",
            "Document the specific sub-clause of Section 7 relied upon (e.g. voluntary provision of data, State function, medical emergency, employment purposes) and retain supporting rationale.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 7", "REQ-LEGITUSE-01"),

        new("DPDP-CTRL-004", "cat-security-breach", "Technical & Organisational Security Safeguards",
            "The organisation implements technical and organisational measures proportionate to the volume and sensitivity of personal data it processes, to prevent unauthorised processing or accidental loss, destruction, or damage.",
            "Protect personal data against breach through appropriate security controls.",
            "CRITICAL", null,
            "Security policy documentation and/or independent audit/assessment reports.",
            "Consider encryption, access controls, logging/monitoring, employee training, and vendor security requirements proportionate to risk.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 8(5)", "REQ-SEC-01"),

        new("DPDP-CTRL-005", "cat-security-breach", "Personal Data Breach Notification Process",
            "The organisation maintains a documented process to detect, assess, and notify the Data Protection Board of India and affected Data Principals of a personal data breach.",
            "Ensure timely, complete breach notification to the regulator and affected individuals.",
            "CRITICAL", null,
            "Breach notification procedure/runbook document.",
            "The process should define detection, internal escalation, assessment, and notification steps and owners, without undue delay upon becoming aware of a breach.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 8(6)", "REQ-BREACH-01"),

        new("DPDP-CTRL-006", "cat-retention-erasure", "Data Retention & Erasure Schedule",
            "The organisation erases personal data when the specified purpose is no longer being served or consent is withdrawn, unless retention is required by law, and applies the same requirement to its data processors.",
            "Avoid retaining personal data beyond what is necessary or lawful.",
            "HIGH", null,
            "Data retention policy specifying retention periods by data/purpose category.",
            "Map retention periods to specific purposes and legal retention obligations; ensure processors are contractually bound to the same erasure requirements.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 8(7)-(8)", "REQ-ERASURE-01"),

        new("DPDP-CTRL-007", "cat-governance", "Grievance Redressal Contact Publication",
            "The organisation publishes the name and contact details of a Data Protection Officer or other person able to answer, on the organisation's behalf, questions about the processing of a Data Principal's personal data.",
            "Give Data Principals an accessible point of contact for questions about their personal data.",
            "MEDIUM", null,
            "Screenshot or URL of the published contact page.",
            "Publish this contact prominently, e.g. in the privacy notice and on the organisation's website.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 8(9)-(10)", "REQ-CONTACT-01"),

        new("DPDP-CTRL-008", "cat-children", "Children's Data Safeguards",
            "The organisation obtains verifiable consent from a parent or lawful guardian before processing a child's personal data, and does not undertake tracking, behavioural monitoring, or targeted advertising directed at children.",
            "Protect children from processing likely to cause them harm.",
            "CRITICAL", "Applies whenever the organisation processes, or has reason to believe it processes, personal data of individuals below the age threshold defined in the Act and its rules.",
            "Parental/guardian consent verification workflow documentation.",
            "Confirm applicable exemptions (if any) before relying on them, and design age-assurance and parental consent flows conservatively.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 9", "REQ-CHILD-01"),

        new("DPDP-CTRL-009", "cat-sdf", "Significant Data Fiduciary Governance Obligations",
            "If notified as a Significant Data Fiduciary, the organisation appoints a Data Protection Officer based in India, engages an independent data auditor, and conducts periodic Data Protection Impact Assessments and audits.",
            "Meet the heightened governance obligations applicable to Significant Data Fiduciaries.",
            "HIGH", "Applies only to organisations notified by the Central Government as a Significant Data Fiduciary.",
            "Most recent Data Protection Impact Assessment report and DPO/auditor appointment records.",
            "Confirm current Significant Data Fiduciary notification status before treating this control as applicable.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 10", "REQ-SDF-01"),

        new("DPDP-CTRL-010", "cat-dp-rights", "Data Principal Access Request Handling",
            "The organisation provides Data Principals a mechanism to request and receive a summary of their personal data being processed and related processing activities.",
            "Enable Data Principals to exercise their right to access information about their own personal data.",
            "MEDIUM", null,
            "Access request log or standard operating procedure with committed turnaround time.",
            "Publish a clear channel for access requests and a committed response timeframe.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 11", "REQ-ACCESS-01"),

        new("DPDP-CTRL-011", "cat-dp-rights", "Data Principal Correction & Erasure Request Handling",
            "The organisation provides Data Principals a mechanism to request correction, completion, updating, and erasure of their personal data.",
            "Enable Data Principals to exercise their right to correction and erasure.",
            "MEDIUM", null,
            "Correction/erasure request handling SOP document.",
            "Define validation steps, response timeframes, and any lawful grounds for declining a request.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 12", "REQ-CORRECT-01"),

        new("DPDP-CTRL-012", "cat-dp-rights", "Data Principal Grievance Redressal Mechanism",
            "The organisation provides a readily accessible grievance redressal mechanism and responds to Data Principal grievances within a reasonable, published timeframe.",
            "Give Data Principals an effective route to raise and resolve concerns before escalating to the Data Protection Board.",
            "MEDIUM", null,
            "Grievance redressal policy document and response-time commitment.",
            "Ensure the mechanism is genuinely accessible (e.g. a discoverable contact channel) and track resolution times.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 13", "REQ-GRIEVANCE-01"),

        new("DPDP-CTRL-013", "cat-transfer", "Cross-Border Data Transfer Compliance Monitoring",
            "The organisation monitors and complies with any Central Government notification restricting transfer of personal data to specific countries or territories before transferring personal data outside India.",
            "Avoid unlawful cross-border transfer of personal data.",
            "HIGH", "Applies whenever the organisation transfers personal data to a recipient outside India.",
            "Register of countries/territories to which personal data is transferred.",
            "Maintain a current transfer register and check it against any Central Government restriction notifications before each new cross-border transfer arrangement.",
            "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 16", "REQ-TRANSFER-01"),
    ];

    // --- Assessment questions (one per control) -----------------------------

    public sealed record QuestionSeed(string ControlId, string Code, string Text, string? HelpText, string QuestionType, string[]? Options);

    public static readonly QuestionSeed[] Questions =
    [
        new("DPDP-CTRL-001", "Q-CTRL-001-1", "Does the organisation provide an itemised notice to Data Principals describing the personal data collected and the purpose of processing, before or at the time of seeking consent?", "Notice should be in plain language and itemise categories of data and each purpose.", "YES_NO", null),
        new("DPDP-CTRL-002", "Q-CTRL-002-1", "How does the organisation capture consent from Data Principals?", "Select the option that best describes the current mechanism.", "MULTIPLE_CHOICE", ["Explicit opt-in via clear affirmative action", "Pre-ticked or implied consent", "No formal consent mechanism", "Not applicable — relies solely on a Section 7 legitimate use"]),
        new("DPDP-CTRL-003", "Q-CTRL-003-1", "Describe the legitimate use ground(s) relied upon and the documented justification.", "Reference the specific Section 7 sub-clause relied upon.", "TEXT", null),
        new("DPDP-CTRL-004", "Q-CTRL-004-1", "Which of the following technical/organisational safeguards are implemented?", "Select all that apply.", "MULTI_SELECT", ["Encryption at rest", "Encryption in transit", "Access controls / least privilege", "Regular security audits", "Employee security training", "Documented incident response plan"]),
        new("DPDP-CTRL-005", "Q-CTRL-005-1", "Does the organisation have a documented process to notify the Data Protection Board of India and affected Data Principals of a personal data breach?", "A written runbook, not just an informal practice.", "YES_NO", null),
        new("DPDP-CTRL-006", "Q-CTRL-006-1", "What is the maximum retention period, in days, applied to personal data after the processing purpose is fulfilled, absent a legal retention requirement?", "Enter the longest applicable retention period across data categories.", "NUMBER", null),
        new("DPDP-CTRL-007", "Q-CTRL-007-1", "Provide the URL where the organisation publishes its Data Protection Officer / grievance contact details.", "This is typically part of the privacy notice.", "URL", null),
        new("DPDP-CTRL-008", "Q-CTRL-008-1", "Does the organisation obtain verifiable parental or guardian consent before processing a child's personal data?", "Answer NOT_APPLICABLE-equivalent by selecting No and explaining in follow-up evidence if the organisation does not process children's data at all.", "YES_NO", null),
        new("DPDP-CTRL-009", "Q-CTRL-009-1", "What was the date of the organisation's most recent Data Protection Impact Assessment?", "Only applicable if notified as a Significant Data Fiduciary.", "DATE", null),
        new("DPDP-CTRL-010", "Q-CTRL-010-1", "What is the organisation's committed turnaround time, in days, for responding to a Data Principal access request?", "Enter the published or internally committed SLA.", "NUMBER", null),
        new("DPDP-CTRL-011", "Q-CTRL-011-1", "Does the organisation provide a mechanism for Data Principals to request correction or erasure of their personal data?", null, "YES_NO", null),
        new("DPDP-CTRL-012", "Q-CTRL-012-1", "Describe the organisation's grievance redressal process and its response-time commitment.", null, "TEXT", null),
        new("DPDP-CTRL-013", "Q-CTRL-013-1", "Upload the organisation's current register of countries/territories to which personal data is transferred, if any.", "Upload 'none' documentation if no cross-border transfers occur.", "FILE", null),
    ];

    // --- Evidence requirements (one per question) --------------------------

    public sealed record EvidenceSeed(string QuestionCode, string Name, string Description, bool IsMandatory, string AcceptableFormats);

    public static readonly EvidenceSeed[] EvidenceRequirements =
    [
        new("Q-CTRL-001-1", "Notice template or evidence", "Copy of the notice template/document or screenshot shown to Data Principals at the point of data collection.", true, "PDF, DOCX, PNG, JPG"),
        new("Q-CTRL-002-1", "Consent capture evidence", "Screenshot or export of the consent capture flow/UI, including the withdrawal mechanism.", true, "PDF, PNG, JPG"),
        new("Q-CTRL-003-1", "Legitimate use justification memo", "Written memo identifying the specific Section 7 ground relied upon and supporting rationale.", true, "PDF, DOCX"),
        new("Q-CTRL-004-1", "Security policy / audit evidence", "Security policy document and/or independent audit or assessment report evidencing implemented safeguards.", true, "PDF, DOCX"),
        new("Q-CTRL-005-1", "Breach notification procedure", "Documented breach detection, escalation, and notification procedure/runbook.", true, "PDF, DOCX"),
        new("Q-CTRL-006-1", "Data retention policy", "Data retention policy document specifying retention periods by data/purpose category.", true, "PDF, DOCX"),
        new("Q-CTRL-007-1", "Published contact page evidence", "Screenshot or URL of the published DPO/grievance contact page.", true, "PDF, PNG, JPG"),
        new("Q-CTRL-008-1", "Parental consent workflow evidence", "Documentation of the parental/guardian consent verification workflow.", true, "PDF, DOCX"),
        new("Q-CTRL-009-1", "DPIA report", "Most recent Data Protection Impact Assessment report.", true, "PDF, DOCX"),
        new("Q-CTRL-010-1", "Access request handling evidence", "Access request log or standard operating procedure document showing committed turnaround times.", false, "PDF, DOCX, XLSX"),
        new("Q-CTRL-011-1", "Correction/erasure request SOP", "Correction/erasure request handling standard operating procedure document.", true, "PDF, DOCX"),
        new("Q-CTRL-012-1", "Grievance redressal policy", "Grievance redressal policy document, including response-time commitments.", true, "PDF, DOCX"),
        new("Q-CTRL-013-1", "Cross-border transfer register", "Register/list of countries or territories to which personal data is transferred.", false, "PDF, DOCX, XLSX"),
    ];
}
