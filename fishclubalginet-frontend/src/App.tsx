import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider, ProtectedRoute } from './auth';
import { Routes as AppRoutes } from './constants';
import LoginPage from './pages/Login/LoginPage';
import HomePage from './pages/Home/HomePage';
import AppLayout from './layout/AppLayout';
import NotFoundPage from './pages/NotFound/NotFoundPage';

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
            <Route path={AppRoutes.Users} element={<AdminUsersPlaceholder />} />
            <Route path={AppRoutes.Fishermen} element={<AdminFishermenPlaceholder />} />
            <Route path="*" element={<NotFoundPage />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

/* Placeholders temporales - se reemplazaran en futuras fases */
function ProfilePlaceholder() {
  return <div><h2>Mi perfil</h2><p>Pendiente de implementacion.</p></div>;
}

function AdminUsersPlaceholder() {
  return <div><h2>Gestion de usuarios</h2><p>Pendiente de implementacion.</p></div>;
}

function AdminFishermenPlaceholder() {
  return <div><h2>Gestion de pescadores</h2><p>Pendiente de implementacion.</p></div>;
}

export default App;
