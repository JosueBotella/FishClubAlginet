import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  // Carga variables de .env, .env.[mode] y las del entorno del proceso
  const env = loadEnv(mode, process.cwd(), '')

  // Target del proxy: dentro de Docker -> http://api:8080
  // En local sin Docker -> https://localhost:7179 (kestrel con cert dev)
  const proxyTarget = env.VITE_PROXY_TARGET ?? 'https://localhost:7179'

  return {
    plugins: [react()],
    server: {
      host: '0.0.0.0',
      port: 5173,
      proxy: {
        '/api': {
          target: proxyTarget,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  }
})
