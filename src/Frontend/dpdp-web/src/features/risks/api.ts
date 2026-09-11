import { apiFetch } from "../../lib/apiClient";
import type { PagedRisks, RiskDetail, RiskPayload } from "./types";

export interface GetRisksParams {
  page: number;
  pageSize: number;
  search?: string;
  riskLevel?: string;
  status?: string;
}

export function getRisks(params: GetRisksParams): Promise<PagedRisks> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.riskLevel) query.set("riskLevel", params.riskLevel);
  if (params.status) query.set("status", params.status);
  return apiFetch<PagedRisks>(`/api/v1/risks?${query.toString()}`);
}

export function getRiskById(id: string): Promise<RiskDetail> {
  return apiFetch<RiskDetail>(`/api/v1/risks/${id}`);
}

export function createRisk(payload: RiskPayload): Promise<RiskDetail> {
  return apiFetch<RiskDetail>("/api/v1/risks", { method: "POST", body: JSON.stringify(payload) });
}

export function updateRisk(id: string, payload: RiskPayload & { status: string }): Promise<RiskDetail> {
  return apiFetch<RiskDetail>(`/api/v1/risks/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}
