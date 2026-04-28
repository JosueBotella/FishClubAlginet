import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider, ProtectedRoute } from './auth';
import { Routes as AppRoutes } from './constants';
import LoginPage from './pages/Login/LoginPage';
import HomePage from './pages/Home/HomePage';
import AppLayout from './layout/AppLayout';
import NotFoundPage from './pages/NotFound/NotFoundPage';
import AdminUsersPage from './pages/AdminUsers/AdminUsersPage';
import AdminFishermenPage from './pages/AdminFishermen/AdminFishermenPage';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Ruta publica - sin layout */}
          <Route path={AppRoutes.Login} element={<LoginPage />} />

          {/* Rutas protegidas - con layout (sidebar + header) */}
          <Route
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            <Route path={AppRoutes.Home} element={<HomePage />} />
            <Route path={AppRoutes.Profile} element={<ProfilePlaceholder />} />
            <Route
              path={AppRoutes.Users}
              element={
                <ProtectedRoute requiredRoles={['Admin']}>
                  <AdminUsersPage />
                </ProtectedRoute>
              }
            />
            <Route
              path={AppRoutes.Fishermen}
              element={
                <ProtectedRoute requiredRoles={['Admin']}>
                  <AdminFishermenPage />
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<NotFoundPage />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

/* Placeholder temporal - se reemplazara en futuras fases */
function ProfilePlaceholder() {
  return <div><h2>Mi perfil</h2><p>Pendiente de implementacion.</p></div>;
}

export default App;
