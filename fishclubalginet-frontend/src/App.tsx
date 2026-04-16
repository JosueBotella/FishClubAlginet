import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider, ProtectedRoute } from './auth';
import { Routes as AppRoutes } from './constants';
import LoginPage from './pages/Login/LoginPage';
import HomePage from './pages/Home/HomePage';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Ruta pública */}
          <Route path={AppRoutes.Login} element={<LoginPage />} />

          {/* Rutas protegidas */}
          <Route
            path={AppRoutes.Home}
            element={
              <ProtectedRoute>
                <HomePage />
              </ProtectedRoute>
            }
          />

          {/* TODO: añadir resto de rutas migradas aquí */}

          {/* Fallback 404 */}
          <Route
            path="*"
            element={
              <ProtectedRoute>
                <div style={{ padding: '2rem' }}>
                  <h2>Página no encontrada</h2>
                  <p>La ruta solicitada no existe.</p>
                </div>
              </ProtectedRoute>
            }
          />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
