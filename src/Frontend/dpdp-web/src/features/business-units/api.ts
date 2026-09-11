import { apiFetch } from "../../lib/apiClient";
import type { BusinessUnit, BusinessUnitPayload, PagedBusinessUnits } from "./types";

export function getBusinessUnits(page: number, pageSize: number, search?: string): Promise<PagedBusinessUnits> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (search) params.set("search", search);
  return apiFetch<PagedBusinessUnits>(`/api/v1/business-units?${params.toString()}`);
}

export function createBusinessUnit(organisationId: string, payload: BusinessUnitPayload): Promise<BusinessUnit> {
  return apiFetch<BusinessUnit>("/api/v1/business-units", {
    method: "POST",
    body: JSON.stringify({ organisationId, ...payload }),
  });
}

export function updateBusinessUnit(id: string, payload: BusinessUnitPayload & { isActive: boolean }): Promise<BusinessUnit> {
  return apiFetch<BusinessUnit>(`/api/v1/business-units/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function deleteBusinessUnit(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/business-units/${id}`, { method: "DELETE" });
}
