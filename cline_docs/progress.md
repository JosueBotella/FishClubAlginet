# Roadmap de Progreso - FishClubAlginet

## Estado General del Proyecto
- **Backend:** Arquitectura Limpia (Clean Architecture), .NET 10, EF Core, SQL Server Express, MediatR (CQRS), Outbox Pattern. CRUDs principales completos y lógica de dominio rica en funcionamiento.
- **Frontend:** React 19 + TypeScript, Mantine v7, Axios, Client-side routing. Vistas de administración para usuarios, pescadores, ligas y concursos operativas con guards de estado.
- **Base de Datos:** Migrado de PostgreSQL a SQL Server Express. Soporta dockerización completa en entornos de desarrollo y producción.

---

## ✅ Fase 1 a Fase 4: Infraestructura, Auth, Ligas y Concursos (COMPLETADAS)

### Fase 1: Auth, Identity y Dockerización
- [x] Configuración de ASP.NET Core Identity con roles `Admin` y `Fisherman`.
- [x] Autenticación JWT en backend y almacenamiento seguro con tokens en frontend.
- [x] Muro de login, rutas protegidas y sidebar adaptativo por rol en frontend.
- [x] Dockerización multi-stage para desarrollo (`docker-compose.yml`) y producción (`docker-compose.prod.yml`).
- [x] Integración de Portainer para monitorización local.

### Fase 2: Gestión de Ligas
- [x] Entidad `League` con Rich Domain Model (métodos de activación, inactivación, archivo e históricos).
- [x] CRUD de ligas con endpoints REST y handlers de MediatR correspondientes.
- [x] Grid de administración de ligas en frontend con modals para creación/edición.

### Fase 3: Concursos y Resultados Básicos
- [x] Entidades `Competition` y `CompetitionResult` (que combinan registro y resultado) con índices únicos para evitar duplicidades de pesquera e inscripción doble.
- [x] Transiciones de estado del concurso: `Planned` $\rightarrow$ `RegistrationOpen` $\rightarrow$ `Closed`.
- [x] Flujos de registro e inscripción de pescadores desde frontend.
- [x] Asignación secuencial automática de pesqueras (`AssignSpotsCommand`).

### Fase 3.5: Estabilización del Outbox Pattern
- [x] Interceptor `ConvertDomainEventsToOutboxMessagesInterceptor` generalizado con la interfaz `IHasDomainEvents` (captura eventos de todas las entidades, no solo de tipo `int`).
- [x] Job `ProcessOutboxMessagesJob` adaptado para escanear tipos en el AppDomain, resolviendo tipos dinámicos sin nombres de espacio hardcodeados.
- [x] Domain events en `Fisherman` (`Added`, `Updated`, `Deleted`) integrados y testados con cobertura del 100% de los handlers con FluentAssertions.

### Fase 4: Concursos Avanzados y Ligas Archivadas
- [x] Transiciones de estado avanzado: `Closed` $\rightarrow$ `ResultsDraft` $\rightarrow$ `ResultsValidated`.
- [x] Acciones avanzadas: reapertura de inscripciones (`ReopenRegistration` con ventana de 30 días) y desarchivo de ligas.
- [x] Pestañas de clasificación básica por peso y puntos en `LeagueStandingsPage` en frontend.
- [x] Guards de estado en frontend (`EDITABLE_STATUSES` para edición de resultados únicamente en borrador o cerrado).

---

## ✅ Fase 5.A: PointsCalculator + Configuración de Pieza Mayor (COMPLETADA 2026-05-22)

### 5.A — Sistema de puntos rediseñado y automatizado
- [x] Interfaz `IPointsCalculator` definida en el dominio (`Core/Domain/Services`).
- [x] Implementación en `PointsCalculatorService` (Application) siguiendo las reglas del club:
  - Asistencia base: `League.MinPoints` (= 5 puntos) para todo el que asista (`DidAttend = true`).
  - Bonus por ranking: Posiciones 1 a 20 reciben bonus decreciente (+20 a +1 punto). Posiciones 21+ reciben +0 puntos.
  - Gestión de empates: Los empatados en peso en el top 20 reciben el ranking de inicio de grupo y se reparten equitativamente un bonus de `+1 / nEmpatados`.
  - Ausentes: Reciben 0 puntos y ranking 0.
