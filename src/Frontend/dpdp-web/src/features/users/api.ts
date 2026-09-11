import { apiFetch } from "../../lib/apiClient";
import type { CreateUserPayload, PagedResult, UpdateUserPayload, UserDto } from "./types";

export function getUsers(page: number, pageSize: number, search?: string): Promise<PagedResult<UserDto>> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (search) params.set("search", search);
  return apiFetch<PagedResult<UserDto>>(`/api/v1/users?${params.toString()}`);
}

export function getUser(id: string): Promise<UserDto> {
  return apiFetch<UserDto>(`/api/v1/users/${id}`);
}

export function createUser(payload: CreateUserPayload): Promise<UserDto> {
  return apiFetch<UserDto>("/api/v1/users", { method: "POST", body: JSON.stringify(payload) });
}

export function updateUser(id: string, payload: UpdateUserPayload): Promise<UserDto> {
  return apiFetch<UserDto>(`/api/v1/users/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function activateUser(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/users/${id}/activate`, { method: "POST" });
}

export function deactivateUser(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/users/${id}/deactivate`, { method: "POST" });
}

export function assignUserRole(userId: string, roleId: string): Promise<void> {
  return apiFetch<void>(`/api/v1/users/${userId}/roles/${roleId}`, { method: "POST" });
}

export function revokeUserRole(userId: string, roleId: string): Promise<void> {
  return apiFetch<void>(`/api/v1/users/${userId}/roles/${roleId}`, { method: "DELETE" });
}
