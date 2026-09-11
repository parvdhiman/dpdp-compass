import type { PagedResult } from "../users/types";

export const EVIDENCE_TYPES = [
  "PDF", "DOCX", "XLSX", "IMAGE", "URL", "POLICY", "SCREENSHOT",
  "CONFIGURATION", "AUDIT_RECORD", "CONTRACT", "APPROVAL_RECORD",
] as const;

export const EVIDENCE_STATUSES = ["UPLOADED", "UNDER_REVIEW", "APPROVED", "REJECTED", "EXPIRED", "ARCHIVED"] as const;

export interface EvidenceSummary {
  id: string;
  evidenceNumber: string;
  title: string;
  evidenceType: string;
  status: string;
  ownerUserId: string | null;
  ownerName: string | null;
  reviewerUserId: string | null;
  reviewerName: string | null;
  expiryDate: string | null;
  isExpired: boolean;
  currentVersionNumber: number;
  createdAt: string;
}

export interface EvidenceVersion {
  id: string;
  versionNumber: number;
  originalFileName: string | null;
  contentType: string | null;
  sizeBytes: number | null;
  checksumSha256: string | null;
  externalUrl: string | null;
  malwareScanStatus: string;
  isPreviewSafe: boolean;
  uploadedByUserId: string;
  uploadedByName: string;
  uploadedAt: string;
}

export interface EvidenceReviewRecord {
  id: string;
  evidenceVersionNumber: number;
  reviewerUserId: string;
  reviewerName: string;
  decision: string;
  comments: string | null;
  createdAt: string;
}

export interface EvidenceDetail {
  id: string;
  evidenceNumber: string;
  title: string;
  description: string | null;
  evidenceType: string;
  status: string;
  assessmentId: string | null;
  controlId: string | null;
  controlBusinessId: string | null;
  controlName: string | null;
  findingId: string | null;
  findingNumber: string | null;
  vendorReference: string | null;
  processingActivityReference: string | null;
  ownerUserId: string | null;
  ownerName: string | null;
  reviewerUserId: string | null;
  reviewerName: string | null;
  expiryDate: string | null;
  isExpired: boolean;
  approvedAt: string | null;
  rejectedAt: string | null;
  rejectionReason: string | null;
  archivedAt: string | null;
  createdAt: string;
  updatedAt: string;
  versions: EvidenceVersion[];
  reviews: EvidenceReviewRecord[];
}

export type PagedEvidence = PagedResult<EvidenceSummary>;

export interface UploadEvidencePayload {
  title: string;
  description?: string | null;
  evidenceType: string;
  assessmentId?: string | null;
  controlId?: string | null;
  findingId?: string | null;
  vendorReference?: string | null;
  processingActivityReference?: string | null;
  ownerUserId?: string | null;
  reviewerUserId?: string | null;
  expiryDate?: string | null;
  externalUrl?: string | null;
  file?: File | null;
}

export interface UpdateEvidencePayload {
  title: string;
  description?: string | null;
  vendorReference?: string | null;
  processingActivityReference?: string | null;
  ownerUserId?: string | null;
  reviewerUserId?: string | null;
  expiryDate?: string | null;
}
