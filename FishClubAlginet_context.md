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
- **Puntuación por concurso (sistema actualizado — Fase 5.A):**
  - Asistencia: cada participante que asiste recibe `League.MinPoints` (= 5) solo por acudir.
  - Escala fija de ranking: 1º → +20 pts, 2º → +19 pts, … 20º → +1 pt, más allá del 20º → +0 pts.
  - Empates dentro del top 20: cada empatado conserva sus puntos de posición individual + recibe `+1/nEmpatados` bonus. Ejemplo: 2º y 3º empatados → 2º = 5+19+0.5 = 24.5, 3º = 5+18+0.5 = 23.5.
  - Ausencia: 0 puntos (no se aplica mínimo de asistencia).
- Clasificación liga por peso: suma directa de gramos acumulados
- Clasificación liga por puntos (resta): suma de puntos − N peores resultados (configurable por liga)
- **Pieza Mayor (PM):** por concurso (mayor BiggestCatchWeight). Cada competición tiene un `BiggestCatchMinWeightInGrams` opcional (configurable por zona): solo se considera "pieza mayor" si supera ese umbral. Sin configurar → cualquier captura válida. Control manual del admin.
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
    - Sistema de puntos: el 1º recibe 20 puntos base de ranking, descendiendo 1 punto por cada puesto sucesivo (1º = 20, 2º = 19, 3º = 18, etc.) hasta la posición 20 (+1 punto). Además, cada asistente recibe MinPoints (default 5) de asistencia. En caso de empate en el top 20, se reparte 1 punto adicional entre los empatados (+1 / nEmpatados a cada uno). Ejemplo: 2º y 3º empatados sin contar asistencia reciben 19.5 y 18.5 puntos respectivamente (con MinPoints = 5, el total es 24.5 y 23.5).
    - Sistema resta: al total de puntos acumulados se le descuentan los N peores resultados (WorstResultsToDiscard).
    - No participar en un concurso = no recibir puntos (no se asigna MinPoints por ausencia).

- **Competition:** Id (Guid), CompetitionNumber (int — nº ordinal dentro de la liga: 1º, 2º... 18º), Name, Date, StartTime, EndTime, Venue (string — escenario: "Bellús", "Pinedo", "Fortaleny"), Zone (string?, nullable — opcional, zona: "Norte", "Sur", "C", "B", "A1-A2-A3"...), Subspecialty (string — "MAR", "AGUA DULCE"), Category (string — "SENIORS", "JUVENIL"), MaxSpots (int), ParticipantCount (int — nº real de participantes), **BiggestCatchMinWeightInGrams (int?, nullable — peso mínimo en gramos para que una captura se considere "pieza mayor"; sin valor = sin mínimo)**, LeagueId (FK). → Pertenece a League, 1:N con CompetitionResults.
  - Método `SetBiggestCatchMinWeight(int?)` en el dominio.
  - Endpoint `PATCH /api/competitions/{id}/biggest-catch-config` body `{ minWeightInGrams: int? }` para actualizar el mínimo en cualquier momento.

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

## Flujo CQRS + Outbox Pattern

### Regla de oro: dónde se lanzan los domain events
Los eventos se lanzan en el **command handler** (Application), NO en el entity (Core).
Esto evita dependencia circular Core → Application (los DTOs de eventos están en Application).
Los métodos de dominio (`Create`, `Update`, `Delete`) son **mutación de estado puro**.

```
POST /api/fishermen/add
  → Controller mapea DTO → FisherManCommand (IRequest<ErrorOr<int>>)
  → MediatR → FisherManAddCommandHandler
    → FluentValidation automático (ValidationPipelineBehavior)
    → Fisherman.Create() [Factory — solo estado, no lanza eventos]
    → fisherman.RaiseDomainEvent(FishermanAddedDomainEvent) [handler lanza evento ANTES de guardar]
    → _genericRepository.AddAsync()
    → SaveChangesAsync()
      → ConvertDomainEventsToOutboxMessagesInterceptor captura eventos de todas las entidades (IHasDomainEvents)
      → Crea OutboxMessage en la MISMA transacción (ACID)
  → ProcessOutboxMessagesJob (cada 10s, AppDomain scan para resolver tipos)
  → FishermanAddedDomainEventHandler procesa
```

**Garantía ACID:** entidad + OutboxMessage se guardan juntos o no se guarda nada.

