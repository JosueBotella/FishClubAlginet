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

function parseUserFromToken(token: string): AuthUser | null {
  try {
    const decoded = jwtDecode<JwtPayload>(token);
    if (decoded.exp * 1000 < Date.now()) return null;

    const roles = Array.isArray(decoded.role)
      ? decoded.role
      : decoded.role
        ? [decoded.role]
        : [];

    return { id: decoded.sub, email: decoded.email, roles };
  } catch {
    return null;
  }
}

function readTokenFromStorage(): string | null {
  const raw = localStorage.getItem(StorageKeys.Token);
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw);
    return typeof parsed === 'string' ? parsed : null;
  } catch {
    return raw;
  }
}

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

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    token: null,
    isAuthenticated: false,
    isLoading: true,
  });

  useEffect(() => {
    const token = readTokenFromStorage();
    if (token) {
      const user = parseUserFromToken(token);
      if (user) {
        setState({ user, token, isAuthenticated: true, isLoading: false });
        return;
      }
      localStorage.removeItem(StorageKeys.Token);
    }
    setState((prev) => ({ ...prev, isLoading: false }));
  }, []);

  const login = useCallback((token: string) => {
    localStorage.setItem(StorageKeys.Token, JSON.stringify(token));
    const user = parseUserFromToken(token);
    setState({ user, token, isAuthenticated: !!user, isLoading: false });
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(StorageKeys.Token);
    setState({ user: null, token: null, isAuthenticated: false, isLoading: false });
  }, []);

  const hasRole = useCallback(
    (role: string) => state.user?.roles.includes(role) ?? false,
    [state.user]
  );

  const value = useMemo<AuthContextValue>(
    () => ({ ...state, login, logout, hasRole }),
    [state, login, logout, hasRole]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
