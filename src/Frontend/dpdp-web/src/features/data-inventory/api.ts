import { apiFetch, apiFetchBlob } from "../../lib/apiClient";
import type {
  DataCategory, DataCollectionSource, DataFlow, DataInventoryItem, ItSystem,
  PagedDataFlows, PagedDataInventoryItems, PagedProcessingActivities,
  Processor, ProcessingActivityDetail, Recipient, RetentionPolicy,
  UpsertDataFlowPayload, UpsertDataInventoryItemPayload, UpsertProcessingActivityPayload,
} from "./types";

function buildCatalogApi<T>(path: string) {
  return {
    list: (search?: string, isActive?: boolean): Promise<T[]> => {
      const query = new URLSearchParams();
      if (search) query.set("search", search);
      if (isActive !== undefined) query.set("isActive", String(isActive));
      const qs = query.toString();
      return apiFetch<T[]>(`${path}${qs ? `?${qs}` : ""}`);
    },
    create: (payload: Record<string, unknown>): Promise<T> => apiFetch<T>(path, { method: "POST", body: JSON.stringify(payload) }),
    update: (id: string, payload: Record<string, unknown>): Promise<T> => apiFetch<T>(`${path}/${id}`, { method: "PUT", body: JSON.stringify(payload) }),
    remove: (id: string): Promise<void> => apiFetch<void>(`${path}/${id}`, { method: "DELETE" }),
  };
}

export const dataCategoriesApi = buildCatalogApi<DataCategory>("/api/v1/data-categories");
export const itSystemsApi = buildCatalogApi<ItSystem>("/api/v1/it-systems");
export const dataCollectionSourcesApi = buildCatalogApi<DataCollectionSource>("/api/v1/data-collection-sources");
export const processorsApi = buildCatalogApi<Processor>("/api/v1/processors");
export const recipientsApi = buildCatalogApi<Recipient>("/api/v1/recipients");
export const retentionPoliciesApi = buildCatalogApi<RetentionPolicy>("/api/v1/retention-policies");

export type GetDataInventoryItemsParams = {
  page: number;
  pageSize: number;
  search?: string;
  dataCategoryId?: string;
  itSystemId?: string;
  processorId?: string;
  classification?: string;
  riskLevel?: string;
};

function toQuery(params: Record<string, string | number | undefined>): string {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") query.set(key, String(value));
  }
  return query.toString();
}

export function getDataInventoryItems(params: GetDataInventoryItemsParams): Promise<PagedDataInventoryItems> {
  return apiFetch<PagedDataInventoryItems>(`/api/v1/data-inventory?${toQuery(params)}`);
}

export function getDataInventoryItemById(id: string): Promise<DataInventoryItem> {
  return apiFetch<DataInventoryItem>(`/api/v1/data-inventory/${id}`);
}

export function createDataInventoryItem(payload: UpsertDataInventoryItemPayload): Promise<DataInventoryItem> {
  return apiFetch<DataInventoryItem>("/api/v1/data-inventory", { method: "POST", body: JSON.stringify(payload) });
}

export function updateDataInventoryItem(id: string, payload: UpsertDataInventoryItemPayload): Promise<DataInventoryItem> {
  return apiFetch<DataInventoryItem>(`/api/v1/data-inventory/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function deleteDataInventoryItem(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/data-inventory/${id}`, { method: "DELETE" });
}

async function downloadCsv(url: string, fileName: string): Promise<void> {
  const blob = await apiFetchBlob(url);
  const objectUrl = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = objectUrl;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(objectUrl);
}

export function exportDataInventory(params: Omit<GetDataInventoryItemsParams, "page" | "pageSize">): Promise<void> {
  return downloadCsv(`/api/v1/data-inventory/export?${toQuery(params)}`, `data-inventory-${new Date().toISOString().slice(0, 10)}.csv`);
}

export type GetProcessingActivitiesParams = {
  page: number;
  pageSize: number;
  search?: string;
  status?: string;
  ownerUserId?: string;
  dataCategoryId?: string;
  itSystemId?: string;
};

export function getProcessingActivities(params: GetProcessingActivitiesParams): Promise<PagedProcessingActivities> {
  return apiFetch<PagedProcessingActivities>(`/api/v1/processing-activities?${toQuery(params)}`);
}

export function getProcessingActivityById(id: string): Promise<ProcessingActivityDetail> {
  return apiFetch<ProcessingActivityDetail>(`/api/v1/processing-activities/${id}`);
}

export function createProcessingActivity(payload: UpsertProcessingActivityPayload): Promise<ProcessingActivityDetail> {
  return apiFetch<ProcessingActivityDetail>("/api/v1/processing-activities", { method: "POST", body: JSON.stringify(payload) });
}

export function updateProcessingActivity(id: string, payload: UpsertProcessingActivityPayload): Promise<ProcessingActivityDetail> {
  return apiFetch<ProcessingActivityDetail>(`/api/v1/processing-activities/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function deleteProcessingActivity(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/processing-activities/${id}`, { method: "DELETE" });
}

export function submitProcessingActivityForReview(id: string): Promise<ProcessingActivityDetail> {
  return apiFetch<ProcessingActivityDetail>(`/api/v1/processing-activities/${id}/submit`, { method: "POST" });
}

export function approveProcessingActivity(id: string, reviewComments?: string | null): Promise<ProcessingActivityDetail> {
  return apiFetch<ProcessingActivityDetail>(`/api/v1/processing-activities/${id}/approve`, { method: "POST", body: JSON.stringify({ reviewComments }) });
}

export function sendProcessingActivityBackToDraft(id: string, reviewComments: string): Promise<ProcessingActivityDetail> {
  return apiFetch<ProcessingActivityDetail>(`/api/v1/processing-activities/${id}/send-back`, { method: "POST", body: JSON.stringify({ reviewComments }) });
}

export function archiveProcessingActivity(id: string): Promise<ProcessingActivityDetail> {
  return apiFetch<ProcessingActivityDetail>(`/api/v1/processing-activities/${id}/archive`, { method: "POST" });
}

export function reopenProcessingActivity(id: string): Promise<ProcessingActivityDetail> {
  return apiFetch<ProcessingActivityDetail>(`/api/v1/processing-activities/${id}/reopen`, { method: "POST" });
}

export function exportProcessingActivities(params: { search?: string; status?: string; ownerUserId?: string }): Promise<void> {
  return downloadCsv(`/api/v1/processing-activities/export?${toQuery(params)}`, `processing-activities-${new Date().toISOString().slice(0, 10)}.csv`);
}

export interface GetDataFlowsParams {
  page: number;
  pageSize: number;
  search?: string;
  processingActivityId?: string;
  crossBorderOnly?: boolean;
}

export function getDataFlows(params: GetDataFlowsParams): Promise<PagedDataFlows> {
  const query = toQuery({ page: params.page, pageSize: params.pageSize, search: params.search, processingActivityId: params.processingActivityId });
  const suffix = params.crossBorderOnly ? "&crossBorderOnly=true" : "";
  return apiFetch<PagedDataFlows>(`/api/v1/data-flows?${query}${suffix}`);
}

export function createDataFlow(payload: UpsertDataFlowPayload): Promise<DataFlow> {
  return apiFetch<DataFlow>("/api/v1/data-flows", { method: "POST", body: JSON.stringify(payload) });
}

export function updateDataFlow(id: string, payload: UpsertDataFlowPayload): Promise<DataFlow> {
  return apiFetch<DataFlow>(`/api/v1/data-flows/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function deleteDataFlow(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/data-flows/${id}`, { method: "DELETE" });
}
