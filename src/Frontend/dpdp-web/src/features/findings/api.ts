import { apiFetch } from "../../lib/apiClient";
import type { CreateFindingPayload, FindingDetail, PagedFindings, UpdateFindingPayload } from "./types";

export interface GetFindingsParams {
  page: number;
  pageSize: number;
  search?: string;
  status?: string;
  severity?: string;
  ownerUserId?: string;
  overdueOnly?: boolean;
}

export function getFindings(params: GetFindingsParams): Promise<PagedFindings> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.status) query.set("status", params.status);
  if (params.severity) query.set("severity", params.severity);
  if (params.ownerUserId) query.set("ownerUserId", params.ownerUserId);
  if (params.overdueOnly) query.set("overdueOnly", "true");
  return apiFetch<PagedFindings>(`/api/v1/findings?${query.toString()}`);
}

export function getFindingById(id: string): Promise<FindingDetail> {
  return apiFetch<FindingDetail>(`/api/v1/findings/${id}`);
}

export function createFinding(payload: CreateFindingPayload): Promise<FindingDetail> {
  return apiFetch<FindingDetail>("/api/v1/findings", { method: "POST", body: JSON.stringify(payload) });
}

export function createFindingFromAssessmentControl(assessmentControlId: string, recommendation?: string | null): Promise<FindingDetail> {
  return apiFetch<FindingDetail>("/api/v1/findings/from-assessment-control", {
    method: "POST",
    body: JSON.stringify({ assessmentControlId, recommendation }),
  });
}

export function updateFinding(id: string, payload: UpdateFindingPayload): Promise<FindingDetail> {
  return apiFetch<FindingDetail>(`/api/v1/findings/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function assignFinding(id: string, ownerUserId: string): Promise<FindingDetail> {
  return apiFetch<FindingDetail>(`/api/v1/findings/${id}/assign`, { method: "POST", body: JSON.stringify({ ownerUserId }) });
}

export function updateFindingStatus(id: string, status: string): Promise<FindingDetail> {
  return apiFetch<FindingDetail>(`/api/v1/findings/${id}/status`, { method: "POST", body: JSON.stringify({ status }) });
}

export function closeFinding(id: string): Promise<FindingDetail> {
  return apiFetch<FindingDetail>(`/api/v1/findings/${id}/close`, { method: "POST" });
}

export function acceptFindingRisk(id: string, comments: string): Promise<FindingDetail> {
  return apiFetch<FindingDetail>(`/api/v1/findings/${id}/accept-risk`, { method: "POST", body: JSON.stringify({ comments }) });
}

export function createRiskFromFinding(id: string, likelihood: string, impact: string, treatmentPlan?: string | null): Promise<FindingDetail> {
  return apiFetch<FindingDetail>(`/api/v1/findings/${id}/risk`, {
    method: "POST",
    body: JSON.stringify({ likelihood, impact, treatmentPlan }),
  });
}
