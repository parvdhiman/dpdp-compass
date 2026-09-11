import { apiFetch, apiFetchBlob } from "../../lib/apiClient";
import type { EvidenceDetail, PagedEvidence, UpdateEvidencePayload, UploadEvidencePayload } from "./types";

export interface GetEvidenceParams {
  page: number;
  pageSize: number;
  search?: string;
  status?: string;
  evidenceType?: string;
  ownerUserId?: string;
  reviewerUserId?: string;
  assessmentId?: string;
  controlId?: string;
  findingId?: string;
  expiredOnly?: boolean;
}

export function getEvidenceList(params: GetEvidenceParams): Promise<PagedEvidence> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.status) query.set("status", params.status);
  if (params.evidenceType) query.set("evidenceType", params.evidenceType);
  if (params.ownerUserId) query.set("ownerUserId", params.ownerUserId);
  if (params.reviewerUserId) query.set("reviewerUserId", params.reviewerUserId);
  if (params.assessmentId) query.set("assessmentId", params.assessmentId);
  if (params.controlId) query.set("controlId", params.controlId);
  if (params.findingId) query.set("findingId", params.findingId);
  if (params.expiredOnly) query.set("expiredOnly", "true");
  return apiFetch<PagedEvidence>(`/api/v1/evidence?${query.toString()}`);
}

export function getEvidenceById(id: string): Promise<EvidenceDetail> {
  return apiFetch<EvidenceDetail>(`/api/v1/evidence/${id}`);
}

function buildEvidenceFormData(payload: UploadEvidencePayload): FormData {
  const form = new FormData();
  form.set("title", payload.title);
  if (payload.description) form.set("description", payload.description);
  form.set("evidenceType", payload.evidenceType);
  if (payload.assessmentId) form.set("assessmentId", payload.assessmentId);
  if (payload.controlId) form.set("controlId", payload.controlId);
  if (payload.findingId) form.set("findingId", payload.findingId);
  if (payload.vendorReference) form.set("vendorReference", payload.vendorReference);
  if (payload.processingActivityReference) form.set("processingActivityReference", payload.processingActivityReference);
  if (payload.ownerUserId) form.set("ownerUserId", payload.ownerUserId);
  if (payload.reviewerUserId) form.set("reviewerUserId", payload.reviewerUserId);
  if (payload.expiryDate) form.set("expiryDate", payload.expiryDate);
  if (payload.externalUrl) form.set("externalUrl", payload.externalUrl);
  if (payload.file) form.set("file", payload.file);
  return form;
}

export function uploadEvidence(payload: UploadEvidencePayload): Promise<EvidenceDetail> {
  return apiFetch<EvidenceDetail>("/api/v1/evidence", { method: "POST", body: buildEvidenceFormData(payload) });
}

export function uploadEvidenceVersion(id: string, file?: File | null, externalUrl?: string | null): Promise<EvidenceDetail> {
  const form = new FormData();
  if (file) form.set("file", file);
  if (externalUrl) form.set("externalUrl", externalUrl);
  return apiFetch<EvidenceDetail>(`/api/v1/evidence/${id}/versions`, { method: "POST", body: form });
}

export function updateEvidence(id: string, payload: UpdateEvidencePayload): Promise<EvidenceDetail> {
  return apiFetch<EvidenceDetail>(`/api/v1/evidence/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function submitEvidenceForReview(id: string): Promise<EvidenceDetail> {
  return apiFetch<EvidenceDetail>(`/api/v1/evidence/${id}/submit`, { method: "POST" });
}

export function approveEvidence(id: string, comments?: string | null): Promise<EvidenceDetail> {
  return apiFetch<EvidenceDetail>(`/api/v1/evidence/${id}/approve`, { method: "POST", body: JSON.stringify({ comments }) });
}

export function rejectEvidence(id: string, rejectionReason: string): Promise<EvidenceDetail> {
  return apiFetch<EvidenceDetail>(`/api/v1/evidence/${id}/reject`, { method: "POST", body: JSON.stringify({ rejectionReason }) });
}

export function archiveEvidence(id: string): Promise<EvidenceDetail> {
  return apiFetch<EvidenceDetail>(`/api/v1/evidence/${id}/archive`, { method: "POST" });
}

export async function downloadEvidenceVersion(id: string, versionNumber: number, fileName: string): Promise<void> {
  const blob = await apiFetchBlob(`/api/v1/evidence/${id}/versions/${versionNumber}/download`);
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}

/**
 * Preview is an authenticated endpoint, so a plain <img src="..."> can't
 * carry the bearer token — fetch the blob ourselves and hand back an
 * object URL the caller can drop into <img>/<iframe src>, then revoke
 * once it's replaced/unmounted.
 */
export async function previewEvidenceVersion(id: string, versionNumber: number): Promise<string> {
  const blob = await apiFetchBlob(`/api/v1/evidence/${id}/versions/${versionNumber}/preview`);
  return URL.createObjectURL(blob);
}