### Eventos implementados (Fase 3.5)
| Evento | Lanzado en | Handler |
|---|---|---|
| `FishermanAddedDomainEvent` | `FisherManAddCommandHandler` | `FishermanAddedDomainEventHandler` (log) |
| `FishermanUpdatedDomainEvent` | `UpdateFishermanCommandHandler` | `FishermanUpdatedDomainEventHandler` (log) |
| `FishermanDeletedDomainEvent` | `SoftDeleteFishermanCommandHandler` | `FishermanDeletedDomainEventHandler` (log) |

---

## Estándares de Código Backend

### Testing y Mockeo de Datos (AAA obligatorio)

Las pruebas unitarias son críticas para asegurar la fiabilidad y regresiones. Se estructuran estrictamente bajo el patrón **Arrange, Act, Assert (AAA)**.

#### 1. Nomenclatura Estándar de Tests
El nombre de cada método de test debe ser autodescriptivo e indicar claramente el flujo y comportamiento esperado:
`// Naming: NombreMetodo_EscenarioODe entrada_ComportamientoEsperado`
*Ejemplo:* `Handle_DuplicateYear_ShouldReturnDuplicateYearErrorAndNotPersist`

#### 2. FluentAssertions
Se utiliza **FluentAssertions** de forma exclusiva para evaluar los resultados en los asserts:
```csharp
result.IsSuccess.Should().BeTrue();
result.Value.Should().NotBeEmpty();
result.FirstError.Code.Should().Be("League.DuplicateYear");
```
*Evitar:* `Assert.True()`, `Assert.Equal()`.

#### 3. Estándar de Mockeo (Moq)
* **Testing del Core (Dominio):** Las entidades ricas (`League`, `Competition`, `CompetitionResult`) y los Value Objects deben probarse con tests unitarios directos **sin mockeo**. Son métodos mutadores puros de estado que no tocan infraestructura.
* **Handlers e Infrastructure:** Se mockean las dependencias (`IGenericRepository<T, TId>`, `IUnitOfWork`, `ILogger`).
  * **Verificación de Transaccionalidad:** En todo comando de escritura (Commands), se **debe** verificar que se guardaron los cambios llamando a `SaveChangesAsync` exactamente una vez:
    `_mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);`
  * **Mockeo de Guardado (UoW):**
    * *Guardado Exitoso:* `_mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync((ErrorOr<int>)1);`
    * *Error transaccional:* `_mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Error.Conflict("Database.UniqueConstraintViolation", "..."));`

#### 4. Fixtures y Builders de Datos para Pruebas
* Para evitar código repetitivo y boilerplate en la instanciación de entidades dentro del bloque *Arrange*, se utiliza el **Builder Pattern** (`LeagueBuilder`, `CompetitionBuilder`) o se extienden las clases fixtures en `FishClubAlginet.Tests/Data/` (ej: `FisherManFixture`).
* Ejemplo de uso de Builder en Arrange:
  ```csharp
  var league = new LeagueBuilder().WithId(Guid.NewGuid()).Active().Build();
  ```

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
- [x] `PROJECT_STATUS.md` — Manifest técnico completo para handoff a OpenCode Desktop / nuevas sesiones.
- [x] `cline_docs/activeContext.md`, `cline_docs/progress.md`, `domain_model.md` reescritos.

**Fase 3.5 + TASK-C (sesión 2026-05-15)**
- [x] BUG-1: Interceptor generalizado con `IHasDomainEvents` (todas las entidades, no solo `BaseEntity<int>`).
- [x] BUG-2: Job resuelve tipos via AppDomain scan (sin namespace hardcodeado).
- [x] Arq: Interceptor registrado como DI singleton en `Program.cs` (no `new` en `OnConfiguring`).
- [x] Build fix CS0246: `global using ...Interceptors` añadido a `API/GlobalUsing.cs`.
- [x] `FishermanUpdatedDomainEvent` + `FishermanDeletedDomainEvent` + handlers stub.
- [x] `Fisherman.Update()` y `Fisherman.Delete()` implementados (estado puro, evento en handler).
- [x] `UpdateFishermanCommandHandler` (nuevo) con validator.
- [x] `SoftDeleteFishermanCommandHandler` refactorizado: usa entity method + raise event.
- [x] Tests: `UpdateFishermanCommandHandlerTests` (6) + `SoftDeleteFishermanCommandHandlerTests` actualizado (5).

