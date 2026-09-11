import type { PagedResult } from "../users/types";

export const FINDING_SEVERITIES = ["CRITICAL", "HIGH", "MEDIUM", "LOW", "INFORMATIONAL"] as const;
export const FINDING_STATUSES = ["OPEN", "ASSIGNED", "IN_PROGRESS", "PENDING_VERIFICATION", "RESOLVED", "CLOSED", "ACCEPTED_RISK"] as const;
export const LIKELIHOOD_LEVELS = ["RARE", "UNLIKELY", "POSSIBLE", "LIKELY", "ALMOST_CERTAIN"] as const;
export const IMPACT_LEVELS = ["NEGLIGIBLE", "MINOR", "MODERATE", "MAJOR", "SEVERE"] as const;

export interface FindingSummary {
  id: string;
  findingNumber: string;
  title: string;
  severity: string;
  status: string;
  source: string;
  ownerUserId: string | null;
  ownerName: string | null;
  dueDate: string | null;
  isOverdue: boolean;
  riskLevel: string | null;
  createdAt: string;
}

export interface FindingRemediationTaskSummary {
  id: string;
  title: string;
  status: string;
  ownerUserId: string | null;
  ownerName: string | null;
  dueDate: string | null;
}

export interface FindingDetail {
  id: string;
  findingNumber: string;
  title: string;
  description: string;
  source: string;
  assessmentId: string | null;
  assessmentControlId: string | null;
  controlId: string | null;
  controlBusinessId: string | null;
  controlName: string | null;
  assetReference: string | null;
  riskId: string | null;
  riskNumber: string | null;
  riskLevel: string | null;
  severity: string;
  ownerUserId: string | null;
  ownerName: string | null;
  dueDate: string | null;
  status: string;
  recommendation: string | null;
  createdAt: string;
  updatedAt: string;
  remediationTasks: FindingRemediationTaskSummary[];
}

export type PagedFindings = PagedResult<FindingSummary>;

export interface CreateFindingPayload {
  title: string;
  description: string;
  severity: string;
  controlId?: string | null;
  assetReference?: string | null;
  ownerUserId?: string | null;
  dueDate?: string | null;
  recommendation?: string | null;
}

export interface UpdateFindingPayload {
  title: string;
  description: string;
  severity: string;
  assetReference?: string | null;
  dueDate?: string | null;
  recommendation?: string | null;
}
