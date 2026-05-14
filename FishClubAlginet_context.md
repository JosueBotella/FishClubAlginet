# FishClubAlginet_context.md — Contexto del Proyecto

## Objetivo
Plataforma de gestión para club de pesca local (Alginet). Gestiona socios (Fishermen), Ligas anuales y Competiciones.

## Stack Técnico
- **Backend:** .NET 10, Clean Architecture
- **Frontend:** React + TypeScript (migración desde Blazor WebAssembly)
- **UI:** Mantine v7 + @tabler/icons-react — sustituye Radzen Blazor
- **ORM:** Entity Framework Core + SQL Server Express
- **CQRS:** MediatR
- **Auth:** ASP.NET Core Identity (roles: Admin, Fisherman) + JWT
- **Testing backend:** xUnit, Moq, FluentAssertions
- **Patrones:** Repository, Unit of Work, Outbox Pattern, Domain Events, Factory Pattern

## Arquitectura de Capas
```
FishClubAlginet.Core          → Entidades, Value Objects, Interfaces, Domain Events
FishClubAlginet.Application   → Commands/Queries (CQRS), Handlers, Validators, DTOs
FishClubAlginet.Infrastructure → DbContext, Repositories, EF Configs, Interceptors, Seeds
FishClubAlginet.API           → Controllers, BackgroundJobs, Program.cs
FishClubAlginet.React         → App React+TypeScript (sustituye FishClubAlginet.Blazor)
FishClubAlginet.Tests         → Unit tests (xUnit, Moq, FluentAssertions)
```

## Reglas de Negocio
- Solo Admins pueden crear Ligas, Competiciones y gestionar usuarios
- Las Ligas son anuales (1 enero – 31 diciembre)
- Una Competición pertenece a una sola Liga
- Cada Competición tiene un número máximo de puestos (MaxSpots) y un nº real de participantes (ParticipantCount)
- Las inscripciones requieren validación manual por Admin
- Puntuación por concurso: ranking inverso basado en peso. Empates en peso → media de puntos de las posiciones afectadas. Mínimo 5 puntos (incluso con 0 capturas). Ausencia = sin puntos.
- Clasificación liga por peso: suma directa de gramos acumulados
- Clasificación liga por puntos (resta): suma de puntos − N peores resultados (configurable por liga)
- Premio Pieza Mayor (PM): por concurso (mayor BiggestCatchWeight) y por temporada (mayor de toda la liga)
- El Acta oficial de cada concurso se genera para la FPCV (Federación de Pesca de la Comunidad Valenciana), club V42

## Estándares i18n
- Backend sin strings en español hardcodeados
- Errores en `Errors.cs` con códigos únicos (ej: `"Auth.InvalidCredentials"`)
- Mensajes de usuario en `.resx` o manejados por el frontend via Error Codes
- Logs técnicos en inglés

---

## Modelo de Dominio

### Entidades existentes
- **Fisherman:** Socio del club. Campos: FirstName, LastName, DateOfBirth, DocumentType, DocumentNumber, FederationLicense, FederationNumber (string, ej: "V-552" — identificador federativo único), Address (Value Object), UserId, IsDeleted.

### Entidades a implementar (Phase 2-3)

- **League:** Id (Guid), Name, Year (int), IsActive, MinPoints (int, default 5 — puntos mínimos que recibe cada participante), WorstResultsToDiscard (int, default 0 — nº de peores resultados a restar en clasificación por puntos). → 1:N con Competitions.
  - Reglas de negocio:
    - Siempre se calculan DOS clasificaciones: por peso total acumulado (gramos) y por puntos (sistema resta).
    - Sistema de puntos: el 1º recibe N puntos (N = nº participantes del concurso menos posiciones de empate), descendiendo 1 punto por posición. Mínimo = MinPoints (siempre 5, incluso con 0 capturas). Empates en peso → se reparten los puntos de las posiciones afectadas (ej: dos empatados en pos. 18-19 → (8+7)/2 = 7.5 cada uno).
    - Sistema resta: al total de puntos acumulados se le descuentan los N peores resultados (WorstResultsToDiscard).
    - No participar en un concurso = no recibir puntos (no se asigna MinPoints por ausencia).

