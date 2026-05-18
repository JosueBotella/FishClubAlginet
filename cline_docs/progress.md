# Roadmap

## Fase 3: Concursos y Resultados ✅

### Capa 1: Dominio (Core)
- [x] Crear Enums: `Subspecialty`, `Category`, `CompetitionStatus`.
- [x] Crear Entidad `Competition` (con reglas de negocio y estados).
- [x] Crear Entidad `CompetitionResult` (vinculación pescador-concurso y lógica de puntos).

### Capa 2: Infraestructura (Infrastructure)
- [x] Configuración Fluent API para `Competition` (Índice único: LeagueId + Number).
- [x] Configuración Fluent API para `CompetitionResult` (Índice único: CompetitionId + FishermanId).
- [x] Crear y ejecutar Migración de EF Core: `AddCompetitions`.

### Capa 3: Aplicación (Application)
- [x] Definir DTOs de Fase 3 (`CompetitionDto`, `CompetitionResultDto`, `CreateCompetitionRequest`, `RegisterFishermanRequest`).
- [x] Implementar Command: `CreateCompetitionCommand` + Handler + Validator (Solo Admin).
- [x] Implementar Command: `RegisterFishermanCommand` + Handler + Validator.
- [x] Implementar Query: `GetCompetitionResultsQuery` (Rankings calculados en tiempo real).
- [x] Implementar Query: `GetCompetitionsByLeagueQuery` (listado de concursos por liga).

### Capa 4: API (Presentación backend)
- [x] `CompetitionsController`: GET /competitions?leagueId, POST /competitions, POST /competitions/{id}/register, GET /competitions/{id}/results.

### Capa 5: Presentación (React)
- [x] `types/competition.ts` — interfaces TypeScript para todos los DTOs.
- [x] `api/competitionsApi.ts` — cliente Axios (getByLeague, create, register, getResults).
- [x] `constants/endpoints.ts` + `routes.ts` — endpoints y rutas actualizados.
- [x] `AdminLeaguesPage` — icono para navegar a concursos de cada liga.
- [x] `AdminCompetitionsPage` — listado de concursos con estado y plazas.
- [x] `CreateCompetitionModal` — formulario completo (número, fecha, venue, zona, subesp., categoría, plazas).
- [x] `CompetitionResultsPage` — tabla de resultados con ranking en tiempo real + modal inscripción pescador.
- [x] `App.tsx` — rutas registradas con ProtectedRoute Admin.

---

## ✅ Fase 3.5: Estabilización del Outbox Pattern (COMPLETADA 2026-05-15)

> Bugs silenciosos detectados el 2026-05-14. Resueltos el 2026-05-15.

### TASK-A — Interceptor genérico para todas las entidades con eventos ✅
- [x] Interfaz `IHasDomainEvents` añadida en `Core/Domain/Entities/BaseEntity.cs`.
- [x] `BaseEntity<TId>` implementa `IHasDomainEvents`.
- [x] `Entries<BaseEntity<int>>()` → `Entries<IHasDomainEvents>()` en el interceptor.
- [ ] Test de integración pendiente (TASK-C lo cerrará con `LeagueActivatedDomainEvent` si se implementa).

### TASK-B — Resolver de tipos generalizado en el job ✅
- [x] Namespace hardcoded eliminado. Implementada **Opción 3 (AppDomain scan)**: `AppDomain.CurrentDomain.GetAssemblies().SelectMany(...).FirstOrDefault(t => t.Name == outboxMessage.Type && typeof(IDomainEvent).IsAssignableFrom(t) && !t.IsAbstract)`.
- [x] Sin migración EF — columna `Type` sigue con `Name` simple (retrocompatible).
- [x] `try/catch` en `GetTypes()` por ensamblados dinámicos.

### Mejora arquitectural: interceptor vía DI ✅ *(bonus — 2026-05-15)*
- [x] Eliminado `new ConvertDomainEventsToOutboxMessagesInterceptor()` de `AppDbContext.OnConfiguring`.
- [x] Registrado como `AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>()` en `Program.cs`.
- [x] `AddDbContext` migrado a patrón `(sp, options) => options.AddInterceptors(sp.GetRequiredService<...>())`.

### TASK-C — Cerrar TODOs de `Fisherman` (Update + Delete con domain events) ✅
- [x] Crear `FishermanUpdatedDomainEvent.cs` + handler stub (`FishermanUpdatedDomainEventHandler`).
- [x] Crear `FishermanDeletedDomainEvent.cs` + handler stub (`FishermanDeletedDomainEventHandler`).
- [x] `Fisherman.Update()` descomentado — mutación de estado puro (sin RaiseDomainEvent, evita dependencia circular Core → Application).
- [x] `Fisherman.Delete()` descomentado — ídem.
- [x] `UpdateFishermanCommandHandler.cs` (NEW) — lanza `FishermanUpdatedDomainEvent` antes de SaveChanges.
- [x] `SoftDeleteFishermanCommandHandler.cs` refactorizado — usa `GetById + fisherman.Delete() + RaiseDomainEvent`.
- [x] `UpdateFishermanCommandHandlerTests.cs` (NEW, 6 tests — verifica domain event + campos + error paths).
- [x] `SoftDeleteFishermanCommandHandlerTests.cs` actualizado (5 tests — verifica domain event).

