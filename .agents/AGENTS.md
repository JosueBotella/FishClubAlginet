<!-- STACK_LAUNCH_RULE_START -->
## Regla de Arranque del Stack del Aplicativo (FishClubAlginet)

Esta regla define los comandos y procedimientos estándar para iniciar y gestionar todo el stack de la aplicación FishClubAlginet (Backend .NET 10 API, Frontend React 19 / Vite y Base de Datos SQL Server 2022).

### Opción 1: Desarrollo Local Directo (PowerShell)

Para arrancar el Backend (.NET API) y el Frontend (Vite) en paralelo en entorno local de desarrollo:

```powershell
.\dev.ps1
```

- **Frontend (Vite):** [http://localhost:5173](http://localhost:5173)
- **Backend (.NET API):** [https://localhost:7179](https://localhost:7179) (Documentación Scalar en `/scalar`)
- **Parada:** Detener con `Ctrl+C` en el terminal de PowerShell (el script detiene automáticamente ambos procesos).

---

### Opción 2: Stack Contenedorizado Completo (Docker Compose)

Para levantar la infraestructura completa mediante contenedores Docker:

1. **Verificar el daemon de Docker:**
   ```bash
   docker info
   ```

2. **Levantar el stack principal (SQL Server + API + Frontend):**
   ```bash
   docker compose up -d
   ```

3. **Monitorear logs de la API (migraciones y semillas iniciales):**
   ```bash
   docker compose logs -f api
   ```

4. **(Opcional) Levantar herramientas de administración (Portainer):**
   ```bash
   docker compose -f docker-compose.tools.yml up -d
   ```

5. **Apagar el stack:**
   ```bash
   docker compose down        # Mantiene el volumen de la base de datos
   docker compose down -v     # Elimina también los volúmenes de datos
   ```

#### Puertos y Servicios Expuestos:

| Servicio | Puerto Host | Puerto Interno | URL / Acceso |
|---|---|---|---|
| **Frontend (Vite dev)** | `5173` | `5173` | [http://localhost:5173](http://localhost:5173) |
| **API (.NET 10)** | `5000` | `8080` | [http://localhost:5000](http://localhost:5000) (Scalar UI en `/scalar`) |
| **SQL Server 2022** | `1433` | `1433` | `localhost,1433` — Usuario: `sa` |
| **Portainer (opcional)** | `19100` | `9000` | [http://localhost:19100](http://localhost:19100) |
<!-- STACK_LAUNCH_RULE_END -->

<!-- OBSIDIAN_LOG_RULE_START -->
## Registro en Obsidian (Diario de a Bordo)

Al finalizar una conversación, al alcanzar un hito importante, o al terminar una sesión de trabajo, DEBES actualizar el archivo de bitácora en Obsidian ubicado en \G:\Mi unidad\Obsidian\DigitalLife\Proyectos\Fishing\DiarioDeAbordo.md\ con la fecha actual, lo que se ha completado, y cuál es el siguiente paso. Esto permite al usuario recuperar el contexto rápidamente en sesiones futuras.
<!-- OBSIDIAN_LOG_RULE_END -->

<!-- GIT_BRANCHING_RULE_START -->
## Estrategia de Ramas de Git (Feature Branches)

Siempre que vayas a implementar una nueva funcionalidad, refactorización o corrección de error:
1. **Aislamiento Obligatorio**: Crear y cambiarse a una nueva rama de características desde `main` ANTES de modificar código:
   - Características: `feat/nombre-corto-funcionalidad`
   - Correcciones de errores: `fix/nombre-corto-issue`
   - Refactorizaciones: `refactor/nombre-corto-tarea`
2. **TDD y Barrera de Pruebas**: Ejecutar la suite de pruebas automatizadas (`dotnet test`, `npm test`) y asegurar que todo pase limpio antes y después de los cambios.
3. **Commits y Push a Remoto**: Realizar commits atómicos con formato de Conventional Commits (`feat: ...`, `fix: ...`, `refactor: ...`) y subir la rama a remoto (`git push -u origin <nombre-rama>`).
<!-- GIT_BRANCHING_RULE_END -->