- **Competition:** Id (Guid), CompetitionNumber (int — nº ordinal dentro de la liga: 1º, 2º... 18º), Name, Date, StartTime, EndTime, Venue (string — escenario: "Bellús", "Pinedo", "Fortaleny"), Zone (string — zona: "Norte", "Sur", "C", "B", "A1-A2-A3"...), Subspecialty (string — "MAR", "AGUA DULCE"), Category (string — "SENIORS", "JUVENIL"), MaxSpots (int), ParticipantCount (int — nº real de participantes), LeagueId (FK). → Pertenece a League, 1:N con CompetitionResults.

- **CompetitionResult:** Id (Guid), FishermanId (FK), CompetitionId (FK), AssignedSpotNumber (int — nº pesquera asignado por sorteo), WeightInGrams (int — peso total capturado), BiggestCatchWeight (int?, nullable — peso de la pieza mayor, solo si aplica), Points (decimal — puntos asignados según ranking, ej: 25, 11.5, 7.5, 5), Ranking (int — posición final en ese concurso), RegistrationDate, IsValidated (bool, default false).
  - Nota: sustituye a la antigua CompetitionRegistration. Combina inscripción + resultado en una sola entidad.
  - El premio "Pieza Mayor" (PM) se determina por query: mayor BiggestCatchWeight del concurso o de la temporada.

- ~~**FishingSpot**~~ — **Eliminada.** El nº de puesto pesquero es simplemente `AssignedSpotNumber` (int) en CompetitionResult. No requiere entidad propia (no tiene estado ni comportamiento independiente).

### Clasificaciones (valores calculados, no persistidos)
- **LeagueStandingByWeight:** Suma de WeightInGrams de todos los CompetitionResults del pescador en la liga. Ordenado desc.
- **LeagueStandingByPoints:** Suma de Points de todos los CompetitionResults, menos los N peores (WorstResultsToDiscard). Ordenado desc.
- **SeasonBiggestCatch:** Mayor BiggestCatchWeight de toda la liga → pescador + peso + concurso.

### Value Objects
- **Address:** Street, City, ZipCode, Province (sin tabla propia)

---

## Flujo CQRS + Outbox Pattern (implementado en Fisherman)

```
POST /api/fishermen/add
  → Controller mapea DTO → FisherManCommand (IRequest<ErrorOr<int>>)
  → MediatR → FisherManAddCommandHandler
    → FluentValidation automático
    → Fisherman.Create() [Factory]
    → fisherman.RaiseDomainEvent(FishermanAddedDomainEvent) [ANTES de guardar]
    → _genericRepository.AddAsync()
    → SaveChangesAsync()
      → ConvertDomainEventsToOutboxMessagesInterceptor captura eventos
      → Crea OutboxMessage en la MISMA transacción (ACID)
  → ProcessOutboxMessagesJob (cada 10s) publica eventos
  → FishermanAddedDomainEventHandler procesa
```

**Garantía ACID:** Fisherman + OutboxMessage se guardan juntos o no se guarda nada.

---

## Estándares de Código Backend

### Testing (AAA obligatorio)
```csharp
// Naming: MethodName_StateUnderTest_ExpectedBehavior
[Fact]
public async Task Handle_WhenValidRequest_ShouldCreateFisherman()
{
    // Arrange
    // Act
    // Assert — FluentAssertions exclusivamente
    result.IsSuccess.Should().BeTrue();
}
```
- Result Pattern: `.IsSuccess`, `.IsFailure`, `.Value`, `.Errors`
- Fixtures: usar y extender `FisherManFixture`

### C# preferido
- Primary constructors, records para DTOs
- `sealed class` para handlers y services
- `ErrorOr<T>` como tipo de retorno
- Async/await en toda la cadena

---

## Estado del Roadmap

### ✅ Completado (backend — se mantiene íntegro)
- Base Solution Structure + Fisherman CRUD completo
- Phase 1: Identity, Security, User Management backend
- Phase 1.5: Blazor Admin MVP — **descartado, se rehace en React**
- Phase 1.85: UI Enhancements — **descartado, se rehace en React**

### 🔲 TAREA ACTIVA — Migración Frontend Blazor → React+TypeScript
Rehacer desde cero todo el frontend en React+TypeScript manteniendo la misma funcionalidad ya implementada en Blazor:

