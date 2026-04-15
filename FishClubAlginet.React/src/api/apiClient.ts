import axios from 'axios';
import { StorageKeys } from '../constants';

export const apiClient = axios.create({
  baseURL: '/',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: adjunta el token JWT a cada petición
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem(StorageKeys.Token);
    if (token) {
      // El token se guarda serializado como JSON string desde Blazor,
      // así que lo parseamos por compatibilidad
      try {
        const parsed = JSON.parse(token);
        config.headers.Authorization = `Bearer ${parsed}`;
      } catch {
        // Si no es JSON válido, lo usamos tal cual
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor: si recibimos 401, limpiamos el token y redirigimos
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem(StorageKeys.Token);
      // Solo redirigimos si no estamos ya en /login
      if (!window.location.pathname.startsWith('/login')) {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);
