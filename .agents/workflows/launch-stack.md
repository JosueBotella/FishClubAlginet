---
description: Levanta el stack completo de la aplicación (Backend .NET API, Frontend React/Vite, SQL Server / Docker) y herramientas de desarrollo como SonarQube.
---

# Launch Application Stack Workflow (/launch-stack)

Este workflow contiene las instrucciones y comandos paso a paso para levantar todo el proyecto FishClubAlginet, así como las herramientas de desarrollo y control de calidad de código con **SonarQube**.

## 1. Verificar Requisitos Previos

- Para **Desarrollo Local Directo**: Tener .NET 10 SDK, Node.js y PowerShell disponible.
- Para **Entorno Docker & SonarQube**: Comprobar que el motor de Docker está corriendo:
  ```bash
  docker info
  ```

---

## 2. Opciones de Arranque

### Opción A: Desarrollo Local Directo (PowerShell)

Ejecuta el script `dev.ps1` en la raíz del proyecto para arrancar en paralelo la API de .NET y el servidor Vite del Frontend:

```powershell
.\dev.ps1
```

- **Frontend (Vite):** [http://localhost:5173](http://localhost:5173)
- **Backend (.NET API):** [https://localhost:7179](https://localhost:7179) (Scalar API Docs en `/scalar`)
- **Detención:** Presionar `Ctrl+C` en la consola de PowerShell para detener ambos servicios automáticamente.

---

### Opción B: Stack Contenedorizado Completo (Docker Compose)

Para levantar SQL Server 2022 junto con la API y el Frontend en contenedores:

```bash
docker compose up -d
```

- **Monitorear arranque / migraciones y seeds:**
  ```bash
  docker compose logs -f api
  ```

---

### Opción C: Herramientas de Desarrollo y Calidad (SonarQube + Portainer)

Para levantar **SonarQube** (análisis de calidad de código) y **Portainer** (gestión de contenedores):

```bash
docker compose -f docker-compose.tools.yml up -d
```

- **SonarQube UI:** [http://localhost:9000](http://localhost:9000) *(Credenciales por defecto: `admin` / `admin`)*
- **Portainer UI:** [http://localhost:19100](http://localhost:19100)

---

## 3. Control de Calidad Obligatorio con SonarQube antes de subir cambios

Antes de realizar commits o push a remoto, se DEBE ejecutar el análisis de calidad para asegurar que no se introducen bugs, vulnerabilidades ni code smells:

```powershell
.\sonar-analysis.ps1 -Token "TU_TOKEN_DE_SONAR"
```

1. Acceder a [http://localhost:9000](http://localhost:9000).
2. Crear/Verificar el proyecto `FishClubAlginet` y generar un Token de usuario.
3. Ejecutar `.\sonar-analysis.ps1` antes de subir cambios para garantizar que el Quality Gate sea superado sin issues.

---

## 4. URLs y Puertos Expuestos

| Servicio | Puerto Host | URL / Acceso |
|---|---|---|
| **Frontend (Vite dev)** | `5173` | [http://localhost:5173](http://localhost:5173) |
| **API (.NET 10)** | `5000` (Docker) / `7179` (Local) | [http://localhost:5000](http://localhost:5000) (Scalar UI en `/scalar`) |
| **SQL Server 2022** | `1433` | `localhost,1433` (Usuario: `sa`) |
| **SonarQube (Calidad de Código)** | `9000` | [http://localhost:9000](http://localhost:9000) |
| **Portainer (Gestor Docker)** | `19100` | [http://localhost:19100](http://localhost:19100) |

---

## 5. Detener el Stack y Herramientas

- **Local:** `Ctrl+C` en el terminal de `dev.ps1`.
- **Docker Stack & Tools:**
  ```bash
  docker compose -f docker-compose.yml -f docker-compose.tools.yml down        # Mantiene volúmenes
  docker compose -f docker-compose.yml -f docker-compose.tools.yml down -v     # Elimina volúmenes
  ```
