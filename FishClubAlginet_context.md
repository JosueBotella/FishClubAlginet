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
- [x] `.env.example` raíz con `SA_PASSWORD`, `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_DURATION_MINUTES`, `PUBLIC_HTTP_PORT`
- [x] `.dockerignore` raíz (excluye `bin/`, `obj/`, `node_modules/`, `.env`, `.git`)
- [x] Ajustes en `Program.cs`: `UseHttpsRedirection` solo fuera de container; `MapInboundClaims = false` en JwtBearer (fix de 403 por mapping legacy de claims)
- [x] `appsettings.json` y `appsettings.Development.json` con secrets vacíos (se inyectan vía env vars `ConnectionStrings__LocalConnectionString` y `JwtSettings__SecretKey`)
- [x] `vite.config.ts` con `VITE_PROXY_TARGET` configurable (Docker → `http://api:8080`, local → `https://localhost:7179`)
- [x] README actualizado con sección "🚢 Arranque rápido con Docker" + tabla de comandos
- [x] Refactor a patrón Unit of Work consistente: `IUnitOfWork.SaveChangesAsync` devuelve `ErrorOr<int>`, mapeo de excepciones EF (`DbUpdateException` con `SqlException 2627/2601` → `Error.Conflict`) centralizado en `UnitOfWorkService` (Infrastructure); handlers de Application sin dependencias de EF
- [x] Tests unitarios actualizados al nuevo patrón UoW (`SoftDeleteFishermanCommandHandlerTests` y `FisherManAddCommanHandlerTests`) con verificación explícita de `SaveChangesAsync` invocado/no invocado según flujo
### 🔲 Pendiente — Phase 2: Gestión de Ligas

> **Análisis basado en `LIGA POR PESO 2025.xls` y `LIGA RESTA 2025.xls`** (ver carpeta `Concurso 18/`).
>
> Una **Liga** es una temporada anual de competiciones (1 enero – 31 diciembre) que agrupa varios concursos. Para cada liga se generan **dos clasificaciones independientes** que el club mantiene en paralelo: por **peso total acumulado** (gramos) y por **sistema de puntos con resta**. Ambas usan los mismos `CompetitionResult` como fuente, pero los presentan distinto.

#### Backend
- [ ] Entidad `League` (Domain): `Id` (Guid), `Name`, `Year` (int), `IsActive` (bool), `MinPoints` (int, default 5), `WorstResultsToDiscard` (int, default 0).
  - Reglas:
    - 1 League ↔ N Competitions
    - **Solo una `League` activa simultáneamente en todo el club** (no hay separación por subespecialidad: una sola liga sirve para Mar y Agua Dulce, Seniors y Juvenil)
    - Al crear, validar que no exista otra liga del mismo `Year`
    - Al activar una liga, las demás se desactivan automáticamente (`ActivateLeagueCommand` se encarga de la transición)
    - `MinPoints` y `WorstResultsToDiscard` se aplican al cálculo de la clasificación por puntos
- [ ] DTOs: `CreateLeagueRequest`, `UpdateLeagueRequest`, `LeagueDto`, `LeagueWithCompetitionsCountDto` (incluye nº concursos celebrados / planificados)
- [ ] Commands MediatR: `CreateLeagueCommand`, `UpdateLeagueCommand`, `ActivateLeagueCommand` (desactiva las demás del mismo año), `ArchiveLeagueCommand`
- [ ] Queries MediatR: `GetAllLeaguesQuery` (paginada con filtro por año), `GetLeagueByIdQuery`, `GetActiveLeagueQuery`
- [ ] FluentValidation: `Year` entre 2000 y currentYear + 1; `Name` no vacío y máx 100 chars; `MinPoints` >= 0; `WorstResultsToDiscard` >= 0
- [ ] Endpoint REST: `[Authorize(Roles="Admin")]` para mutaciones, lectura abierta a Admin/Fisherman
- [ ] Tests unitarios de los handlers (xUnit + Moq + UoW pattern, igual que el resto)

#### Ampliación a entidad existente `Fisherman`
- [ ] Añadir campo `FederationNumber` (string, ej: "V-552") — **identificador federativo único** dentro del club, distinto de `FederationLicense` que puede ser una cadena más extensa
  - Reglas:
    - Único por club (validar `Database.UniqueConstraintViolation`)
    - Formato `^V-\d+$` (regex configurable)
    - Obligatorio si el pescador participa en concursos oficiales
