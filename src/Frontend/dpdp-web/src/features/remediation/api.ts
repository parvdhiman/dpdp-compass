import { apiFetch } from "../../lib/apiClient";
import type { CreateRemediationTaskPayload, EvidenceReference, PagedRemediationTasks, RemediationComment, RemediationTaskDetail } from "./types";

export interface GetRemediationTasksParams {
  page: number;
  pageSize: number;
  search?: string;
  status?: string;
  ownerUserId?: string;
  findingId?: string;
  overdueOnly?: boolean;
}

export function getRemediationTasks(params: GetRemediationTasksParams): Promise<PagedRemediationTasks> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.status) query.set("status", params.status);
  if (params.ownerUserId) query.set("ownerUserId", params.ownerUserId);
  if (params.findingId) query.set("findingId", params.findingId);
  if (params.overdueOnly) query.set("overdueOnly", "true");
  return apiFetch<PagedRemediationTasks>(`/api/v1/remediation-tasks?${query.toString()}`);
}

export function getRemediationTaskById(id: string): Promise<RemediationTaskDetail> {
  return apiFetch<RemediationTaskDetail>(`/api/v1/remediation-tasks/${id}`);
}

export function createRemediationTask(payload: CreateRemediationTaskPayload): Promise<RemediationTaskDetail> {
  return apiFetch<RemediationTaskDetail>("/api/v1/remediation-tasks", { method: "POST", body: JSON.stringify(payload) });
}

export function assignRemediationTask(id: string, ownerUserId: string): Promise<RemediationTaskDetail> {
  return apiFetch<RemediationTaskDetail>(`/api/v1/remediation-tasks/${id}/assign`, { method: "POST", body: JSON.stringify({ ownerUserId }) });
}

export function updateRemediationTaskStatus(id: string, status: string): Promise<RemediationTaskDetail> {
  return apiFetch<RemediationTaskDetail>(`/api/v1/remediation-tasks/${id}/status`, { method: "POST", body: JSON.stringify({ status }) });
}

export function addRemediationEvidence(id: string, evidence: EvidenceReference[]): Promise<RemediationTaskDetail> {
  return apiFetch<RemediationTaskDetail>(`/api/v1/remediation-tasks/${id}/evidence`, { method: "POST", body: JSON.stringify({ evidence }) });
}

export function addRemediationComment(id: string, comment: string): Promise<RemediationComment> {
  return apiFetch<RemediationComment>(`/api/v1/remediation-tasks/${id}/comments`, { method: "POST", body: JSON.stringify({ comment }) });
}

export function verifyRemediationTask(id: string, verificationNotes: string | null): Promise<RemediationTaskDetail> {
  return apiFetch<RemediationTaskDetail>(`/api/v1/remediation-tasks/${id}/verify`, { method: "POST", body: JSON.stringify({ verificationNotes }) });
}

export function closeRemediationTask(id: string): Promise<RemediationTaskDetail> {
  return apiFetch<RemediationTaskDetail>(`/api/v1/remediation-tasks/${id}/close`, { method: "POST" });
}
