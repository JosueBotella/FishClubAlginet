import {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { jwtDecode } from 'jwt-decode';
import { StorageKeys } from '../../constants';
import type { AuthState, AuthUser, JwtPayload } from '../../types';

/* ------------------------------------------------------------------ */
/*  Helpers (equivalente a GetClaimsFromToken / IsTokenExpired)        */
/* ------------------------------------------------------------------ */

function parseUserFromToken(token: string): AuthUser | null {
  try {
    const decoded = jwtDecode<JwtPayload>(token);

    // Verificar expiración
    if (decoded.exp * 1000 < Date.now()) return null;

    const roles = Array.isArray(decoded.role)
      ? decoded.role
      : decoded.role
        ? [decoded.role]
        : [];

    return {
      id: decoded.sub,
      email: decoded.email,
      roles,
    };
  } catch {
    return null;
  }
}

function readTokenFromStorage(): string | null {
  const raw = localStorage.getItem(StorageKeys.Token);
  if (!raw) return null;

  // Compatibilidad con Blazor: el token puede estar serializado como JSON string
  try {
    const parsed = JSON.parse(raw);
    return typeof parsed === 'string' ? parsed : null;
  } catch {
    return raw;
  }
}

/* ------------------------------------------------------------------ */
/*  Context                                                            */
/* ------------------------------------------------------------------ */

export interface AuthContextValue extends AuthState {
  login: (token: string) => void;
  logout: () => void;
  hasRole: (role: string) => boolean;
}

const defaultContext: AuthContextValue = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: true,
  login: () => {},
  logout: () => {},
  hasRole: () => false,
};

export const AuthContext = createContext<AuthContextValue>(defaultContext);

/* ------------------------------------------------------------------ */
/*  Provider                                                           */
/* ------------------------------------------------------------------ */

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    token: null,
    isAuthenticated: false,
    isLoading: true,
  });

  // Al montar: leer token de localStorage (equivalente a GetAuthenticationStateAsync)
  useEffect(() => {
    const token = readTokenFromStorage();
    if (token) {
      const user = parseUserFromToken(token);
      if (user) {
        setState({ user, token, isAuthenticated: true, isLoading: false });
        return;
      }
      // Token expirado o inválido → limpiar
      localStorage.removeItem(StorageKeys.Token);
    }
    setState((prev) => ({ ...prev, isLoading: false }));
  }, []);

  // Equivalente a LoginAsync
  const login = useCallback((token: string) => {
    localStorage.setItem(StorageKeys.Token, JSON.stringify(token));
    const user = parseUserFromToken(token);
    setState({
      user,
      token,
      isAuthenticated: !!user,
      isLoading: false,
    });
  }, []);

  // Equivalente a LogoutAsync
  const logout = useCallback(() => {
    localStorage.removeItem(StorageKeys.Token);
    setState({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
    });
  }, []);

  const hasRole = useCallback(
    (role: string) => state.user?.roles.includes(role) ?? false,
    [state.user]
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      ...state,
      login,
      logout,
      hasRole,
    }),
    [state, login, logout, hasRole]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
