import { apiFetch } from "../../lib/apiClient";
import type {
  ClassificationSummary,
  CreateDataSourcePayload,
  DataAssetDetail,
  DataElement,
  DataSourceDetail,
  DiscoveryJob,
  DiscoveryJobDetail,
  PagedDataAssets,
  PagedDataElements,
  PagedDataSources,
  PagedDiscoveryJobs,
  TestConnectionResult,
  UpdateDataSourcePayload,
} from "./types";

export interface GetDataSourcesParams {
  page: number;
  pageSize: number;
  search?: string;
  sourceType?: string;
  isActive?: boolean;
}

export function getDataSources(params: GetDataSourcesParams): Promise<PagedDataSources> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.sourceType) query.set("sourceType", params.sourceType);
  if (params.isActive !== undefined) query.set("isActive", String(params.isActive));
  return apiFetch<PagedDataSources>(`/api/v1/data-sources?${query.toString()}`);
}

export function getDataSourceById(id: string): Promise<DataSourceDetail> {
  return apiFetch<DataSourceDetail>(`/api/v1/data-sources/${id}`);
}

export function createDataSource(payload: CreateDataSourcePayload): Promise<DataSourceDetail> {
  return apiFetch<DataSourceDetail>("/api/v1/data-sources", { method: "POST", body: JSON.stringify(payload) });
}

export function updateDataSource(id: string, payload: UpdateDataSourcePayload): Promise<DataSourceDetail> {
  return apiFetch<DataSourceDetail>(`/api/v1/data-sources/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function deleteDataSource(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/data-sources/${id}`, { method: "DELETE" });
}

export function testDataSourceConnection(id: string): Promise<TestConnectionResult> {
  return apiFetch<TestConnectionResult>(`/api/v1/data-sources/${id}/test-connection`, { method: "POST" });
}

export function startDiscoveryJob(dataSourceId: string): Promise<DiscoveryJob> {
  return apiFetch<DiscoveryJob>(`/api/v1/data-sources/${dataSourceId}/discovery-jobs`, { method: "POST" });
}

export interface GetDiscoveryJobsParams {
  page: number;
  pageSize: number;
  dataSourceId?: string;
  status?: string;
}

export function getDiscoveryJobs(params: GetDiscoveryJobsParams): Promise<PagedDiscoveryJobs> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.dataSourceId) query.set("dataSourceId", params.dataSourceId);
  if (params.status) query.set("status", params.status);
  return apiFetch<PagedDiscoveryJobs>(`/api/v1/discovery-jobs?${query.toString()}`);
}

export function getDiscoveryJobById(id: string): Promise<DiscoveryJobDetail> {
  return apiFetch<DiscoveryJobDetail>(`/api/v1/discovery-jobs/${id}`);
}

export function cancelDiscoveryJob(id: string): Promise<DiscoveryJob> {
  return apiFetch<DiscoveryJob>(`/api/v1/discovery-jobs/${id}/cancel`, { method: "POST" });
}

export interface GetDataAssetsParams {
  page: number;
  pageSize: number;
  search?: string;
  dataSourceId?: string;
  assetType?: string;
}

export function getDataAssets(params: GetDataAssetsParams): Promise<PagedDataAssets> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.dataSourceId) query.set("dataSourceId", params.dataSourceId);
  if (params.assetType) query.set("assetType", params.assetType);
  return apiFetch<PagedDataAssets>(`/api/v1/data-assets?${query.toString()}`);
}

export function getDataAssetById(id: string): Promise<DataAssetDetail> {
  return apiFetch<DataAssetDetail>(`/api/v1/data-assets/${id}`);
}

export interface GetDataElementsParams {
  page: number;
  pageSize: number;
  dataAssetId?: string;
  category?: string;
  unclassifiedOnly?: boolean;
  lowConfidenceOnly?: boolean;
}

export function getDataElements(params: GetDataElementsParams): Promise<PagedDataElements> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.dataAssetId) query.set("dataAssetId", params.dataAssetId);
  if (params.category) query.set("category", params.category);
  if (params.unclassifiedOnly) query.set("unclassifiedOnly", "true");
  if (params.lowConfidenceOnly) query.set("lowConfidenceOnly", "true");
  return apiFetch<PagedDataElements>(`/api/v1/data-elements?${query.toString()}`);
}

export function getClassificationSummary(): Promise<ClassificationSummary> {
  return apiFetch<ClassificationSummary>("/api/v1/data-elements/classification-summary");
}

export function updateDataElementClassification(id: string, category: string | null): Promise<DataElement> {
  return apiFetch<DataElement>(`/api/v1/data-elements/${id}/classification`, { method: "POST", body: JSON.stringify({ category }) });
}
