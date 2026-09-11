import { apiFetch } from "../../lib/apiClient";
import type {
  ConsentPurpose, ConsentRecord, CreateConsentPayload, CreateDataPrincipalRequestPayload, CreatePrivacyNoticePayload,
  DataPrincipal, DataPrincipalRequestDetail, PagedConsentRecords, PagedDataPrincipalRequests, PagedPrivacyNotices,
  PrivacyNoticeDetail, SlaPolicy, SlaSummary, UpsertPrivacyNoticePayload,
} from "./types";

// GenericCatalogManager's form defaults every field to "" until touched; optional enum
// selects (SlaPolicy.requestType, DataPrincipal.referenceCategory) must reach the API as
// null, not "", or FluentValidation's `v is null || Enum.TryParse(v)` rule rejects it.
function nullifyEmptyStrings(payload: Record<string, unknown>): Record<string, unknown> {
  return Object.fromEntries(Object.entries(payload).map(([key, value]) => [key, value === "" ? null : value]));
}

function buildCatalogApi<T>(path: string) {
  return {
    list: (search?: string, isActive?: boolean): Promise<T[]> => {
      const query = new URLSearchParams();
      if (search) query.set("search", search);
      if (isActive !== undefined) query.set("isActive", String(isActive));
      const qs = query.toString();
      return apiFetch<T[]>(`${path}${qs ? `?${qs}` : ""}`);
    },
    create: (payload: Record<string, unknown>): Promise<T> => apiFetch<T>(path, { method: "POST", body: JSON.stringify(nullifyEmptyStrings(payload)) }),
    update: (id: string, payload: Record<string, unknown>): Promise<T> => apiFetch<T>(`${path}/${id}`, { method: "PUT", body: JSON.stringify(nullifyEmptyStrings(payload)) }),
    remove: (id: string): Promise<void> => apiFetch<void>(`${path}/${id}`, { method: "DELETE" }),
  };
}

export const dataPrincipalsApi = buildCatalogApi<DataPrincipal>("/api/v1/data-principals");
export const consentPurposesApi = buildCatalogApi<ConsentPurpose>("/api/v1/consent-purposes");
export const slaPoliciesApi = buildCatalogApi<SlaPolicy>("/api/v1/sla-policies");

function toQuery(params: Record<string, string | number | boolean | undefined>): string {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") query.set(key, String(value));
  }
  return query.toString();
}

export type GetPrivacyNoticesParams = { page: number; pageSize: number; search?: string; status?: string; code?: string };

export function getPrivacyNotices(params: GetPrivacyNoticesParams): Promise<PagedPrivacyNotices> {
  return apiFetch<PagedPrivacyNotices>(`/api/v1/privacy-notices?${toQuery(params)}`);
}

export function getPrivacyNoticeById(id: string): Promise<PrivacyNoticeDetail> {
  return apiFetch<PrivacyNoticeDetail>(`/api/v1/privacy-notices/${id}`);
}

export function createPrivacyNotice(payload: CreatePrivacyNoticePayload): Promise<PrivacyNoticeDetail> {
  return apiFetch<PrivacyNoticeDetail>("/api/v1/privacy-notices", { method: "POST", body: JSON.stringify(payload) });
}

export function updatePrivacyNotice(id: string, payload: UpsertPrivacyNoticePayload): Promise<PrivacyNoticeDetail> {
  return apiFetch<PrivacyNoticeDetail>(`/api/v1/privacy-notices/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function deletePrivacyNotice(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/privacy-notices/${id}`, { method: "DELETE" });
}

export function approvePrivacyNotice(id: string): Promise<PrivacyNoticeDetail> {
  return apiFetch<PrivacyNoticeDetail>(`/api/v1/privacy-notices/${id}/approve`, { method: "POST" });
}

export function publishPrivacyNotice(id: string): Promise<PrivacyNoticeDetail> {
  return apiFetch<PrivacyNoticeDetail>(`/api/v1/privacy-notices/${id}/publish`, { method: "POST" });
}

export function archivePrivacyNotice(id: string): Promise<PrivacyNoticeDetail> {
  return apiFetch<PrivacyNoticeDetail>(`/api/v1/privacy-notices/${id}/archive`, { method: "POST" });
}

