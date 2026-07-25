# Launch Application Stack Workflow

Workflow para guiar e iniciar el stack completo de la aplicación FishClubAlginet.

## 1. Seleccionar Entorno de Arranque

Determinar si el usuario o la tarea requiere arranque **local** o **contenedorizado en Docker**.

### Modo A: Desarrollo Local Directo (PowerShell)
Ejecutar el script que inicia el backend y frontend localmente:
```powershell
.\dev.ps1
```

### Modo B: Entorno Contenedorizado (Docker Compose)
Para entornos de staging o pruebas integradas con SQL Server en contenedor:
```bash
docker compose up -d
docker compose logs -f api
```

## 2. Verificar Disponibilidad de Servicios

Comprobar que los puertos responden correctamente:
- **Frontend (Vite):** `http://localhost:5173`
- **Backend API:** `https://localhost:7179` (Local) o `http://localhost:5000` (Docker) / Documentation: `/scalar`
- **SQL Server:** `localhost:1433`