### Build fix ✅
- [x] CS0246 en `Program.cs`: `global using FishClubAlginet.Infrastructure.Persistence.Interceptors;` añadido a `API/GlobalUsing.cs`.

### Definition of Done de Fase 3.5 ✅
- [x] Los tres eventos (`FishermanAdded`, `FishermanUpdated`, `FishermanDeleted`) implementados, con handlers y circulan por el Outbox.
- [x] Un evento nuevo en cualquier bounded context circulará sin tocar interceptor ni job (TASK-A + TASK-B).
- [ ] Verificación manual: cero registros con `Error IS NOT NULL` en `OutboxMessages` tras ciclo limpio (pendiente ejecución Docker).

---

## Fase 3 — Capa Application (✅ completada 2026-05-15)

- [x] `ValidationPipelineBehavior` + `DependencyInjection.cs` + `Application.csproj` actualizado.
- [x] Validators en `OpenRegistrationCommandHandler` y `CloseRegistrationCommandHandler`.
- [x] Tests: `ValidationPipelineBehaviorTests`, `CreateCompetitionCommandHandlerTests`, `RegisterFishermanCommandHandlerTests`, `OpenCloseRegistrationCommandHandlerTests`.

---

## ✅ Fase 4 — Full-stack estados de concurso + clasificación (COMPLETADA 2026-05-16)

### 4.A — Backend ✅
- [x] **`ReopenRegistrationCommand`** — `Closed → RegistrationOpen`, ventana ≤30 días. `PUT /competitions/{id}/reopen-registration`.
- [x] **`UnarchiveLeagueCommand`** — `IsArchived=false, IsActive=false`. `PUT /leagues/{id}/unarchive`.
- [x] **`AssignSpotsCommand`** — asignación secuencial por `RegistrationDate`. Permitido en `RegistrationOpen` o `Closed`. `POST /competitions/{id}/assign-spots`.
- [x] **`MoveToResultsDraftCommand`** — `Closed → ResultsDraft`. `POST /competitions/{id}/results-draft`.
- [x] **`ValidateResultsCommand`** — `ResultsDraft → ResultsValidated`. `POST /competitions/{id}/validate-results`.
- [x] **`GetCompetitionByIdQuery`** — `GET /competitions/{id}` para el frontend.
- [x] **`GetLeagueStandingsQuery`** — `GET /leagues/{id}/standings`. ByWeight + ByPoints con descarte configurable.
- [x] **`GetAllLeaguesQuery`** ampliado con `archived?` — filtra ligas archivadas/no archivadas.
- [x] Rich domain methods en `Competition`: `OpenRegistration`, `CloseRegistration`, `ReopenRegistration`, `MoveToResultsDraft`, `ValidateResults`.
- [x] `League.Unarchive()` domain method.
- [x] Tests: `ReopenRegistrationCommandHandlerTests`, `AssignSpotsCommandHandlerTests`, `MoveToResultsDraftAndValidateTests`, `UnarchiveLeagueCommandHandlerTests`.
- [ ] **Revisar concurrencia** en `RegisterFishermanCommandHandler` (race condition potencial último spot) — deuda técnica.

### 4.B/C — Frontend ✅
- [x] **`ConfirmationModal`** componente reutilizable.
- [x] **`AdminLeaguesPage`** — filtra archivadas, ConfirmationModal al archivar, botón histórico, botón clasificación.
- [x] **`AdminArchivedLeaguesPage`** (nueva) — lista archivadas, ConfirmationModal al desarchivar.
- [x] **`AdminCompetitionsPage`** — panel de estado completo, ConfirmationModal unificado, todos los botones de transición por estado.
- [x] **`CompetitionResultsPage`** — fetcha `CompetitionDto` al cargar, guarda edición solo en `Closed`/`ResultsDraft`, Alert informativo en otros estados.
- [x] **`LeagueStandingsPage`** (nueva) — tabs "Por puntos" (con descarte) y "Por peso".
- [x] Rutas `ArchivedLeagues`, `LeagueStandings` en `App.tsx` + `routes.ts`.
- [x] `endpoints.ts`, `leaguesApi.ts`, `competitionsApi.ts`, `types/competition.ts` — todo actualizado.

---

---

## 🔴 Fase 5 — PointsCalculator + Clasificación detallada (PRÓXIMA)

### 5.A — Bug crítico: Points = WeightInGrams (BLOQUEANTE) 🔴

