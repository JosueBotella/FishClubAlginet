import { Navigate, useLocation } from 'react-router-dom';
import { Center, Loader } from '@mantine/core';
import { useAuth } from '../../hooks';
import { Routes } from '../../constants';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
}

export default function ProtectedRoute({ children, requiredRoles }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, hasRole } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <Center h="100vh">
        <Loader size="lg" />
      </Center>
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
