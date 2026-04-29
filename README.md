# 🎣 Sistema de Gestión de Competiciones de Pesca

Una plataforma robusta diseñada para la gestión integral de federaciones y clubes de pesca. Este proyecto implementa una arquitectura **Modular Monolith** basada en principios de **Clean Architecture**, asegurando mantenibilidad, escalabilidad y un alto rendimiento.

El sistema permite la administración de pescadores, licencias federativas, creación de ligas anuales y gestión de torneos, con una separación clara entre el panel de administración y el portal del pescador.

-----

## 🚀 Características Principales

### Arquitectura y Backend (.NET 10)
* **Clean Architecture:** Separación estricta de responsabilidades (Domain, Application, Infrastructure, API).
* **Seguridad Avanzada:** Sistema de Autenticación y Autorización basado en **ASP.NET Core Identity** y **JWT (JSON Web Tokens)**.
* **Roles y Permisos:** Gestión diferenciada para Administradores (gestión total) y Pescadores (perfil/rankings).
* **CQRS con MediatR:** Commands y Queries desacoplados, con FluentValidation automático por pipeline.
* **Patrones de Diseño:** *Generic Repository*, *Unit of Work*, *Outbox Pattern* y *Domain Events*.
* **Modelado de Dominio Rico:** Entidades con Factory Methods, *Value Objects* (ej. Address) y Domain Events.
* **Data Seeding:** Inicialización automática de roles, usuario administrador y datos de prueba.

### Frontend (React + TypeScript)
* **SPA con Vite:** Interfaz construida con **React 19 + TypeScript**, bundleada con Vite 6.
* **UI con Mantine v7:** Componentes de tabla, modal, formularios, notificaciones y paginación.
* **Autenticación JWT:** Interceptor Axios, almacenamiento de token y rutas protegidas por rol.
* **Proxy de desarrollo:** Vite redirige `/api/*` → `https://localhost:7179` sin necesidad de configurar CORS manualmente.

-----

## 📂 Estructura del Proyecto

La solución sigue la Regla de Dependencia, dividida en capas concéntricas:

### 1. FishClubAlginet.Core (Dominio)
*El núcleo del sistema. Contiene la lógica empresarial pura.*
* **Entidades:** `Pescador`, `Liga`, `Torneo`, `IdentityUser`.
* **Value Objects:** `Address` (Manejo de direcciones sin tablas redundantes).
* **Interfaces:** Contratos para Repositorios, Servicios de Auth y Unit of Work.
* **DTOs:** Objetos de transferencia de datos para Login, Registro y Vistas.

### 2. FishClubAlginet.Infrastructure (Infraestructura)
*La capa de implementación. Conecta con el mundo exterior.*
* **Persistencia:** `AppDbContext`, configuraciones de EF Core y Migraciones.
* **Identidad:** Implementación de servicios de autenticación y generación de Tokens.
* **Seeds:** Carga automática de datos iniciales (Usuarios Admin, Municipios).

### 3. FishClubAlginet.API (Presentación - Backend)
*El punto de entrada de datos.*
* **Controllers:** Endpoints RESTful protegidos por atributos `[Authorize]`.
* **Configuración:** Inyección de dependencias y configuración de CORS para Blazor.

### 4. fishclubalginet-frontend (Presentación - Frontend)
*La aplicación cliente React + TypeScript.*
* **Pages:** Vistas organizadas por módulos (`Login`, `AdminUsers`, `AdminFishermen`, `Home`).
* **API layer:** Clientes Axios por dominio (`authApi`, `usersApi`, `fishermenApi`) con interceptor JWT.
* **Routing:** React Router v7 con rutas protegidas por rol mediante `ProtectedRoute`.
* **Estado de Auth:** Context API (`AuthContext`) con decodificación de JWT y gestión de sesión.

-----

## 🚢 Arranque rápido con Docker (Recomendado)

Si solo quieres ver el proyecto funcionando, esta es la vía más rápida y la que evita instalar SDK de .NET, SQL Server o Node en local.

### Requisitos
- **Docker Desktop** (Windows / macOS) o Docker Engine + Compose (Linux). Asignar al menos **4 GB de RAM** a Docker (SQL Server requiere ~2 GB).
- En Mac con Apple Silicon (M1/M2/M3), la imagen de SQL Server 2022 funciona vía emulación amd64. Si notas mucha lentitud, considera usar `mcr.microsoft.com/azure-sql-edge` cambiando la imagen del servicio `db`.

