import type { PagedResult } from "../users/types";

export const CONSENT_CHANNELS = ["WEB", "MOBILE_APP", "EMAIL", "PHONE", "PAPER", "IN_PERSON", "API", "OTHER"] as const;
export const PRIVACY_NOTICE_STATUSES = ["DRAFT", "APPROVED", "PUBLISHED", "ARCHIVED"] as const;
export const CONSENT_STATUSES = ["GRANTED", "WITHDRAWN", "EXPIRED", "REVOKED"] as const;
export const DATA_PRINCIPAL_REQUEST_TYPES = ["ACCESS", "CORRECTION", "ERASURE", "WITHDRAW_CONSENT", "GRIEVANCE", "OTHER"] as const;
export const DATA_PRINCIPAL_REQUEST_STATUSES = [
  "REQUESTED", "IDENTITY_VERIFICATION", "IN_PROGRESS", "AWAITING_INFORMATION", "COMPLETED", "REJECTED", "CLOSED",
] as const;
export const DATA_SUBJECT_CATEGORIES = ["CUSTOMER", "EMPLOYEE", "VENDOR", "PROSPECT", "JOB_APPLICANT", "MINOR", "OTHER"] as const;

export interface DataPrincipal {
  id: string;
  externalReferenceId: string;
  referenceCategory: string | null;
  notes: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface ConsentPurpose {
  id: string;
  name: string;
  description: string | null;
  dataCategoryId: string | null;
  dataCategoryName: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface SlaPolicy {
  id: string;
  name: string;
  description: string | null;
  requestType: string | null;
  responseDueDays: number;
  isActive: boolean;
  createdAt: string;
}

export interface PrivacyNoticeSummary {
  id: string;
  noticeNumber: string;
  code: string;
  title: string;
  version: string;
  language: string;
  status: string;
  publishedDate: string | null;
  effectiveDate: string | null;
  createdAt: string;
}

export type PagedPrivacyNotices = PagedResult<PrivacyNoticeSummary>;

export interface DataCategoryRef {
  id: string;
  name: string;
}

export interface PrivacyNoticeDetail {
  id: string;
  noticeNumber: string;
  code: string;
  title: string;
  version: string;
  language: string;
  purpose: string;
  status: string;
  publishedDate: string | null;
  effectiveDate: string | null;
  approvedAt: string | null;
  publishedAt: string | null;
  archivedAt: string | null;
  createdAt: string;
  updatedAt: string;
  dataCategories: DataCategoryRef[];
}

export interface UpsertPrivacyNoticePayload {
  title: string;
  language: string;
  purpose: string;
  publishedDate?: string | null;
  effectiveDate?: string | null;
  dataCategoryIds: string[];
}

export interface CreatePrivacyNoticePayload extends UpsertPrivacyNoticePayload {
  code: string;
  version: string;
}

export interface ConsentRecord {
  id: string;
  consentNumber: string;
  dataPrincipalId: string;
  dataPrincipalReference: string;
  consentPurposeId: string;
  consentPurposeName: string;
  noticeVersionId: string | null;
  noticeVersionLabel: string | null;
  grantedAt: string;
  channel: string;
  status: string;
  withdrawnAt: string | null;
  expiresAt: string | null;
  sourceSystem: string | null;
  externalReferenceId: string | null;
  createdAt: string;
  updatedAt: string;
}

export type PagedConsentRecords = PagedResult<ConsentRecord>;

export interface CreateConsentPayload {
  dataPrincipalId: string;
  consentPurposeId: string;
  noticeVersionId?: string | null;
  grantedAt?: string | null;
  channel: string;
  expiresAt?: string | null;
  sourceSystem?: string | null;
  externalReferenceId?: string | null;
}

export interface DataPrincipalRequestSummary {
  id: string;
  requestNumber: string;
  requestType: string;
  status: string;
  requesterName: string;
  assignedToUserId: string | null;
  assignedToName: string | null;
  dueAt: string | null;
  isOverdue: boolean;
  createdAt: string;
}

export type PagedDataPrincipalRequests = PagedResult<DataPrincipalRequestSummary>;

export interface DataPrincipalRequestDetail {
  id: string;
  requestNumber: string;
  requestType: string;
  status: string;
  requesterName: string;
  requesterContactEmail: string | null;
  requesterContactPhone: string | null;
  externalReferenceId: string | null;
  dataPrincipalId: string | null;
  dataPrincipalReference: string | null;
  relatedConsentId: string | null;
  relatedConsentNumber: string | null;
  description: string | null;
  slaPolicyId: string | null;
  slaPolicyName: string | null;
  dueAt: string | null;
  isOverdue: boolean;
  identityVerifiedAt: string | null;
  identityVerifiedBy: string | null;
  assignedToUserId: string | null;
  assignedToName: string | null;
  resolvedAt: string | null;
  resolutionNotes: string | null;
  rejectedAt: string | null;
  rejectionReason: string | null;
  closedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDataPrincipalRequestPayload {
  requestType: string;
  requesterName: string;
  requesterContactEmail?: string | null;
  requesterContactPhone?: string | null;
  externalReferenceId?: string | null;
  dataPrincipalId?: string | null;
  relatedConsentId?: string | null;
  description?: string | null;
}

export interface SlaSummary {
  openCount: number;
  overdueCount: number;
  dueWithin48HoursCount: number;
  noSlaConfiguredCount: number;
}