**Fase 4.A — Estados avanzados de concurso + clasificación de liga (sesión 2026-05-16)**
- [x] `Competition` refactorizado a Rich Domain Model: métodos `OpenRegistration`, `CloseRegistration`, `ReopenRegistration` (bool, ventana 30d), `MoveToResultsDraft`, `ValidateResults`.
- [x] `League.Unarchive()` domain method.
- [x] `GetCompetitionByIdQueryHandler` + `GET /competitions/{id}`.
- [x] `ReopenRegistrationCommandHandler` + `PUT /competitions/{id}/reopen-registration`.
- [x] `AssignSpotsCommandHandler` + `POST /competitions/{id}/assign-spots` (orden por `RegistrationDate`).
- [x] `MoveToResultsDraftCommandHandler` + `POST /competitions/{id}/results-draft`.
- [x] `ValidateResultsCommandHandler` + `POST /competitions/{id}/validate-results`.
- [x] `UnarchiveLeagueCommandHandler` + `PUT /leagues/{id}/unarchive`.
- [x] `GetLeagueStandingsQueryHandler` + `GET /leagues/{id}/standings` — `LeagueStandingsDto` con ByWeight + ByPoints (con `WorstResultsToDiscard`).
- [x] `GetAllLeaguesQuery` ampliado con `archived?` bool — filtra archivadas del listado principal.
- [x] Tests: `ReopenRegistrationCommandHandlerTests`, `AssignSpotsCommandHandlerTests`, `MoveToResultsDraftAndValidateTests`, `UnarchiveLeagueCommandHandlerTests`.
- [x] Frontend: `ConfirmationModal`, `AdminArchivedLeaguesPage`, `LeagueStandingsPage` (nueva).
- [x] Frontend: `AdminLeaguesPage`, `AdminCompetitionsPage`, `CompetitionResultsPage` mejorados con guards de estado.
- [x] Frontend: `types/competition.ts` — `LeagueFishermanStandingDto`, `LeagueStandingsDto`.
- [x] Frontend: `leaguesApi.ts` + `competitionsApi.ts` + `endpoints.ts` — todos los endpoints nuevos.
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

### ✅ Completado — Phase 3 + Fase 4.A: Concursos y Resultados (gestión avanzada de estados)

> Gestión completa de concursos: crear, transiciones de estado (Planned→RegistrationOpen→Closed→ResultsDraft→ResultsValidated), inscripción de pescadores, asignación de pesqueras, registro de resultados, consulta de ranking, clasificación de liga. Frontend con panel de estado completo y guards.

#### Backend — Entidades

- [x] Entidad `Competition : BaseEntity<Guid>` con todos los campos definidos (LeagueId, CompetitionNumber, Date, Venue/Zone libres, Subspecialty, Category, MaxSpots, ParticipantCount, `Status` con enum `Planned/RegistrationOpen/Closed/ResultsDraft/ResultsValidated`).
  - ✅ **Rich Domain Model** (métodos: `OpenRegistration`, `CloseRegistration`, `ReopenRegistration`, `MoveToResultsDraft`, `ValidateResults`). Refactor completado en Fase 4.A.
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

#### Backend — Fase 4.A completada
- [x] `AssignSpotsCommand` (asignación secuencial por `RegistrationDate`, válido en `RegistrationOpen` o `Closed`)
- [x] `ReopenRegistrationCommand` (ventana ≤30 días desde cierre)
- [x] `MoveToResultsDraftCommand` + `ValidateResultsCommand`
- [x] `UnarchiveLeagueCommand`
- [x] `GetCompetitionByIdQuery` + `GetLeagueStandingsQuery`

#### Backend — Pendiente (deuda técnica)
- [ ] `EnterResultsCommand` bulk (introducir todos los pesos en una sola operación)
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

### ✅ Phase 3.5: Estabilización del Outbox Pattern (COMPLETADA 2026-05-15)

- [x] **BUG-1** corregido: Interceptor generalizado con `IHasDomainEvents` — captura eventos de todas las entidades, no solo `BaseEntity<int>`.
- [x] **BUG-2** corregido: Job resuelve tipos via AppDomain scan (sin namespace hardcodeado).
- [x] Interceptor registrado como DI singleton en `Program.cs`.
- [x] Build fix CS0246: `global using ...Interceptors` añadido a `API/GlobalUsing.cs`.
- [x] `FishermanUpdatedDomainEvent` + `FishermanDeletedDomainEvent` + handlers stub.
- [x] `Fisherman.Update()` y `Fisherman.Delete()` implementados (estado puro, evento en handler).
- [x] `UpdateFishermanCommandHandler` (nuevo). `SoftDeleteFishermanCommandHandler` refactorizado.
- [x] Tests: `UpdateFishermanCommandHandlerTests` (6) + `SoftDeleteFishermanCommandHandlerTests` actualizado (5).

---

### ✅ Fase 4: Estados avanzados + Clasificación básica (COMPLETADA 2026-05-16)

> Transiciones de estado completas, ligas archivadas, clasificación de liga simplificada (totales).

