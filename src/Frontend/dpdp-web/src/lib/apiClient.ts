import { env } from "./env";

/**
 * crypto.randomUUID() only exists in secure contexts (HTTPS, or the
 * literal hostname "localhost") — plain HTTP over an IP or other hostname
 * doesn't have it. Fall back to a non-cryptographic UUID v4 in that case;
 * this id is only ever used for request correlation, never for security.
 */
function generateCorrelationId(): string {
  if (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function") {
    return crypto.randomUUID();
  }

  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

export class ApiError extends Error {
  readonly status: number;
  readonly correlationId: string | null;

  constructor(message: string, status: number, correlationId: string | null) {
    super(message);
    this.status = status;
    this.correlationId = correlationId;
  }
}

/**
 * Set once by AuthProvider at app startup. apiClient can't import the auth
 * store directly (the store's own api calls would be circular), so these
 * are injected instead — see features/auth/AuthProvider.tsx.
 */
let accessTokenGetter: () => string | null = () => null;
let unauthorizedHandler: () => void = () => {};

export function configureApiClient(options: {
  getAccessToken: () => string | null;
  onUnauthorized: () => void;
}) {
  accessTokenGetter = options.getAccessToken;
  unauthorizedHandler = options.onUnauthorized;
}

/**
 * Thin fetch wrapper: prefixes the API base URL, attaches the bearer
 * token, always sends a fresh correlation id, and turns a non-2xx
 * ProblemDetails response into an ApiError — see docs/API.md section 1.
 * A 401 triggers the shared "session expired" handler once, rather than
 * every caller having to special-case it.
 */
export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const correlationId = generateCorrelationId();
  const accessToken = accessTokenGetter();

  // A FormData body (multipart evidence upload) must NOT get an explicit
  // Content-Type — fetch sets multipart/form-data with the correct
  // boundary itself only when the header is left unset.
  const isFormData = typeof FormData !== "undefined" && init?.body instanceof FormData;

  const response = await fetch(`${env.apiBaseUrl}${path}`, {
    ...init,
    headers: {
      Accept: "application/json",
      "X-Correlation-Id": correlationId,
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      ...(init?.body && !isFormData ? { "Content-Type": "application/json" } : {}),
      ...init?.headers,
    },
  });

  if (response.status === 401) {
    unauthorizedHandler();
  }

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new ApiError(
      problem?.detail ?? problem?.title ?? `Request to ${path} failed with status ${response.status}`,
      response.status,
      problem?.correlationId ?? response.headers.get("X-Correlation-Id"),
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

/** Same wrapper, but for endpoints that stream a file (evidence download/preview) instead of JSON. */
export async function apiFetchBlob(path: string): Promise<Blob> {
  const correlationId = generateCorrelationId();
  const accessToken = accessTokenGetter();

  const response = await fetch(`${env.apiBaseUrl}${path}`, {
    headers: {
      "X-Correlation-Id": correlationId,
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
  });

  if (response.status === 401) {
    unauthorizedHandler();
  }

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new ApiError(
      problem?.detail ?? problem?.title ?? `Request to ${path} failed with status ${response.status}`,
      response.status,
      problem?.correlationId ?? response.headers.get("X-Correlation-Id"),
    );
  }

  return response.blob();
}