**Nueva Rama react-migration**
  - [x] Crear nueva rama `react-migration` desde `master`
  - [x] Cambiar acceso a datos de PostgreSQL a SQL Server Express
  - [x] Revisar tests unitarios backend para asegurar compatibilidad con cambios en infraestructura 
  - [x] Generar commit con la fase de migración completa del backend (sin cambios funcionales, solo infraestructura)
  - [x] Generar pull request a master para integrar cambios backend
  - [x] Marcar con check los items completados a medida que se avanza.

**Integración Mantine UI**
- [x] Instalar dependencias Mantine (`@mantine/core`, `@mantine/hooks`, `@mantine/form`, `@mantine/notifications`, `@tabler/icons-react`, PostCSS)
- [x] Configurar `MantineProvider` con tema personalizado (colores club, fuente, border-radius)
- [x] Configurar `postcss.config.cjs` con `postcss-preset-mantine`
- [x] Importar CSS base de Mantine en `main.tsx`
- [x] Configurar `Notifications` provider global
- [x] Crear tema base en `src/theme/theme.ts` (overrides de colores, fuente, etc.)

**Auth**
- [x] Login (muro de login)
- [x] Logout
- [x] Gestión de JWT (almacenamiento, refresh, interceptor HTTP)
- [x] Rutas protegidas por rol (Admin / Fisherman)
- [x] Revisar tests unitarios backend para asegurar compatibilidad con cambios en infraestructura (tests 100% mocks, sin deps de BD)
- [x] Generar commit con la fase de migración completa del backend (sin cambios funcionales, solo infraestructura)
- [x] Generar pull request a master para integrar cambios backend — PR #9 mergeado
- [x] Marcar con check los items completados a medida que se avanza.
**Layout y navegación**
- [x] Layout con sidebar diferenciado por rol (AppShell de Mantine, NavItems filtrados por rol)
- [x] Mostrar nombre del usuario logado junto al logout (header con Avatar + email + roles + logout)
- [x] Enlace Home y enlace Perfil en navegación (+ Usuarios y Pescadores solo para Admin)
- [x] Pagina 404 con navegacion de vuelta
- [x] Revisar tests unitarios backend — no aplica, sin cambios backend en esta fase (solo frontend)
- [x] Generar commit con mensaje "feat: add layout with role-based sidebar and navigation"
- [x] Generar pull request a master para integrar cambios con mensaje "feat: Layout y navegación"
- [x] Marcar con check los items completados a medida que se avanza.
- [x] Revisar tests unitarios backend para asegurar compatibilidad con cambios en infraestructura 
- [x] Generar commit con la fase de migración completa del backend (sin cambios funcionales, solo infraestructura)
- [x] Generar pull request a master para integrar cambios backend con mensaje "feat: Layout y navegación"
- [x] Marcar con check los items completados a medida que se avanza.

**Admin — Users**
- [x] Grid de usuarios (email, roles, estado bloqueo) — Mantine Table con badges de rol y estado
- [x] Crear usuario Admin/Fisherman (modal/dialog) — CreateUserModal con form validado
- [x] Bloquear/Desbloquear usuario — ActionIcon con toggle y notificaciones
- [x] Deshabilitar acciones sobre el propio usuario logado — isSelf check, disabled en ActionIcon
- [x] Search/filter en grid — TextInput con busqueda por email, Enter y boton
- [x] Paginación — server-side con Pagination de Mantine, PAGE_SIZE=15
- [x] Revisar tests unitarios backend — no aplica, sin cambios backend (solo frontend)
- [x] Generar commit con mensaje "feat: admin users management page"
- [x] Generar pull request a master para integrar cambios con mensaje "feat: Admin — Users"
- [x] Marcar con check los items completados a medida que se avanza.
**Admin — Fishermen**
- [x] Crear endpoint backend DELETE /api/fishermen/{id} (SoftDeleteFishermanCommand + handler)
- [x] Grid de pescadores (Mantine Table con nombre, documento, licencia, fecha nac., ciudad)
- [x] Soft Delete (botón con modal de confirmación + notificación)
- [x] Filtrar eliminados del grid (backend: parámetro ShowDeleted en GetAll)
- [x] Vista histórico de pescadores eliminados (toggle activa vista histórico con badge y título dinámico)
- [x] Search/filter en grid (por nombre, documento, licencia)
- [x] Paginación server-side (PAGE_SIZE=15, Pagination de Mantine)
- [x] Tests unitarios backend para ShowDeleted (3 nuevos casos con FluentAssertions)
- [X] Generar commit con mensaje "feat: Admin — Fishermen"
- [X] Generar pull request a master con mensaje "feat: Admin — Fishermen"

