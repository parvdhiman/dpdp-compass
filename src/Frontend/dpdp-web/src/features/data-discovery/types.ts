import type { PagedResult } from "../users/types";

export const DATA_SOURCE_TYPES = ["POSTGRESQL", "MYSQL", "SQLSERVER", "FILE_SYSTEM"] as const;
export const DISCOVERY_JOB_STATUSES = ["PENDING", "RUNNING", "COMPLETED", "FAILED", "CANCELLED"] as const;
export const DATA_ASSET_TYPES = ["TABLE", "VIEW", "FILE"] as const;
export const CLASSIFICATION_CATEGORIES = [
  "IDENTIFIER", "CONTACT", "ADDRESS", "FINANCIAL", "IDENTITY", "EMPLOYEE",
  "CUSTOMER", "CHILD", "HEALTH_RELATED", "LOCATION", "OTHER_PERSONAL_DATA",
] as const;

export interface DataSourceSummary {
  id: string;
  name: string;
  sourceType: string;
  isActive: boolean;
  lastTestedAt: string | null;
  lastTestSucceeded: boolean | null;
  createdAt: string;
}

export interface DataSourceDetail {
  id: string;
  name: string;
  description: string | null;
  sourceType: string;
  host: string | null;
  port: number | null;
  databaseName: string | null;
  username: string | null;
  rootPath: string | null;
  schemaFilter: string | null;
  isActive: boolean;
  lastTestedAt: string | null;
  lastTestSucceeded: boolean | null;
  lastTestError: string | null;
  createdAt: string;
  updatedAt: string;
}

export type PagedDataSources = PagedResult<DataSourceSummary>;

export interface CreateDataSourcePayload {
  name: string;
  description?: string | null;
  sourceType: string;
  host?: string | null;
  port?: number | null;
  databaseName?: string | null;
  username?: string | null;
  password?: string | null;
  rootPath?: string | null;
  schemaFilter?: string | null;
}

export interface UpdateDataSourcePayload extends CreateDataSourcePayload {
  isActive: boolean;
}

export interface TestConnectionResult {
  succeeded: boolean;
  errorMessage: string | null;
}

export interface DiscoveryJob {
  id: string;
  dataSourceId: string;
  dataSourceName: string;
  status: string;
  triggeredByUserId: string;
  triggeredByName: string;
  startedAt: string | null;
  completedAt: string | null;
  errorMessage: string | null;
  assetsDiscoveredCount: number;
  elementsDiscoveredCount: number;
  createdAt: string;
}

export interface DiscoveryResult {
  id: string;
  dataAssetId: string;
  assetName: string;
  rowCountAtScan: number | null;
  columnsDiscovered: number;
  scannedAt: string;
}

export interface DiscoveryJobDetail extends DiscoveryJob {
  results: DiscoveryResult[];
}

export type PagedDiscoveryJobs = PagedResult<DiscoveryJob>;

export interface DataAssetSummary {
  id: string;
  dataSourceId: string;
  dataSourceName: string;
  databaseName: string | null;
  schemaName: string | null;
  assetName: string;
  assetType: string;
  estimatedRowCount: number | null;
  elementCount: number;
  personalDataElementCount: number;
  lastDiscoveredAt: string | null;
}

export type PagedDataAssets = PagedResult<DataAssetSummary>;

export interface DataElement {
  id: string;
  dataAssetId: string;
  assetName: string;
  columnName: string;
  dataType: string;
  isNullable: boolean;
  ordinalPosition: number;
  sampleMaskedValue: string | null;
  classificationCategory: string | null;
  classificationConfidence: number | null;
  classificationSource: string | null;
  isHumanCorrected: boolean;
  correctedByUserId: string | null;
  correctedByName: string | null;
  correctedAt: string | null;
  lastDiscoveredAt: string | null;
}

export interface DataAssetDetail {
  id: string;
  dataSourceId: string;
  dataSourceName: string;
  databaseName: string | null;
  schemaName: string | null;
  assetName: string;
  assetType: string;
  filePath: string | null;
  estimatedRowCount: number | null;
  indexes: string[];
  lastDiscoveredAt: string | null;
  elements: DataElement[];
}

export type PagedDataElements = PagedResult<DataElement>;

export interface ClassificationCategoryCount {
  category: string;
  count: number;
}

export interface ClassificationSummary {
  totalElements: number;
  unclassifiedCount: number;
  humanCorrectedCount: number;
  categoryCounts: ClassificationCategoryCount[];
}
