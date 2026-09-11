import type { PagedResult } from "../users/types";

export const ASSESSMENT_STATUSES = ["DRAFT", "IN_PROGRESS", "SUBMITTED", "UNDER_REVIEW", "APPROVED", "REJECTED", "ARCHIVED"] as const;
export const ANSWER_STATUSES = ["PASS", "PARTIAL", "FAIL", "NOT_APPLICABLE", "NOT_ASSESSED", "NEEDS_REVIEW"] as const;
export const CONFIDENCE_LEVELS = ["LOW", "MEDIUM", "HIGH"] as const;
export const REVIEW_DECISIONS = ["NEEDS_CHANGES", "READY_FOR_APPROVAL"] as const;

export interface AssessmentScopeInput {
  businessUnitId?: string | null;
  departmentId?: string | null;
  notes?: string | null;
}

export interface AssessmentScope {
  id: string;
  businessUnitId: string | null;
  businessUnitName: string | null;
  departmentId: string | null;
  departmentName: string | null;
  notes: string | null;
}

export interface AssessmentReview {
  id: string;
  reviewerId: string;
  reviewerName: string;
  decision: string;
  comments: string | null;
  createdAt: string;
}

export interface AssessmentApproval {
  id: string;
  decidedByUserId: string;
  decidedByUserName: string;
  decision: string;
  comments: string | null;
  createdAt: string;
}

export interface AssessmentControlSummary {
  id: string;
  controlId: string;
  controlBusinessId: string;
  controlName: string;
  categoryName: string;
  riskLevel: string;
  status: string;
  notes: string | null;
  questionCount: number;
  answeredQuestionCount: number;
}

export interface AssessmentSummary {
  id: string;
  name: string;
  status: string;
  frameworkVersionId: string;
  frameworkName: string;
  frameworkVersionLabel: string;
  assignedToUserId: string | null;
  assignedToUserName: string | null;
  dueDate: string | null;
  submittedAt: string | null;
  decidedAt: string | null;
  createdAt: string;
  totalControls: number;
  completedControls: number;
}

export interface AssessmentDetail {
  id: string;
  name: string;
  description: string | null;
  status: string;
  frameworkVersionId: string;
  frameworkName: string;
  frameworkVersionLabel: string;
  assignedToUserId: string | null;
  assignedToUserName: string | null;
  dueDate: string | null;
  submittedAt: string | null;
  submittedBy: string | null;
  decidedAt: string | null;
  decidedBy: string | null;
  createdAt: string;
  updatedAt: string;
  scopes: AssessmentScope[];
  controls: AssessmentControlSummary[];
  reviews: AssessmentReview[];
  approvals: AssessmentApproval[];
}

export interface EvidenceReference {
  requirementId?: string | null;
  description?: string | null;
  url?: string | null;
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

export interface AssessmentAnswer {
  assessmentControlQuestionId: string;
  questionId: string;
  questionCode: string;
  questionText: string;
  helpText: string | null;
  questionType: string;
  options: string[] | null;
  isRequired: boolean;
  sortOrder: number;
  evidenceRequirements: EvidenceRequirement[];
  answerId: string;
  status: string;
  answerValue: string | null;
  answerValues: string[] | null;
  comment: string | null;
  evidence: EvidenceReference[];
  reviewerId: string | null;
  reviewerName: string | null;
  reviewedAt: string | null;
  reviewComment: string | null;
  confidence: string | null;
  assessedRiskLevel: string | null;
  remediationNotes: string | null;
}

export interface AssessmentControlQuestionnaire {
  assessmentControlId: string;
  controlId: string;
  controlBusinessId: string;
  controlName: string;
  categoryName: string;
  riskLevel: string;
  status: string;
  notes: string | null;
  questions: AssessmentAnswer[];
}

export interface AssessmentScore {
  overallScore: number | null;
  controlScore: number | null;
  riskAdjustedScore: number | null;
  evidenceCoveragePercent: number | null;
  assessmentCoveragePercent: number | null;
  totalControls: number;
  applicableControls: number;
  totalRequiredQuestions: number;
  answeredRequiredQuestions: number;
}

export type PagedAssessments = PagedResult<AssessmentSummary>;

export interface CreateAssessmentPayload {
  frameworkVersionId: string;
  name: string;
  description?: string | null;
  assignedToUserId?: string | null;
  dueDate?: string | null;
  scopes?: AssessmentScopeInput[] | null;
}

export interface SaveAnswerPayload {
  status: string;
  answerValue?: string | null;
  answerValues?: string[] | null;
  comment?: string | null;
  evidence?: EvidenceReference[] | null;
  confidence?: string | null;
  assessedRiskLevel?: string | null;
  remediationNotes?: string | null;
}