**Perfil de usuario**
- [x] Vista readonly con datos del usuario (email/roles desde JWT + datos personales del Fisherman vía `GET /api/fishermen/my-profile`)
- [x] Cambio de contraseña (validar contraseña actual, sección dentro de `/profile`, integrado con `POST /api/account/change-password`)
- [x] Notificaciones de éxito/error con `@mantine/notifications`
- [x] Soporte para usuarios sin ficha de Fisherman (Admins puros): se oculta el bloque "Datos personales" si el endpoint devuelve 404

**Gestión de roles**
- [x] Asignar/quitar roles a usuarios — modal "Editar usuario" con checkboxes (Admin/Fisherman), preseleccionados según el estado actual; calcula el diff y llama a `assignRole`/`removeRole` solo para los cambios; bloquea acción si el usuario quedaría sin roles o si es el propio usuario logado

**Dockerización**
- [x] Dockerfile multi-stage para API (.NET 10 SDK build → ASP.NET runtime, expone HTTP plano en 8080)
- [x] Dockerfile dev para React (Node 20 alpine + Vite con HMR) y Dockerfile prod (build estático + Nginx con SPA fallback y proxy `/api/*` → `api:8080`)
- [x] `docker-compose.yml` (dev) con db (SQL Server 2022) + api + frontend, healthcheck en db, bind mount del código del front para HMR
- [x] `docker-compose.prod.yml` con builds optimizados, sin bind mounts, restart policies, sin exponer puerto de DB
- [x] `docker-compose.tools.yml` opcional con **Portainer CE** (UI de gestión de contenedores) en puerto configurable `PORTAINER_PORT` (default 19100, mapeado al 9000 interno de Portainer)
- [x] `.env.example` raíz con `SA_PASSWORD`, `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_DURATION_MINUTES`, `PUBLIC_HTTP_PORT`, `PORTAINER_PORT`
- [x] `.dockerignore` raíz (excluye `bin/`, `obj/`, `node_modules/`, `.env`, `.git`)
- [x] `.gitignore` actualizado: `.env`, `.env.local`, `.env.prod` ignorados; `!.env.example` mantenido
- [x] Ajustes en `Program.cs`: `UseHttpsRedirection` solo fuera de container (vía `DOTNET_RUNNING_IN_CONTAINER`); `MapInboundClaims = false` en JwtBearer (fix de 403 por mapping legacy de claims)
- [x] `appsettings.json` y `appsettings.Development.json` con secrets vacíos (se inyectan vía env vars `ConnectionStrings__LocalConnectionString` y `JwtSettings__SecretKey`)
- [x] `vite.config.ts` con `VITE_PROXY_TARGET` configurable (Docker → `http://api:8080`, local → `https://localhost:7179`)
- [x] README actualizado con sección "🚢 Arranque rápido con Docker" + tabla de comandos
- [x] Refactor a patrón Unit of Work consistente: `IUnitOfWork.SaveChangesAsync` devuelve `ErrorOr<int>`, mapeo de excepciones EF (`DbUpdateException` con `SqlException 2627/2601` → `Error.Conflict`) centralizado en `UnitOfWorkService` (Infrastructure); handlers de Application sin dependencias de EF
- [x] Tests unitarios actualizados al nuevo patrón UoW (`SoftDeleteFishermanCommandHandlerTests` y `FisherManAddCommanHandlerTests`) con verificación explícita de `SaveChangesAsync` invocado/no invocado según flujo