- [x] Rich Domain Model en `Competition`: `OpenRegistration`, `CloseRegistration`, `ReopenRegistration` (ventana 30d), `MoveToResultsDraft`, `ValidateResults`.
- [x] `League.Unarchive()` domain method.
- [x] Handlers: `ReopenRegistration`, `AssignSpots`, `MoveToResultsDraft`, `ValidateResults`, `UnarchiveLeague`, `GetCompetitionById`, `GetLeagueStandings`.
- [x] `GetAllLeaguesQuery` ampliado con `archived?` bool.
- [x] Tests: `ReopenRegistrationCommandHandlerTests`, `AssignSpotsCommandHandlerTests`, `MoveToResultsDraftAndValidateTests`, `UnarchiveLeagueCommandHandlerTests`.
- [x] Frontend: `ConfirmationModal`, `AdminArchivedLeaguesPage`, `LeagueStandingsPage` (básica — totales).
- [x] Frontend: `CompetitionResultsPage` con status guard (edición solo en `Closed`/`ResultsDraft`).
- [x] Frontend: `AdminCompetitionsPage` panel de estados completo + `AdminLeaguesPage` mejorada.

> ⚠️ **BUG CRÍTICO PENDIENTE**: `CompetitionResult.RecordResult()` asigna `Points = WeightInGrams` (no el sistema de puntos por ranking). La clasificación "Por Puntos" en `LeagueStandingsPage` muestra gramos, no puntos reales. **Bloqueante para Fase 5.**

---

### ✅ Completado — Fase 5.A: PointsCalculator + Pieza Mayor config (2026-05-22)

#### 5.A — Sistema de puntos rediseñado

El sistema de puntos fue completamente rediseñado a petición del club. El nuevo algoritmo (clase `PointsCalculatorService`):

1. **Asistencia base:** todo participante que acude recibe `League.MinPoints` (= 5) solo por presentarse.
2. **Bonus de ranking fijo:** posición 1 → +20, posición 2 → +19, … posición 20 → +1, posiciones 21+ → +0.
3. **Empates dentro del top 20:** cada empatado conserva sus puntos de posición individual y recibe un bonus de `+1/nEmpatados`. Ej: 2º y 3º empatados → 2º = 5+19+0.5 = **24.5 pts**, 3º = 5+18+0.5 = **23.5 pts** (1 punto repartido entre ambos).
4. **Ausencia:** 0 puntos (sin asistencia no se aplica el mínimo).

Implementación:
- [x] `IPointsCalculator` (Core) — interfaz del domain service, docs actualizados.
- [x] `PointsCalculatorService` (Application/Services) — algoritmo reescrito con constantes `FirstPlaceBonus = 20`, `MaxRankedPositions = 20`.
- [x] Se invoca automáticamente en `MoveToResultsDraftCommandHandler` con `League.MinPoints`.
- [x] 11 tests unitarios en `PointsCalculatorServiceTests` cubren: ranking normal, posiciones más allá del 20, empates 2-way, empates 3-way, empate en frontera posición 20/21, empate fuera del top 20, no-asistentes, edge cases.

#### 5.A.bis — Pieza Mayor: mínimo de peso configurable por competición

- [x] `Competition.BiggestCatchMinWeightInGrams` (int?, nullable) — campo nuevo con migración `20260522174249_AddBiggestCatchMinWeightToCompetitions`.
- [x] `Competition.SetBiggestCatchMinWeight(int?)` — método de dominio.
- [x] `CreateCompetitionCommand` ampliado con `BiggestCatchMinWeightInGrams? = null`.
- [x] `UpdateBiggestCatchConfigCommandHandler` — nuevo comando + handler `PATCH /api/competitions/{id}/biggest-catch-config`.
- [x] `CompetitionDto` ampliado con `BiggestCatchMinWeightInGrams?` — ambas queries (`GetById`, `GetByLeague`) lo incluyen.
- [x] Frontend — `CreateCompetitionModal`: nuevo `NumberInput` opcional "Peso mínimo pieza mayor".
- [x] Frontend — `CompetitionResultsPage`: sección inline para leer/actualizar el mínimo en tiempo real.