- [x] Integración automática en `MoveToResultsDraftCommandHandler` justo antes de persistir los resultados en la base de datos.
- [x] Cobertura de tests unitarios al 100% en `PointsCalculatorServiceTests.cs` (10 tests detallando happy path, empates dobles/triples, posiciones >20, ausentes y casos sin capturas).

### 5.A.bis — Configuración de Pieza Mayor por Competición
- [x] Agregar campo nullable `BiggestCatchMinWeightInGrams` a `Competition` con migración SQL Server exitosa.
- [x] Crear endpoint `PATCH /api/competitions/{id}/biggest-catch-config` accesible a administradores.
- [x] Añadir control inline `NumberInput` en la página de resultados del concurso (`CompetitionResultsPage.tsx`) en frontend para actualizar y guardar este mínimo dinámicamente en tiempo real.
- [x] Soporte en la creación de concursos (`CreateCompetitionModal.tsx`) para asignar este límite desde el inicio.

---

## ✅ Fase 5.B: Clasificación Detallada (Matriz por Concurso - BACKEND COMPLETADO 2026-05-25)

### 5.B — Clasificación Detallada (Matriz por Concurso)
- [x] **Zona de Concurso Opcional:** Modificado la obligatoriedad del campo `Zone` en `Competition` para que sea opcional (`string?` nullable) en base de datos, backend (validadores de creación) y frontend (formulario `CreateCompetitionModal`).
- [x] Crear consulta matricial `GetLeagueStandingsMatrixQuery` para devolver un desglose matricial ordenado por puntos y peso, incluyendo celdas para cada pescador y concurso (`FishermanId` -> `Dictionary<Guid, CompetitionCellDto>`).
- [x] Definir DTOs del contrato matricial (`CompetitionHeaderDto`, `CompetitionCellDto`, `FishermanMatrixRowDto` y `LeagueStandingsMatrixDto`) para estructurar la matriz de forma óptima.
- [x] Implementar la lógica avanzada de descartes de los N peores resultados (`WorstResultsToDiscard`) ordenando ascendentemente las puntuaciones de las jornadas asistidas.
- [x] Diseñar suite robusta de pruebas unitarias (`GetLeagueStandingsMatrixQueryHandlerTests.cs`) con cobertura total para verificar comportamiento con ligas inexistentes, vacías, descartes secuenciales y no-asistencias.

---

## 🔲 Fase 5.C - 5.E: Piezas Mayores, Frontend de Matriz y Snapshots (EN CURSO)

## ✅ Fase 5.C: Agregaciones de Pieza Mayor (BACKEND COMPLETADO 2026-05-26)

### 5.C — Agregaciones de Pieza Mayor
- [x] Implementar query `GetSeasonBiggestCatchQuery(Guid leagueId)` para retornar la captura récord de la liga (Pescador, Concurso, Peso) respetando mínimos de competición.
- [x] Implementar query `GetCompetitionBiggestCatchQuery(Guid competitionId)` para obtener la pieza mayor de una jornada validando umbrales mínimos.
- [x] Mapear e integrar la marca `IsBiggestCatch` dentro de la respuesta `CompetitionResultDto` cargando y calculando el máximo sobre la marcha en los listados del concurso.
- [x] Exponer las rutas de API REST `GET /api/leagues/{id}/biggest-catch` y `GET /api/competitions/{id}/biggest-catch`.
- [x] Robustecer con tests unitarios dedicados en `GetSeasonBiggestCatchQueryHandlerTests` y `GetCompetitionBiggestCatchQueryHandlerTests`.

### 5.D — Frontend para Matriz Detallada
- [ ] Modificar `LeagueStandingsPage.tsx` para renderizar una tabla scrollable horizontal que liste todos los concursos de la temporada como columnas (`[C1] [C2] ... [CN]`).
- [ ] Agregar fila final con la suma total y promedios por jornada.
- [ ] Diseñar la pestaña "Pieza Mayor" (`/leagues/{id}/biggest-catches`) en frontend.
- [ ] Agregar widgets resumen de la liga en el panel principal (`HomePage.tsx`).