**Documentación (sesión 2026-05-14)**
- [x] `PROJECT_STATUS.md` — Manifest técnico completo para handoff a OpenCode Desktop / nuevas sesiones (tech stack con versiones, esquema BD, fragmentos clave, bugs, micro-commitments).
- [x] `cline_docs/activeContext.md` reescrito enfocado a Phase 3.5 (bugs del Outbox).
- [x] `cline_docs/progress.md` con sección "Fase 3.5" + bloque "Deuda técnica detectada".
- [x] `domain_model.md` reescrito con el modelo real verificado (entidades, value objects, enums, jerarquía, diagrama de relaciones).
### ✅ Completado — Phase 2: Gestión de Ligas

> Entidad `League` operativa con Rich Domain Model. Una sola liga activa simultáneamente en el club.

#### Backend
- [x] Entidad `League : BaseEntity<Guid>` con factory `Create()` y métodos de dominio `Update()`, `Activate()`, `Deactivate()`, `Archive()`.
- [x] DTOs: `LeagueDto`, `CreateLeagueRequest`, `UpdateLeagueRequest` (en `Contracts/DTOs/Requests|Responses/League/`).
- [x] Commands MediatR: `CreateLeagueCommand`, `UpdateLeagueCommand`, `ActivateLeagueCommand`, `ArchiveLeagueCommand`.
- [x] Queries MediatR: `GetAllLeaguesQuery`, `GetLeagueByIdQuery`, `GetActiveLeagueQuery`.
- [x] FluentValidation registrado (pero ver deuda técnica: falta `ValidationBehavior` en pipeline).
- [x] `LeaguesController` con `[Authorize]` por roles.
- [x] Tests unitarios completos de los 7 handlers (`FishClubAlginet.Tests/Handlers/Leagues/`).
- [x] Migración EF Core `20260506183057_Initial` que añade tabla `Leagues`.

#### Ampliación a entidad existente `Fisherman`
- [x] Campo `FederationNumber` (string?) añadido como propiedad nullable en `Fisherman`.
- [ ] *Pendiente*: índice único sobre `FederationNumber` y regex de formato `^V-\d+$` (no aplicado todavía; aún acepta nulls y duplicados).

#### Frontend
- [x] Página **Admin → Ligas** con grid, estados y acciones.
- [x] Modal **Crear / Editar Liga**.
- [ ] Indicador en sidebar de la liga activa (pendiente, baja prioridad).

---

### 🟡 Mayormente completado — Phase 3: Concursos y Resultados

> Núcleo de gestión de concursos implementado: crear, abrir/cerrar inscripciones, inscribir pescadores, registrar resultados, consultar ranking. Quedan piezas avanzadas (sorteo de puestos, entrada bulk, transiciones automáticas, cálculo de puntos).

#### Backend — Entidades

- [x] Entidad `Competition : BaseEntity<Guid>` con todos los campos definidos (LeagueId, CompetitionNumber, Date, Venue/Zone libres, Subspecialty, Category, MaxSpots, ParticipantCount, `Status` con enum `Planned/RegistrationOpen/Closed/ResultsDraft/ResultsValidated`).
  - ⚠️ **Estilo Anemic Model** (todos setters públicos). Refactor a Rich Model pendiente en deuda técnica.
- [x] Entidad `CompetitionResult : BaseEntity<Guid>` combinando inscripción + resultado (FishermanId int, AssignedSpotNumber int?, DidAttend, WeightInGrams, BiggestCatchWeight, Points decimal, Ranking, RegistrationDate, IsValidated).
- [x] Índices únicos compuestos `(CompetitionId, FishermanId)` y `(CompetitionId, AssignedSpotNumber)` aplicados en migración `20260508180238_AddCompetitions`.
- [x] DTOs: `CompetitionDto`, `CompetitionRequests`, `FishermanProfileDto`.

#### Backend — Handlers implementados
- [x] `CreateCompetitionCommand` + Handler
- [x] `OpenRegistrationCommand` + Handler
- [x] `CloseRegistrationCommand` + Handler
- [x] `RegisterFishermanCommand` + Handler *(⚠️ posible race condition en último spot — sin verificar)*
- [x] `RemoveRegistrationCommand` + Handler
- [x] `UpdateCompetitionResultCommand` + Handler
- [x] `GetCompetitionResultsQuery` + Handler (rankings calculados en tiempo real)
- [x] `GetCompetitionsByLeagueQuery` + Handler
- [x] `CompetitionsController` con endpoints REST

