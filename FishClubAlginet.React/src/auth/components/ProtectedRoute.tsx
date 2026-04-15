import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks';
import { Routes } from '../../constants';

interface ProtectedRouteProps {
  children: React.ReactNode;
  /** Si se indica, el usuario debe tener al menos uno de estos roles */
  requiredRoles?: string[];
}

/**
 * Equivalente a <AuthorizeRouteView> + <RedirectToLogin /> de Blazor.
 *
 * - Si isLoading → spinner (equivalente a <Authorizing>)
 * - Si no autenticado → redirige a /login con returnUrl
 * - Si faltan roles → redirige a /  (o podrías mostrar un 403)
 */
export default function ProtectedRoute({ children, requiredRoles }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, hasRole } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', marginTop: '3rem' }}>
        <div className="spinner" role="status">
          <span style={{ display: 'none' }}>Verifying session...</span>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    const returnUrl = encodeURIComponent(location.pathname + location.search);
    return <Navigate to={`${Routes.Login}?returnUrl=${returnUrl}`} replace />;
  }

  if (requiredRoles?.length && !requiredRoles.some(hasRole)) {
    return <Navigate to={Routes.Home} replace />;
  }

  return <>{children}</>;
}
