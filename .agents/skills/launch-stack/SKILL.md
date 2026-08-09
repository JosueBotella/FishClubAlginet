---
name: launch-stack
description: Levanta todo el stack local de FishClubAlginet (.NET API 10, React Frontend 19 / Vite y SQL Server 2022) mediante PowerShell dev.ps1 o Docker Compose con SonarQube y Portainer.
---

# Launch Stack Skill (FishClubAlginet)

Esta skill proporciona las instrucciones y comandos para levantar y gestionar el stack completo del proyecto FishClubAlginet.

## Modos de Ejecución

### 1. Desarrollo Local Directo (PowerShell)
Arranca el Backend (.NET API) y el Frontend (Vite) en paralelo en entorno local:

```powershell
.\dev.ps1
```

- **Frontend (Vite):** [http://localhost:5173](http://localhost:5173)
- **Backend (.NET API):** [https://localhost:7179](https://localhost:7179) (Scalar UI en `/scalar`)
- **Health Endpoint:** [https://localhost:7179/health](https://localhost:7179/health)

### 2. Stack Contenedorizado Completo + Herramientas (Docker Compose)
Levanta todo el stack contenedorizado con SQL Server, API, Frontend, SonarQube y Portainer:

```bash
docker compose -f docker-compose.yml -f docker-compose.tools.yml up -d
```

- **Frontend:** [http://localhost:5173](http://localhost:5173)
- **API Backend:** [http://localhost:5000](http://localhost:5000) (Scalar UI en `/scalar`)
- **Health Endpoint:** [http://localhost:5000/health](http://localhost:5000/health)
- **SQL Server 2022:** `localhost:1433` (Usuario: `sa`)
- **SonarQube (Calidad de Código):** [http://localhost:9000](http://localhost:9000)
- **Portainer (Gestión de Contenedores):** [http://localhost:19100](http://localhost:19100)