- [ ] Migración EF Core que añade `FederationNumber` con índice único (nullable inicialmente para registros existentes; backfill posterior)
- [ ] Actualizar `FishermanProfileDto` y `FishermanDto` con el nuevo campo
- [ ] Actualizar tests existentes (compatibilidad)

#### Frontend
- [ ] Página **Admin → Ligas** (`/admin/leagues`): grid de ligas con `Year`, `Name`, estado (Activa / Histórica), nº concursos celebrados, acciones (Editar, Activar, Archivar)
- [ ] Modal **Crear / Editar Liga** con campos `Name`, `Year`, `MinPoints`, `WorstResultsToDiscard`, validación cliente
- [ ] Indicador en sidebar de la **liga activa** (ej: badge con "Liga 2026" en el AppLayout)
- [ ] Actualizar `EditFishermanModal` (cuando exista) para incluir `FederationNumber`

---

### 🔲 Pendiente — Phase 3: Concursos y Resultados

> **Análisis basado en `18º - CONCURSO.xls`**.
>
> Un **Concurso** (`Competition`) es una jornada concreta dentro de una liga. Tiene escenario, zona, subespecialidad, fecha y un cupo máximo. Los pescadores se inscriben previamente, el día del concurso se les asigna un puesto pesquera por sorteo, y al finalizar se registran sus pesos. El sistema calcula automáticamente puntos y rankings.

#### Backend — Entidades

- [ ] Entidad `Competition` (Domain):
  - Campos: `Id` (Guid), `LeagueId` (FK), `CompetitionNumber` (int, ordinal en la liga: 1º, 2º, ... 18º), `Name` (opcional), `Date`, `StartTime`, `EndTime`, `Venue` (string libre: "BELLUS", "PINEDO", "FORTALENY"...), `Zone` (string libre: "C", "B", "SUR", "NORTE", "A1-A2-A3", "B1-B2-B3", "E1-E2-E3"...), `Subspecialty` (enum: `Mar`, `AguaDulce`), `Category` (enum: `Seniors`, `Juvenil`), `MaxSpots` (int), `Status` (enum: `Planned`, `RegistrationOpen`, `Closed`, `ResultsDraft`, `ResultsValidated`)
    - **Decisión**: `Venue` y `Zone` son **strings libres**, no entidades catálogo. El club los introduce manualmente al crear cada concurso. Se podría sugerir autocompletado en el frontend leyendo valores ya usados, pero sin forzar el modelo.
  - Reglas:
    - `CompetitionNumber` único dentro de la misma liga
    - `Date` debe estar dentro del año de la `League`
    - `MaxSpots` > 0
    - El paso a `ResultsValidated` requiere que TODOS los inscritos tengan `CompetitionResult` registrado
- [ ] Entidad `CompetitionResult` (Domain) — combina inscripción + resultado:
  - Campos: `Id` (Guid), `CompetitionId` (FK), `FishermanId` (FK), `AssignedSpotNumber` (int? — null hasta el sorteo), `DidAttend` (bool), `WeightInGrams` (int — 0 si asistió pero no pescó), `BiggestCatchWeight` (int? — peso de la pieza mayor del concurso si la presentó), `Points` (decimal — calculado), `Ranking` (int — calculado), `RegistrationDate`, `IsValidated` (bool)
  - Reglas:
    - Un pescador solo puede tener un `CompetitionResult` por `Competition` (índice único compuesto)
    - `AssignedSpotNumber` único dentro de la misma `Competition`
    - Si `DidAttend = false`, `WeightInGrams` y `Points` deben ser 0 (vacío en planilla)
    - Si `DidAttend = true` y `WeightInGrams = 0`, recibe `MinPoints` (default 5)
- [ ] DTOs: `RegisterToCompetitionRequest`, `AssignSpotsRequest`, `EnterResultsRequest` (bulk), `CompetitionDto`, `CompetitionDetailDto` (con resultados), `MyRegistrationDto` (vista pescador)

#### Backend — Lógica de cálculo de puntos

> **CONFIRMADO** observando el `18º - CONCURSO.xls` (27 participantes):

