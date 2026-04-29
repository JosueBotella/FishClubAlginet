# =====================================================
# Stage 1: Build de producción con Vite
# =====================================================
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

# Permite inyectar la URL pública de la API en build time si fuera necesario.
# Si tu API queda detrás del mismo Nginx (proxy /api/*), no hace falta.
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

# =====================================================
# Stage 2: Servir con Nginx
# =====================================================
FROM nginx:alpine AS runtime

# Sustituimos la config por defecto por la nuestra (SPA fallback + proxy /api)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Build estático de Vite
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
