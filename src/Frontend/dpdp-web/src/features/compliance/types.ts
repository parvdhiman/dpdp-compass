import type { PagedResult } from "../users/types";

export const RISK_LEVELS = ["LOW", "MEDIUM", "HIGH", "CRITICAL"] as const;
export const CONTROL_STATUSES = ["DRAFT", "ACTIVE", "RETIRED"] as const;
export const QUESTION_TYPES = ["YES_NO", "MULTIPLE_CHOICE", "TEXT", "NUMBER", "DATE", "FILE", "URL", "MULTI_SELECT"] as const;
export const REVIEW_STATUSES = ["DRAFT", "LEGAL_REVIEWED", "APPROVED"] as const;

export interface FrameworkVersionSummary {
  id: string;
  versionLabel: string;
  isCurrent: boolean;
  reviewStatus: string;
  publicationDate: string | null;
  effectiveDate: string | null;
}

export interface Framework {
  id: string;
  name: string;
  code: string;
  jurisdiction: string;
  issuingAuthority: string;
  description: string | null;
  versions: FrameworkVersionSummary[];
}

export interface RequirementSummary {
  id: string;
  code: string;
  title: string;
}

export interface LegalReference {
  id: string;
  frameworkVersionId: string;
  citation: string;
  title: string;
  chapter: string | null;
  summaryText: string | null;
  sourceCitation: string;
  reviewStatus: string;
  requirementCount: number;
}

export interface FrameworkVersion {
  id: string;
  frameworkId: string;
  frameworkName: string;
  versionLabel: string;
  officialCitation: string | null;
  publicationDate: string | null;
  effectiveDate: string | null;
  sourceUrl: string | null;
  reviewStatus: string;
  isCurrent: boolean;
  changeSummary: string | null;
  legalReferences: LegalReference[];
}

export interface ControlSummary {
  id: string;
  controlId: string;
  name: string;
  categoryName: string;
  riskLevel: string;
  status: string;
  reviewStatus: string;
  version: number;
  sourceReference: string;
  questionCount: number;
}

export interface MappedRequirement {
  requirementId: string;
  requirementCode: string;
  requirementTitle: string;
  legalCitation: string;
  mappingNotes: string | null;
}

export interface EvidenceRequirement {
  id: string;
  assessmentQuestionId: string;
  questionCode: string;
  name: string;
  description: string | null;
  isMandatory: boolean;
  acceptableFormats: string | null;
}

export interface AssessmentQuestion {
  id: string;
  controlId: string;
  controlBusinessId: string;
  controlName: string;
  code: string;
  text: string;
  helpText: string | null;
  questionType: string;
  options: string[] | null;
  isRequired: boolean;
  sortOrder: number;
  evidenceRequirements: EvidenceRequirement[];
}

export interface ControlDetail {
  id: string;
  controlId: string;
  name: string;
  description: string;
  objective: string;
  controlCategoryId: string;
  categoryName: string;
  riskLevel: string;
  applicableConditions: string | null;
  evidenceRequirementsSummary: string | null;
  guidance: string | null;
  sourceReference: string;
  effectiveDate: string | null;
  reviewDate: string | null;
  version: number;
  status: string;
  reviewStatus: string;
  createdAt: string;
  updatedAt: string;
  mappedRequirements: MappedRequirement[];
  questions: AssessmentQuestion[];
}

export interface ControlCategory {
  id: string;
  name: string;
  description: string | null;
  sortOrder: number;
  controlCount: number;
}

export interface RequirementDetail {
  id: string;
  legalReferenceId: string;
  legalReferenceCitation: string;
  sourceCitation: string;
  code: string;
  title: string;
  description: string;
  reviewStatus: string;
  mappedControls: ControlSummary[];
}

export type PagedControls = PagedResult<ControlSummary>;
export type PagedQuestions = PagedResult<AssessmentQuestion>;
export type PagedEvidenceRequirements = PagedResult<EvidenceRequirement>;

export interface CreateControlPayload {
  controlId: string;
  name: string;
  description: string;
  objective: string;
  controlCategoryId: string;
  riskLevel: string;
  applicableConditions?: string;
  evidenceRequirementsSummary?: string;
  guidance?: string;
  sourceReference: string;
  effectiveDate?: string | null;
  reviewDate?: string | null;
}

export interface UpdateControlPayload extends Omit<CreateControlPayload, "controlId"> {
  reviewStatus: string;
}