Algoritmo `CalculatePointsForCompetition(competition)`:
1. Filtrar resultados con `DidAttend = true`, ordenar **descendente por `WeightInGrams`**
2. Asignar `Ranking` 1, 2, 3, ... aplicando empates (mismo peso → mismo ranking, siguiente salta)
3. Calcular puntos base:
   - 1ª posición recibe `N` puntos donde `N = (nº de posiciones únicas tras resolver empates)`. En el ejemplo del 18º concurso: 27 participantes − 2 empates dobles = **25 puntos al primero**
   - Cada posición posterior recibe 1 punto menos
4. **Empates**: las posiciones empatadas comparten la media de los puntos que les corresponderían individualmente. Ejemplo del 18º concurso:
   - Posiciones 14-15 con 1125 g cada uno → (12 + 11) / 2 = **11,5 puntos cada uno**
   - Posiciones 18-19 con 1010 g cada uno → (8 + 7) / 2 = **7,5 puntos cada uno**
5. **Mínimo de puntos**: nadie por debajo de `League.MinPoints` (default 5). En el 18º concurso, las posiciones 21 a 27 reciben 5 puntos (incluidos los que pesaron 0 g)
6. **Ausencia (`DidAttend = false`)**: `Points = 0` (NO recibe `MinPoints`, sólo recibe puntos quien acude)

- [ ] Servicio de dominio `PointsCalculator` con tests exhaustivos:
  - Caso simple sin empates
  - Caso con empate doble en posiciones intermedias (replicar 18º concurso)
  - Caso con empate múltiple
  - Caso con muchos pescadores en el mínimo (5 pts)
  - Caso con asistencia + 0 g
  - Caso con ausencia
- [ ] Command `CalculateCompetitionPointsCommand` (idempotente: recalcula sin duplicar)
- [ ] Trigger automático: al pasar `Competition.Status` a `ResultsValidated`, se ejecuta `CalculateCompetitionPointsCommand`

#### Backend — Otros handlers
- [ ] Commands: `CreateCompetitionCommand`, `UpdateCompetitionCommand`, `OpenRegistrationsCommand`, `CloseRegistrationsCommand`, `RegisterToCompetitionCommand` (Fisherman se inscribe), `UnregisterCommand`, `AssignSpotsCommand` (Admin sortea — opcional manual / aleatorio), `EnterResultCommand` (Admin introduce peso de un pescador), `EnterResultsBulkCommand` (Admin introduce todos), `ValidateResultsCommand`
- [ ] Queries: `GetCompetitionsByLeagueQuery` (paginada), `GetCompetitionByIdQuery` (con resultados), `GetMyRegistrationsQuery` (pescador ve sus inscripciones), `GetUpcomingCompetitionsQuery`
- [ ] Tests unitarios de cada handler

#### Backend — Migración
- [ ] Migración EF Core que añade tablas `Leagues`, `Competitions`, `CompetitionResults` con índices y FKs
- [ ] Configuraciones EF (FluentAPI) con propiedades, longitudes máximas, enums como strings

#### Frontend — Admin
- [ ] Página **Admin → Concursos** (`/admin/competitions`): grid de concursos de la liga activa, filtro por estado, acciones contextuales según `Status`
- [ ] Modal **Crear / Editar Concurso** con `Date`, `Venue`, `Zone`, `Subspecialty`, `Category`, `MaxSpots`, `CompetitionNumber`
- [ ] Página de detalle de concurso con tabs:
  - **Tab Inscripciones** (estado `RegistrationOpen` / `Closed`): lista de pescadores inscritos, asignar / quitar puestos pesquera (manual o sortear aleatorio)
  - **Tab Resultados** (estado `Closed` o posterior): formulario para introducir peso por pescador (en gramos), pieza mayor (opcional), validación al guardar todos
  - **Tab Clasificación** (estado `ResultsValidated`): tabla con ranking, peso, pieza mayor, puntos calculados — exportable a Excel y a Word/PDF (acta)

#### Frontend — Pescador
- [ ] Página **Calendario** (`/calendar`): listado de concursos próximos en la liga activa, botón "Inscribirme" si `Status = RegistrationOpen` y aún no estoy inscrito
- [ ] Página **Mis inscripciones** (`/my-registrations`): inscripciones futuras y resultados pasados del pescador logado

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
- **API:** https://localhost:7179
- **React:** http://localhost:5173 (Vite dev server, proxy → API)
- **Repo:** https://github.com/JosueBotella/FishClubAlginet

## Ejecutar proyecto
```bash
dotnet ef database update --project FishClubAlginet.Infrastructure --startup-project FishClubAlginet.API
# Luego arrancar API + React dev server simultáneamente
```