export type GetConsentRecordsParams = { page: number; pageSize: number; status?: string; dataPrincipalId?: string; consentPurposeId?: string };

export function getConsentRecords(params: GetConsentRecordsParams): Promise<PagedConsentRecords> {
  return apiFetch<PagedConsentRecords>(`/api/v1/consent-records?${toQuery(params)}`);
}

export function createConsentRecord(payload: CreateConsentPayload): Promise<ConsentRecord> {
  return apiFetch<ConsentRecord>("/api/v1/consent-records", { method: "POST", body: JSON.stringify(payload) });
}

export function withdrawConsent(id: string): Promise<ConsentRecord> {
  return apiFetch<ConsentRecord>(`/api/v1/consent-records/${id}/withdraw`, { method: "POST" });
}

export function revokeConsent(id: string, reason: string): Promise<ConsentRecord> {
  return apiFetch<ConsentRecord>(`/api/v1/consent-records/${id}/revoke`, { method: "POST", body: JSON.stringify({ reason }) });
}

export function markConsentExpired(): Promise<{ expiredCount: number }> {
  return apiFetch<{ expiredCount: number }>("/api/v1/consent-records/mark-expired", { method: "POST" });
}

export type GetDataPrincipalRequestsParams = {
  page: number;
  pageSize: number;
  status?: string;
  requestType?: string;
  assignedToUserId?: string;
  overdueOnly?: boolean;
};

export function getDataPrincipalRequests(params: GetDataPrincipalRequestsParams): Promise<PagedDataPrincipalRequests> {
  return apiFetch<PagedDataPrincipalRequests>(`/api/v1/data-principal-requests?${toQuery(params)}`);
}

export function getDataPrincipalRequestById(id: string): Promise<DataPrincipalRequestDetail> {
  return apiFetch<DataPrincipalRequestDetail>(`/api/v1/data-principal-requests/${id}`);
}

export function getDataPrincipalRequestSlaSummary(): Promise<SlaSummary> {
  return apiFetch<SlaSummary>("/api/v1/data-principal-requests/sla-summary");
}

export function createDataPrincipalRequest(payload: CreateDataPrincipalRequestPayload): Promise<DataPrincipalRequestDetail> {
  return apiFetch<DataPrincipalRequestDetail>("/api/v1/data-principal-requests", { method: "POST", body: JSON.stringify(payload) });
}

export function deleteDataPrincipalRequest(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/data-principal-requests/${id}`, { method: "DELETE" });
}

export function assignDataPrincipalRequest(id: string, assignedToUserId: string): Promise<DataPrincipalRequestDetail> {
  return apiFetch<DataPrincipalRequestDetail>(`/api/v1/data-principal-requests/${id}/assign`, { method: "POST", body: JSON.stringify({ assignedToUserId }) });
}

export function verifyDataPrincipalRequestIdentity(id: string, matchedDataPrincipalId: string | null): Promise<DataPrincipalRequestDetail> {
  return apiFetch<DataPrincipalRequestDetail>(`/api/v1/data-principal-requests/${id}/verify-identity`, { method: "POST", body: JSON.stringify({ matchedDataPrincipalId }) });
}

export function updateDataPrincipalRequestStatus(id: string, status: string): Promise<DataPrincipalRequestDetail> {
  return apiFetch<DataPrincipalRequestDetail>(`/api/v1/data-principal-requests/${id}/status`, { method: "POST", body: JSON.stringify({ status }) });
}

export function completeDataPrincipalRequest(id: string, resolutionNotes: string | null): Promise<DataPrincipalRequestDetail> {
  return apiFetch<DataPrincipalRequestDetail>(`/api/v1/data-principal-requests/${id}/complete`, { method: "POST", body: JSON.stringify({ resolutionNotes }) });
}

export function rejectDataPrincipalRequest(id: string, rejectionReason: string): Promise<DataPrincipalRequestDetail> {
  return apiFetch<DataPrincipalRequestDetail>(`/api/v1/data-principal-requests/${id}/reject`, { method: "POST", body: JSON.stringify({ rejectionReason }) });
}

export function closeDataPrincipalRequest(id: string): Promise<DataPrincipalRequestDetail> {
  return apiFetch<DataPrincipalRequestDetail>(`/api/v1/data-principal-requests/${id}/close`, { method: "POST" });
}
