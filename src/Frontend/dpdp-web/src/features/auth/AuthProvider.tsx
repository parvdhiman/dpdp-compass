import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { configureApiClient } from "../../lib/apiClient";
import * as authApi from "./api";
import type { MeDto } from "./types";

const REFRESH_TOKEN_STORAGE_KEY = "dpdp.refreshToken";

/**
 * Access token lives only in memory (React state) — never persisted.
 * Refresh token is persisted to localStorage so a page reload doesn't
 * force a fresh login; this is a deliberate trade-off (an XSS payload
 * could read it) accepted for Module 2 given there is no same-site
 * backend-for-frontend to hold an HttpOnly cookie instead. See
 * docs/ARCHITECTURE.md section 11 and the Module 2 completion report.
 */
interface AuthContextValue {
  user: MeDto | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<MeDto>;
  logout: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<MeDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const accessTokenRef = useRef<string | null>(null);
  const refreshTokenRef = useRef<string | null>(null);

  const clearSession = useCallback(() => {
    accessTokenRef.current = null;
    refreshTokenRef.current = null;
    localStorage.removeItem(REFRESH_TOKEN_STORAGE_KEY);
    setUser(null);
  }, []);

  useEffect(() => {
    configureApiClient({
      getAccessToken: () => accessTokenRef.current,
      onUnauthorized: clearSession,
    });
  }, [clearSession]);

  useEffect(() => {
    const storedRefreshToken = localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY);
    if (!storedRefreshToken) {
      setIsLoading(false);
      return;
    }

    authApi
      .refresh(storedRefreshToken)
      .then((result) => {
        accessTokenRef.current = result.accessToken;
        refreshTokenRef.current = result.refreshToken;
        localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, result.refreshToken);
        setUser(result.user);
      })
      .catch(() => {
        localStorage.removeItem(REFRESH_TOKEN_STORAGE_KEY);
      })
      .finally(() => setIsLoading(false));
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const result = await authApi.login(email, password);
    accessTokenRef.current = result.accessToken;
    refreshTokenRef.current = result.refreshToken;
    localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, result.refreshToken);
    setUser(result.user);
    return result.user;
  }, []);

  const logout = useCallback(async () => {
    const currentRefreshToken = refreshTokenRef.current;
    clearSession();
    if (currentRefreshToken) {
      await authApi.logout(currentRefreshToken).catch(() => {
        // Best-effort — the session is already cleared client-side either way.
      });
    }
  }, [clearSession]);

  const hasPermission = useCallback(
    (permission: string) => user?.isSuperAdministrator === true || (user?.permissions.includes(permission) ?? false),
    [user],
  );

  return (
    <AuthContext.Provider value={{ user, isLoading, login, logout, hasPermission }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
