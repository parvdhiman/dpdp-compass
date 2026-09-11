import type { PagedResult } from "../users/types";

export const REMEDIATION_STATUSES = ["OPEN", "IN_PROGRESS", "PENDING_VERIFICATION", "VERIFIED", "CLOSED"] as const;

export interface EvidenceReference {
  description?: string | null;
  url?: string | null;
}

export interface RemediationComment {
  id: string;
  authorUserId: string;
  authorName: string;
  comment: string;
  createdAt: string;
}

export interface RemediationTaskSummary {
  id: string;
  findingId: string;
  findingNumber: string;
  title: string;
  status: string;
  ownerUserId: string | null;
  ownerName: string | null;
  dueDate: string | null;
  isOverdue: boolean;
  createdAt: string;
}

export interface RemediationTaskDetail {
  id: string;
  findingId: string;
  findingNumber: string;
  findingTitle: string;
  title: string;
  description: string | null;
  ownerUserId: string | null;
  ownerName: string | null;
  dueDate: string | null;
  status: string;
  isOverdue: boolean;
  evidence: EvidenceReference[];
  verifiedByUserId: string | null;
  verifiedByName: string | null;
  verifiedAt: string | null;
  verificationNotes: string | null;
  createdAt: string;
  updatedAt: string;
  comments: RemediationComment[];
}

export type PagedRemediationTasks = PagedResult<RemediationTaskSummary>;

export interface CreateRemediationTaskPayload {
  findingId: string;
  title: string;
  description?: string | null;
  ownerUserId?: string | null;
  dueDate?: string | null;
}
