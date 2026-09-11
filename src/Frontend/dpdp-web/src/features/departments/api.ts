import { apiFetch } from "../../lib/apiClient";
import type { Department, DepartmentPayload, PagedDepartments } from "./types";

export function getDepartments(
  page: number,
  pageSize: number,
  search?: string,
  businessUnitId?: string,
): Promise<PagedDepartments> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (search) params.set("search", search);
  if (businessUnitId) params.set("businessUnitId", businessUnitId);
  return apiFetch<PagedDepartments>(`/api/v1/departments?${params.toString()}`);
}

export function createDepartment(businessUnitId: string, payload: DepartmentPayload): Promise<Department> {
  return apiFetch<Department>("/api/v1/departments", {
    method: "POST",
    body: JSON.stringify({ businessUnitId, ...payload }),
  });
}

export function updateDepartment(id: string, payload: DepartmentPayload & { isActive: boolean }): Promise<Department> {
  return apiFetch<Department>(`/api/v1/departments/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function deleteDepartment(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/departments/${id}`, { method: "DELETE" });
}