#### Backend — Pendiente de Phase 3 extendida
- [ ] `AssignSpotsCommand` (sorteo manual / aleatorio de puestos pesquera)
- [ ] `EnterResultsCommand` bulk (introducir todos los pesos en una sola operación)
- [ ] Transición automática del `Status`: al validar todos los resultados → `ResultsValidated`
- [ ] **Servicio de dominio `PointsCalculator`** con tests exhaustivos:
  - Algoritmo (CONFIRMADO observando `18º - CONCURSO.xls`):
    1. Filtrar `DidAttend = true`, ordenar desc por `WeightInGrams`.
    2. Asignar `Ranking` aplicando empates (mismo peso → mismo ranking, siguiente salta).
    3. Puntos base: 1ª posición recibe `N` puntos donde `N = nº de posiciones únicas tras resolver empates`. En el 18º: 27 participantes − 2 empates dobles = **25 puntos al primero**.
    4. **Empates**: comparten la media de los puntos individuales. Ej: pos 14-15 con 1125 g → (12+11)/2 = **11,5 c/u**. Pos 18-19 con 1010 g → (8+7)/2 = **7,5 c/u**.
    5. **Mínimo `League.MinPoints`** (default 5) para todo el que asista, incluso con 0 g.
    6. **Ausencia (`DidAttend = false`)** → `Points = 0` (NO recibe `MinPoints`).
- [ ] `CalculateCompetitionPointsCommand` (idempotente, recalcula sin duplicar)
- [ ] **Tests unitarios de los 8 handlers de Competitions** — actualmente sin cobertura

#### Frontend — Admin (completado)
- [x] Página **Admin → Concursos** con grid filtrable por estado
- [x] Modal **Crear Concurso** con todos los campos
- [x] Página detalle de concurso con tabs (Inscripciones, Resultados, Clasificación)
- [x] `App.tsx` con rutas protegidas Admin

#### Frontend — Pescador (pendiente)
- [ ] Página **Calendario** (`/calendar`)
- [ ] Página **Mis inscripciones** (`/my-registrations`)

---

### 🔧 Phase 3.5: Estabilización del Outbox Pattern (EN CURSO — 2026-05-14)

> **Bugs detectados al auditar el código para `PROJECT_STATUS.md`.** Bloquean cualquier emisión de `DomainEvent` desde Ligas o Competiciones. Resolver antes de añadir nuevos eventos.

#### 🔴 BUG-1 — Interceptor solo captura `BaseEntity<int>`
- **Archivo:** `FishClubAlginet.Infrastructure/Persistence/Interceptors/ConvertDomainEventsToOutboxMessagesInterceptor.cs:19`
- **Síntoma:** `dbContext.ChangeTracker.Entries<BaseEntity<int>>()` solo matchea `Fisherman`. Los eventos de `League`, `Competition`, `CompetitionResult` (todos `BaseEntity<Guid>`) **se descartan silenciosamente**.
- **Impacto:** cualquier `RaiseDomainEvent()` en `League.Activate/Archive/Update` o `Competition.*` no produce side effects.

#### 🔴 BUG-2 — Job hardcoded al namespace Fishermen
- **Archivo:** `FishClubAlginet.API/Infrastructure/BackgroundJobs/ProcessOutboxMessagesJob.cs:53`
- **Síntoma:** `var typeName = $"FishClubAlginet.Application.Features.Events.Commands.Fishermen.{outboxMessage.Type}, FishClubAlginet.Application";`
- **Impacto:** aunque arreglemos BUG-1, eventos de otros bounded contexts saldrán como "Type not found".

#### TODOs ligados (Fisherman)
- [ ] `Fisherman.cs:53` — descomentar `Update()` con `FishermanUpdatedDomainEvent`
- [ ] `Fisherman.cs:76` — descomentar `Delete()` con `FishermanDeletedDomainEvent`

#### Plan de fix (ver detalle en `cline_docs/progress.md`)
- [ ] **TASK-A** — Crear interfaz `IHasDomainEvents` no genérica en `BaseEntity.cs`, cambiar el interceptor a `Entries<IHasDomainEvents>()`.
- [ ] **TASK-B** — Persistir `AssemblyQualifiedName` en `OutboxMessage.Type` o construir diccionario de tipos al startup.
- [ ] **TASK-C** — Cerrar `Update()`/`Delete()` de Fisherman con sus dos `DomainEvent` y handlers stub.