`CompetitionResult.RecordResult()` almacena `Points = WeightInGrams`. Los puntos reales (ranking inverso con empates y mínimo) NO se calculan en ningún sitio.

**Algoritmo confirmado** con `18º - CONCURSO.xls`:
1. Filtrar `DidAttend = true`, ordenar desc por `WeightInGrams`.
2. Asignar `Ranking` con empates (mismo peso → mismo rank, el siguiente salta).
3. Puntos 1º = N (posiciones únicas). *Ej 18º: 27 asistentes − 2 empates dobles = **25 pts al 1º***.
4. Empates comparten media de puntos. *Ej pos 14-15 (1125 g) → 11,5. Pos 18-19 (1010 g) → 7,5*.
5. Mínimo `League.MinPoints` (default 5) para asistentes con 0 g.
6. Ausencia → `Points = 0`, sin mínimo.

**Items a implementar:**
- [ ] `IPointsCalculator` domain service en `Core` — stateless, recibe `IEnumerable<CompetitionResultInput>` + `minPoints`, devuelve `IEnumerable<CalculatedResult>`.
- [ ] `PointsCalculator` implementación en `Application`.
- [ ] `CalculateCompetitionPointsCommand(Guid CompetitionId)` + Handler — obtiene resultados, llama al servicio, persiste `Points` + `Ranking` en cada `CompetitionResult`. Idempotente.
- [ ] Llamada automática en `MoveToResultsDraftCommandHandler` justo antes de `SaveChangesAsync`.
- [ ] Tests unitarios `PointsCalculatorTests` — al menos 8 casos: happy path, empate doble, empate triple, todos con 0 g, un solo participante, mínimo aplicado, ausencia, mezcla asistentes+ausentes.
- [ ] Verificación con datos reales del `18º - CONCURSO.xls`: 27 participantes, pos 14-15 = 11,5, pos 18-19 = 7,5.

### 5.B — Clasificación detallada (matriz por concurso)
- [ ] Ampliar `GetLeagueStandingsQuery` con desglose por concurso: nuevo DTO `LeagueStandingsDetailDto` con `Competitions[]` + `ResultsPerCompetition: Dictionary<Guid, decimal>` por pescador.
- [ ] Tests con datos de `LIGA POR PESO 2025.xls` (43 pescadores, 18 concursos, total 1.071.845 g).
- [ ] **Columna "RESTA"**: ⚠️ PENDIENTE DEL CLIENTE — no implementar hasta recibir la regla exacta.
- [ ] Frontend: `LeagueStandingsPage` ampliada con scroll horizontal, totales por concurso, exportar Excel.

### 5.C — Pieza Mayor
- [ ] `GetSeasonBiggestCatchQuery(leagueId)` — pescador + peso + concurso.
- [ ] `GetCompetitionBiggestCatchQuery(competitionId)`.
- [ ] Widget en `/home` con resumen de liga activa.

### 5.D — Snapshots al archivar (opcional)
- [ ] Entidad `LeagueSeasonSnapshot`. Command `ArchiveLeagueWithSnapshotCommand`.

---

## 🔲 Fase 6 — Acta Oficial FPCV (Word/PDF)

- [ ] Generación programática del Acta Word con `OpenXml SDK` o `DocX`.
- [ ] Endpoint `GET /api/competitions/{id}/acta?format={pdf|docx}` (solo Admin, solo `ResultsValidated`).
- [ ] Conversor Word → PDF (LibreOffice headless o `Spire.Doc`).
- [ ] `GenerateActaRequest` con campos editables (presidente, jueces, especies, tiempo).
- [ ] Frontend: botón **Generar Acta** en detalle concurso (`ResultsValidated`) con modal de campos y descarga.

---

## 🔲 Fase 7 — Frontend rol Fisherman

- [ ] Página Calendario (`/calendar`) — concursos de la liga activa.
- [ ] Página Mis inscripciones (`/my-registrations`) — concursos inscritos con resultado.
- [ ] Sidebar adaptado al rol `Fisherman`.

---

## Deuda técnica

| Prio | Item | Estado |
|------|------|--------|
| 🔴 | `Points = WeightInGrams` en `RecordResult()` | Fase 5.A |
| 🟡 | Race condition `RegisterFishermanCommandHandler` (último spot) | Pendiente |
| 🟡 | Squashar migraciones iniciales antes del deploy | Pendiente |
| 🟡 | Verificar/rotar `JWT_SECRET_KEY` si está en historial git | Pendiente |
| 🟢 | Eliminar `IFishermanRepository` vacía | Pendiente |
| 🟢 | Índice único `Fisherman.FederationNumber` + regex `^V-\d+$` | Pendiente |
| ✅ | `Competition` Rich Domain Model | Fase 4.A |
| ✅ | `ValidationBehavior<,>` en pipeline MediatR | Fase 3 |