- [x] **Fase 5.B: Clasificación Detallada (Matriz por Concurso - COMPLETADO 2026-05-25)**
  - [x] Crear `GetLeagueStandingsMatrixQuery` y su handler `GetLeagueStandingsMatrixQueryHandler` para devolver la matriz completa de la clasificación de la liga por puntos y peso.
  - [x] Definir contratos DTOs matriciales: `CompetitionHeaderDto`, `CompetitionCellDto`, `FishermanMatrixRowDto` y `LeagueStandingsMatrixDto`.
  - [x] Implementar lógica robusta de descartes secuenciales (`WorstResultsToDiscard`): se ordenan ascendentemente por puntos las jornadas asistidas por cada pescador, descartando las de menor puntaje e ignorando las inasistencias.
  - [x] Mapear el nuevo endpoint `GET /api/leagues/{id}/standings-matrix` protegido bajo roles `Admin` y `Fisherman`.
  - [x] Desarrollar suite de pruebas unitarias cubriendo todas las especificaciones sintácticas y semánticas, con 193/193 pruebas superadas.

### 🔲 Pendiente — Fase 5.C-E: Pieza Mayor, Frontend de Matriz y Snapshots

> **Análisis basado en `18º - CONCURSO.xls`, `LIGA POR PESO 2025.xls` y `LIGA RESTA 2025.xls`**.
>
> Las clasificaciones son **vistas calculadas** sobre los `CompetitionResult` ya validados. No se persisten (siempre frescas), pero se puede snapshotear al cerrar temporada.

#### 5.C — Pieza Mayor

- [ ] `GetSeasonBiggestCatchQuery(leagueId)` → pescador, peso, concurso. Confirmado: "PM CRISTIAN VOINESCU — 4870 gr" (`LIGA RESTA 2025.xls`).
- [ ] `GetCompetitionBiggestCatchQuery(competitionId)` → mayor `BiggestCatchWeight` del concurso.
- [ ] Incluido en `CompetitionResultDto` y en la sección del acta.

#### 5.D — Frontend clasificación detallada

- [ ] `LeagueStandingsPage` ampliada con **matriz de scroll horizontal**: columnas Posición | Nombre | [C1] [C2] ... [CN] | Total. Última fila = totales por concurso. Exportable a Excel.
- [ ] Pestaña "Pieza Mayor" (`/leagues/{id}/biggest-catches`): top temporada + por concurso.
- [ ] Widget en `/home` con resumen liga activa (top 3 peso + top 3 puntos + última pieza mayor).

#### 5.E — Snapshots opcionales (al archivar temporada)

- [ ] Entidad `LeagueSeasonSnapshot` (Guid, LeagueId, CapturedAt, JsonPayload).
- [ ] `ArchiveLeagueWithSnapshotCommand` — archiva y guarda snapshot JSON de la clasificación final.

---

### 🔲 Pendiente — Fase 6: Acta Oficial FPCV (Word/PDF)

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

### 🔲 Pendiente — Fase 7: Frontend rol Fisherman (Pescador)

- [ ] Página **Calendario** (`/calendar`) — lista de concursos de la liga activa con estado, fecha, venue.
- [ ] Página **Mis inscripciones** (`/my-registrations`) — lista de concursos en los que el pescador está inscrito, con su puesto asignado y resultado si ya está validado.
- [ ] Navegación del sidebar para rol `Fisherman`: Inicio, Calendario, Mis inscripciones, Perfil.

---

### 🔲 Pendiente — Fase 8: Estadísticas y Reporting (opcional / futura)
- [ ] Dashboard global del club: nº pescadores activos, nº concursos celebrados por temporada, evolución de la pesca (kg por año), top historico de pieza mayor
- [ ] Gráficos con `recharts` (ya disponible en stack React)
- [ ] Comparativa entre temporadas (ligas históricas)
- [ ] Exportación de cualquier vista a Excel (xlsx)

---

## Deuda técnica (priorizada)

| Prio | Item | Estado |
|------|------|--------|
| 🟡 Alta | Race condition en `RegisterFishermanCommandHandler` (último spot) | Pendiente |
| 🟡 Alta | Squashar migraciones `InitialSqlServer` + `Initial` antes del primer deploy real | Pendiente |
| 🟡 Alta | Verificar rotación de `JWT_SECRET_KEY` si llegó a remoto en historial git | Pendiente |
| 🟢 Baja | Eliminar o implementar `IFishermanRepository` (interfaz vacía en `Core/Abstractions/`) | ✅ Completado (Eliminada) |
| 🟢 Baja | Índice único sobre `Fisherman.FederationNumber` + regex `^V-\d+$` | Pendiente |
| ✅ Resuelta | `Points = WeightInGrams` bug — PointsCalculator rediseñado con escala fija | Hecho Fase 5.A (2026-05-22) |
| ✅ Resuelta | `Competition` Anemic Model → Rich Domain Model | Hecho Fase 4.A |
| ✅ Resuelta | `ValidationBehavior<,>` al pipeline MediatR | Hecho Fase 3 |

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
