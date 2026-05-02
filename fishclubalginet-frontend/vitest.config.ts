/// <reference types="vitest" />
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

// Configuración separada de Vite para no interferir con el dev server.
// Vite y Vitest leen sus propios configs cuando ambos existen.
export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    css: false, // No hace falta procesar CSS de Mantine en los tests
  },
});
