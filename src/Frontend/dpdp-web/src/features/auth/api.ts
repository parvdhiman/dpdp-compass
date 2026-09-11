import { apiFetch } from "../../lib/apiClient";
import type { AuthResult, MeDto } from "./types";

export function login(email: string, password: string): Promise<AuthResult> {
  return apiFetch<AuthResult>("/api/v1/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
}

export function refresh(refreshToken: string): Promise<AuthResult> {
  return apiFetch<AuthResult>("/api/v1/auth/refresh", {
    method: "POST",
    body: JSON.stringify({ refreshToken }),
  });
}

export function logout(refreshToken: string): Promise<void> {
  return apiFetch<void>("/api/v1/auth/logout", {
    method: "POST",
    body: JSON.stringify({ refreshToken }),
  });
}

export function getMe(): Promise<MeDto> {
  return apiFetch<MeDto>("/api/v1/auth/me");
}

export function changePassword(currentPassword: string, newPassword: string): Promise<void> {
  return apiFetch<void>("/api/v1/auth/change-password", {
    method: "POST",
    body: JSON.stringify({ currentPassword, newPassword }),
  });
}

export function forgotPassword(email: string): Promise<{ devOnlyResetToken: string | null }> {
  return apiFetch("/api/v1/auth/forgot-password", {
    method: "POST",
    body: JSON.stringify({ email }),
  });
}

export function resetPassword(token: string, newPassword: string): Promise<void> {
  return apiFetch<void>("/api/v1/auth/reset-password", {
    method: "POST",
    body: JSON.stringify({ token, newPassword }),
  });
}