---

### 🔲 Pendiente — Phase 4: Clasificaciones de Liga

> **Análisis basado en `LIGA POR PESO 2025.xls` y `LIGA RESTA 2025.xls`**.
>
> Las clasificaciones son **vistas calculadas** sobre los `CompetitionResult` ya validados de una `League`. No se persisten (siempre son frescas), pero se pueden **snapshotear** para histórico al cerrar una temporada.

#### Backend — Servicios de cálculo
- [ ] Servicio `WeightStandingCalculator`:
  - Suma directa de `WeightInGrams` por `FishermanId` para todos los `CompetitionResult` de la liga
  - Pescador con `DidAttend = false` aporta 0 (igual que `WeightInGrams = 0`)
  - Devuelve lista ordenada descendente con: Posición, Pescador, Peso por concurso (matriz), Total
  - Incluye agregado por concurso (peso total de cada concurso)
- [ ] Servicio `PointsStandingCalculator`:
  - Suma de `Points` por `FishermanId`
  - Aplica `League.WorstResultsToDiscard`: descarta los N peores `CompetitionResult` (incluidas ausencias con 0)
  - Devuelve: Posición, Pescador, Puntos por concurso, **Total bruto**, **Total descartado** (renombrar para evitar confusión con "Resta" del Excel), **Total final**
  - ⚠️ **PENDIENTE DEL CLIENTE — NO IMPLEMENTAR HASTA TENER LA REGLA**: el Excel `LIGA RESTA 2025.xls` muestra una columna "RESTA" cuyo significado preciso no es deducible solo de los datos. En la temporada 2025 únicamente Juan Alcaraz tiene `RESTA = 2,5` aplicada, sin patrón evidente respecto a otros pescadores con decimales. El cliente confirmará la regla más adelante; hasta entonces, la implementación de esta vista debe **dejarse para el final de Phase 4** y arrancar en cuanto se conozca la fórmula. Mientras tanto, la columna se mostrará vacía en la UI con un tooltip "Pendiente de definir".
- [ ] Tests unitarios validando con los datos exactos del `LIGA POR PESO 2025.xls` (43 pescadores, 18 concursos celebrados, total general 1.071.845 g)

#### Backend — Pieza Mayor
- [ ] Query `GetSeasonBiggestCatchQuery(leagueId)`: mayor `BiggestCatchWeight` de toda la liga → devuelve pescador, peso, concurso. Confirmado en `LIGA RESTA 2025.xls` R54: "PM CRISTIAN VOINESCU - con un peso de: 4870 gr."
- [ ] Query `GetCompetitionBiggestCatchQuery(competitionId)`: mayor `BiggestCatchWeight` del concurso

#### Backend — Snapshots opcionales
- [ ] Entidad opcional `LeagueSeasonSnapshot` (Guid, LeagueId, CapturedAt, JsonPayload) para guardar la clasificación final del año al archivar la liga
- [ ] Command `ArchiveLeagueWithSnapshotCommand`

#### Frontend
- [ ] Página **Clasificación liga peso** (`/leagues/{id}/standings/weight`): tabla amplia con scroll horizontal — columnas: Posición, Nombre, [Concurso 1] [Concurso 2] ... [Concurso N], Total. Última fila con totales por concurso. Exportable a Excel
- [ ] Página **Clasificación liga puntos** (`/leagues/{id}/standings/points`): misma estructura pero con puntos. Columnas adicionales: Total bruto, Descartado, Total final
- [ ] Página **Pieza Mayor** (`/leagues/{id}/biggest-catches`): top de mayores capturas de la temporada y por concurso
- [ ] Widget en home con **resumen liga activa** (top 3 peso + top 3 puntos + última pieza mayor)

---

### 🔲 Pendiente — Phase 5: Acta Oficial FPCV (Word/PDF)

