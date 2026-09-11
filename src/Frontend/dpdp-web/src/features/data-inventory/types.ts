import type { PagedResult } from "../users/types";

export const DATA_SUBJECT_CATEGORIES = ["CUSTOMER", "EMPLOYEE", "VENDOR", "PROSPECT", "JOB_APPLICANT", "MINOR", "OTHER"] as const;
export const IT_SYSTEM_TYPES = ["APPLICATION", "DATABASE", "SAAS", "FILE_STORE", "OTHER"] as const;
export const DATA_COLLECTION_SOURCE_TYPES = ["WEB_FORM", "MOBILE_APP", "API", "PHONE", "EMAIL", "IN_PERSON", "THIRD_PARTY", "OTHER"] as const;
export const RECIPIENT_TYPES = ["INTERNAL", "AFFILIATE", "THIRD_PARTY", "GOVERNMENT", "OTHER"] as const;
export const RETENTION_PERIOD_UNITS = ["DAYS", "MONTHS", "YEARS"] as const;
export const CLASSIFICATION_CATEGORIES = [
  "IDENTIFIER", "CONTACT", "ADDRESS", "FINANCIAL", "IDENTITY", "EMPLOYEE",
  "CUSTOMER", "CHILD", "HEALTH_RELATED", "LOCATION", "OTHER_PERSONAL_DATA",
] as const;
export const RISK_LEVELS = ["LOW", "MEDIUM", "HIGH", "CRITICAL"] as const;
export const PROCESSING_ACTIVITY_STATUSES = ["DRAFT", "IN_REVIEW", "APPROVED", "ARCHIVED"] as const;

export interface CatalogEntry {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface DataCategory extends CatalogEntry {
  classificationCategory: string | null;
}

export interface ItSystem extends CatalogEntry {
  systemType: string;
  ownerUserId: string | null;
  ownerName: string | null;
}

export interface DataCollectionSource extends CatalogEntry {
  sourceType: string;
}

export interface Processor extends CatalogEntry {
  contactEmail: string | null;
  country: string | null;
}

export interface Recipient extends CatalogEntry {
  recipientType: string;
}

export interface RetentionPolicy extends CatalogEntry {
  retentionPeriodValue: number;
  retentionPeriodUnit: string;
  triggerEvent: string | null;
}

export interface DataInventoryItem {
  id: string;
  itemNumber: string;
  dataCategoryId: string;
  dataCategoryName: string;
  dataElementName: string;
  discoveredDataElementId: string | null;
  classification: string | null;
  dataCollectionSourceId: string | null;
  dataCollectionSourceName: string | null;
  itSystemId: string | null;
  itSystemName: string | null;
  ownerUserId: string | null;
  ownerName: string | null;
  purpose: string | null;
  retentionPolicyId: string | null;
  retentionPolicyName: string | null;
  sharingDescription: string | null;
  processorId: string | null;
  processorName: string | null;
  riskLevel: string | null;
  createdAt: string;
  updatedAt: string;
}

export type PagedDataInventoryItems = PagedResult<DataInventoryItem>;

export interface UpsertDataInventoryItemPayload {
  dataCategoryId: string;
  dataElementName: string;
  discoveredDataElementId?: string | null;
  classification?: string | null;
  dataCollectionSourceId?: string | null;
  itSystemId?: string | null;
  ownerUserId?: string | null;
  purpose?: string | null;
  retentionPolicyId?: string | null;
  sharingDescription?: string | null;
  processorId?: string | null;
  riskLevel?: string | null;
}

export interface ProcessingActivitySummary {
  id: string;
  activityNumber: string;
  name: string;
  status: string;
  ownerUserId: string | null;
  ownerName: string | null;
  reviewDate: string | null;
  dataCategoryCount: number;
  createdAt: string;
}

export type PagedProcessingActivities = PagedResult<ProcessingActivitySummary>;

export interface ProcessingActivityDetail {
  id: string;
  activityNumber: string;
  name: string;
  purpose: string;
  dataSubjectCategories: string[];
  securityControls: string[];
  retentionPolicyId: string | null;
  retentionPolicyName: string | null;
  ownerUserId: string | null;
  ownerName: string | null;
  status: string;
  reviewDate: string | null;
  submittedForReviewAt: string | null;
  reviewedAt: string | null;
  reviewComments: string | null;
  approvedAt: string | null;
  archivedAt: string | null;
  createdAt: string;
  updatedAt: string;
  dataCategories: DataCategory[];
  itSystems: ItSystem[];
  dataCollectionSources: DataCollectionSource[];
  recipients: Recipient[];
  processors: Processor[];
}

export interface UpsertProcessingActivityPayload {
  name: string;
  purpose: string;
  dataSubjectCategories: string[];
  securityControls: string[];
  retentionPolicyId?: string | null;
  ownerUserId?: string | null;
  reviewDate?: string | null;
  dataCategoryIds: string[];
  itSystemIds: string[];
  dataCollectionSourceIds: string[];
  recipientIds: string[];
  processorIds: string[];
}

export interface DataFlow {
  id: string;
  flowNumber: string;
  name: string;
  description: string | null;
  processingActivityId: string | null;
  processingActivityName: string | null;
  dataCategoryId: string | null;
  dataCategoryName: string | null;
  fromItSystemId: string | null;
  fromItSystemName: string | null;
  fromDataCollectionSourceId: string | null;
  fromDataCollectionSourceName: string | null;
  fromDescription: string;
  toItSystemId: string | null;
  toItSystemName: string | null;
  toProcessorId: string | null;
  toProcessorName: string | null;
  toRecipientId: string | null;
  toRecipientName: string | null;
  toDescription: string;
  transferMechanism: string | null;
  isCrossBorder: boolean;
  crossBorderCountry: string | null;
  createdAt: string;
  updatedAt: string;
}

export type PagedDataFlows = PagedResult<DataFlow>;

export interface UpsertDataFlowPayload {
  name: string;
  description?: string | null;
  processingActivityId?: string | null;
  dataCategoryId?: string | null;
  fromItSystemId?: string | null;
  fromDataCollectionSourceId?: string | null;
  fromDescription: string;
  toItSystemId?: string | null;
  toProcessorId?: string | null;
  toRecipientId?: string | null;
  toDescription: string;
  transferMechanism?: string | null;
  isCrossBorder: boolean;
  crossBorderCountry?: string | null;
}