### Pasos
```bash
# 1) Clonar
git clone https://github.com/JosueBotella/FishClubAlginet.git
cd FishClubAlginet

# 2) Variables de entorno (passwords y JWT secret)
cp .env.example .env       # En Windows: copy .env.example .env
# edita .env y pon contraseñas reales

# 3) Levantar todo (DB + API + Frontend)
docker compose up --build
```

Cuando termine de arrancar:
- Frontend: http://localhost:5173
- API:      http://localhost:5000 (Scalar API reference en `/scalar`)
- SQL Server expuesto en `localhost:1433` por si quieres conectar con SSMS / Azure Data Studio (usuario `sa`, password la del `.env`).

El primer arranque tarda ~1–2 min (descarga de imágenes + arranque inicial de SQL Server + migraciones EF + seed). Los siguientes son mucho más rápidos.

### Comandos útiles
| Comando | Para qué |
|---|---|
| `docker compose up` | Arrancar (sin rebuild si ya hay imágenes) |
| `docker compose up --build` | Rebuild forzado de imágenes |
| `docker compose down` | Parar y eliminar contenedores (mantiene la DB en volumen) |
| `docker compose down -v` | Parar y **borrar también la base de datos** |
| `docker compose logs -f api` | Ver logs en vivo del backend |
| `docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C` | Abrir consola SQL dentro del contenedor |

### Producción
Hay un compose separado optimizado (Nginx sirviendo el front, sin bind mounts, sin HMR):
```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```
El TLS (HTTPS) se asume gestionado por un reverse proxy externo (Cloudflare, Traefik, Caddy…) delante de este compose.

---

## 🛠️ Primeros Pasos (instalación manual, sin Docker)

### Requisitos Previos

| Herramienta | Versión mínima | Notas |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 | |
| [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) | 2019+ | LocalDB también funciona |
| [Node.js](https://nodejs.org/) | 20 LTS | Incluye npm |
| Visual Studio 2022 / VS Code | — | |

### Backend — Instalación y arranque

1. **Clonar el repositorio:**
    ```bash
    git clone https://github.com/JosueBotella/FishClubAlginet.git
    cd FishClubAlginet
    ```

2. **Configurar la cadena de conexión:**
    Edita `FishClubAlginet.API/appsettings.Development.json` y apunta `ConnectionStrings:DefaultConnection` a tu instancia de SQL Server Express.

3. **Aplicar migraciones y seeds:**
    ```bash
    dotnet ef database update --project FishClubAlginet.Infrastructure --startup-project FishClubAlginet.API
    ```
    El `DbInitializer` crea roles, usuario admin y datos de prueba en el primer arranque.

4. **Arrancar la API:**
    ```bash
    dotnet run --project FishClubAlginet.API
    ```
    La API queda disponible en `https://localhost:7179`.

### Frontend — Instalación y arranque

> La API debe estar corriendo antes de arrancar el frontend.

1. **Instalar dependencias** (solo la primera vez o tras cambios en `package.json`):
    ```bash
    cd fishclubalginet-frontend
    npm install
    ```

2. **Arrancar el servidor de desarrollo:**
    ```bash
    npm run dev
    ```
    Vite arranca en `http://localhost:5173`. Las llamadas a `/api/*` se redirigen automáticamente a `https://localhost:7179` mediante el proxy configurado en `vite.config.ts`.

3. **Otros comandos útiles:**

    | Comando | Descripción |
    |---|---|
    | `npm run build` | Compilación de producción (genera `dist/`) |
    | `npm run preview` | Sirve el build de producción localmente |
    | `npm run lint` | Linting con ESLint + TypeScript |

### Credenciales de desarrollo

El seed crea un usuario administrador por defecto. Consulta `FishClubAlginet.Infrastructure/Persistence/Seeds/` para ver el email y contraseña iniciales.

-----

## 📄 Licencia

Derechos de autor © 2026 Josué Botella. Todos los derechos reservados.

Este proyecto es propiedad intelectual de su creador. No se permite la reproducción, distribución o uso comercial del código fuente sin autorización expresa.