> **Análisis basado en `18ª Acta competiciones clubs de 02-11-2025.doc`**.
>
> El Acta es un documento oficial federativo (formato FPCV — Federación de Pesca de la Comunidad Valenciana, club V42) que se genera por concurso una vez validados los resultados. Tiene cabecera fija con texto legal, datos del concurso, tabla de clasificación individual y secciones de estadística + especies capturadas.

#### Backend
- [ ] Plantilla Word `.dotx` o generación programática con biblioteca .NET (`OpenXml SDK` o `DocX`):
  - Cabecera: "Club de Pesca V42", título "ACTA del concurso celebrado durante el día {fecha} en la localidad de {Venue}", subespecialidad, categoría
  - Texto legal del jurado (constitución, fallo, antedecentes)
  - Sección **CLASIFICACIÓN INDIVIDUAL**: tabla con `Clasif | CONCURSANTE | Federativa | < 14 años | > 14 años | Puesto | Peso`
    - Las columnas `< 14 años` y `> 14 años` se rellenan **automáticamente** según la edad calculada del pescador a la fecha del concurso (`Fisherman.DateOfBirth` vs `Competition.Date`). No se introducen manualmente.
  - Total general del concurso
  - Sección **ESTADÍSTICA**: tiempo efectivo de pesca (en horas)
  - Sección **ESPECIES CAPTURADAS**: dos filas de checkboxes con especies marítimas (Mabras, Doradas, Llobarros, Palometas, Roncadores, Sargos, Llisas, Salpas, Vidriada, Otros) y dulceacuícolas (Carpas, Barbos, Alburnos, Carpines, Percasol, Gobios, Lucio, Truchas, Black Bass, Otros)
  - Pie con campaña antidopaje y URL FPCV
- [ ] Conversor Word → PDF (LibreOffice headless en el container de la API, o `Spire.Doc`, evaluar licencia)
- [ ] Endpoint `GET /api/competitions/{id}/acta?format={pdf|docx}` que devuelve binario con `Content-Type` adecuado
- [ ] Solo accesible a `Admin`. Solo permitido si `Competition.Status = ResultsValidated`
- [ ] DTO `GenerateActaRequest` con campos editables por Admin antes de generar (presidente del jurado, comité organizador, jueces, especies marcadas, tiempo efectivo)

#### Frontend
- [ ] En el detalle del concurso (cuando estado = `ResultsValidated`), botón **Generar Acta** que abre modal con campos editables (presidente, jueces, especies, tiempo) y botones **Descargar Word** / **Descargar PDF**

---

### 🔲 Pendiente — Phase 6: Estadísticas y Reporting (opcional / futura)
- [ ] Dashboard global del club: nº pescadores activos, nº concursos celebrados por temporada, evolución de la pesca (kg por año), top historico de pieza mayor
- [ ] Gráficos con `recharts` (ya disponible en stack React)
- [ ] Comparativa entre temporadas (ligas históricas)
- [ ] Exportación de cualquier vista a Excel (xlsx)

---

## URLs de desarrollo

### Modo Docker (recomendado, ver `docker-compose.yml`)
- **Frontend:** http://localhost:5173
- **API:** http://localhost:5000 (Scalar API reference en `/scalar`)
- **SQL Server:** `localhost,1433` con usuario `sa` y `SA_PASSWORD` del `.env`
- **Portainer (opcional):** http://localhost:19100 (levantar con `docker-compose.tools.yml`)

### Modo local sin Docker (Kestrel directo)
- **API:** https://localhost:7179
- **React:** http://localhost:5173 (Vite proxy → `https://localhost:7179`)

- **Repo:** https://github.com/JosueBotella/FishClubAlginet

## Ejecutar proyecto

### Vía Docker (recomendado)
```bash
cp .env.example .env             # rellena passwords / JWT secret
docker compose up -d             # levanta db + api + frontend
docker compose logs -f api       # ver migraciones + seed en el primer arranque

# Opcional: Portainer UI
docker compose -f docker-compose.tools.yml up -d
```

### Vía CLI nativo (necesita SDK .NET 10 + SQL Server + Node 20)
```bash
dotnet ef database update --project FishClubAlginet.Infrastructure --startup-project FishClubAlginet.API
dotnet run --project FishClubAlginet.API
# en otra terminal:
cd fishclubalginet-frontend && npm install && npm run dev
```
