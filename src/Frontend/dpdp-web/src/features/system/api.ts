import { apiFetch } from "../../lib/apiClient";

export interface SystemInfo {
  applicationName: string;
  version: string;
  environment: string;
  serverTimeUtc: string;
}

export function getSystemInfo(): Promise<SystemInfo> {
  return apiFetch<SystemInfo>("/api/v1/system/info");
}