### 5.E — Snapshots de Temporada
- [ ] Crear la entidad de persistencia `LeagueSeasonSnapshot` para guardar el estado final inmutable de la liga.
- [ ] Implementar `ArchiveLeagueWithSnapshotCommand` que se ejecute en cascada al archivar una liga activa.

---

## 🔲 Planificación de Fases Futuras

### Fase 6: Acta Oficial FPCV (Word/PDF)
- [ ] Integrar generación de documentos oficiales con `OpenXml SDK` o `DocX` en backend.
- [ ] Programar los filtros de edad automatizados para rellenar las columnas `< 14 años` y `> 14 años` usando la fecha de nacimiento (`Fisherman.DateOfBirth`) contra la del concurso.
- [ ] Endpoint `GET /api/competitions/{id}/acta?format={pdf|docx}` disponible cuando `Status = ResultsValidated`.
- [ ] Formulario interactivo en frontend para rellenar campos variables (jueces, presidente, especies, tiempo de pesca) antes de la descarga.

### Fase 7: Frontend para Rol Fisherman
- [ ] Página de calendario de concursos (`/calendar`).
- [ ] Página personal del pescador (`/my-registrations`) para ver estado de sorteos, pesqueras asignadas e histórico de pesajes propios.
- [ ] Adaptar la barra lateral de navegación para usuarios con rol Fisherman.

### Fase 8: Estadísticas y Reporting
- [ ] Gráficos evolutivos de pesca total (kg acumulados por año/escenario) con `recharts`.
- [ ] Exportación directa de clasificaciones y grids a formato Excel (`.xlsx`).

### 🔲 Fase de Revisión de Tests Unitarios y Mockeo (Plan en [test_review_plan.md](file:///C:/Users/spawndevuser/.gemini/antigravity/brain/a6d74e53-07f9-449b-8472-052cbb9ec34c/test_review_plan.md))
- [x] **Fase A — Tests Unitarios de Dominio Puro (Core):** Crear pruebas directas enfocadas en la lógica rica de entidades de negocio (`CompetitionTests`, `LeagueTests`, `CompetitionResultTests`).
- [ ] **Fase B — Fixture Builders:** Diseñar e integrar `LeagueBuilder` y `CompetitionBuilder` para eliminar boilerplate de Arrange.
- [ ] **Fase C — Robustecimiento de Mocks (Handlers):** Estandarizar comprobaciones de transacción (`Verify` de `SaveChangesAsync` en commands) y pruebas de error de base de datos.

---

## Deuda Técnica Priorizada

| Prioridad | Descripción del Item | Bounded Context | Estado |
| :---: | :--- | :---: | :---: |
| 🟡 **Media** | Controlar carrera de competidores en el último spot disponible en `RegisterFishermanCommandHandler`. | Application | Pendiente |
| 🟡 **Media** | Integrar/squashar migraciones de desarrollo (`InitialSqlServer` + `Initial`) antes de producción. | Infrastructure | Pendiente |
| 🟡 **Media** | Auditar historial git para asegurar la no exposición de claves secretas (`JWT_SECRET_KEY`). | DevOps | Pendiente |
| 🟢 **Baja** | Eliminar la interfaz vacía `IFishermanRepository` si se usa exclusivamente el repositorio genérico. | Core | Pendiente |
| 🟢 **Baja** | Aplicar índice único y restricción regex `^V-\d+$` en `Fisherman.FederationNumber`. | Core / DB | Pendiente |
| ✅ **Resuelta** | Cambiar el campo `Zone` de obligatorio a opcional (nullable) en la creación de concursos. | Core / App / Front | **Resuelto** |
| ✅ **Resuelta** | Corregir bug crítico de asignación de gramos como puntos en el cálculo de resultados. | Core / App | **Resuelto (5.A)** |
| ✅ **Resuelta** | Refactorizar `Competition` de modelo anémico a Rich Domain Model. | Core | **Resuelto (4.A)** |
| ✅ **Resuelta** | Registrar `ValidationBehavior` global en el pipeline de MediatR para validaciones automáticas. | Application | **Resuelto (3)** |
