/**
 * Typed, single point of access to build-time configuration. Never read
 * import.meta.env directly elsewhere — see docs/API.md and .env.example.
 *
 * Default API host: derived from the page's own hostname, not a hardcoded
 * "localhost". "localhost" in a fetch() call always means the *browser's*
 * machine — if this page is loaded from another host (LAN IP, SSH-tunnelled
 * hostname, etc.), a hardcoded "localhost" default silently points at the
 * wrong machine and every request fails with "Failed to fetch". Matching
 * the page's own hostname works for the common dev cases (localhost, a LAN
 * IP, a tunnelled hostname with both ports forwarded) without configuration.
 * VITE_API_BASE_URL always overrides this when explicitly set (Docker,
 * production).
 */
function defaultApiBaseUrl(): string {
  if (typeof window === "undefined") {
    return "http://localhost:5136";
  }

  return `${window.location.protocol}//${window.location.hostname}:5136`;
}

export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL || defaultApiBaseUrl(),
} as const;
