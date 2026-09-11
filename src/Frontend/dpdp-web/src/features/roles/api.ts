import { apiFetch } from "../../lib/apiClient";
import type { PermissionDto, RoleDto } from "./types";

export function getRoles(): Promise<RoleDto[]> {
  return apiFetch<RoleDto[]>("/api/v1/roles");
}

export function getPermissions(): Promise<PermissionDto[]> {
  return apiFetch<PermissionDto[]>("/api/v1/permissions");
}

export function assignRolePermission(roleId: string, permissionId: string): Promise<void> {
  return apiFetch<void>(`/api/v1/roles/${roleId}/permissions/${permissionId}`, { method: "POST" });
}

export function revokeRolePermission(roleId: string, permissionId: string): Promise<void> {
  return apiFetch<void>(`/api/v1/roles/${roleId}/permissions/${permissionId}`, { method: "DELETE" });
}
