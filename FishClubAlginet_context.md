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
### 🔲 Pendiente — Phase 2: League Management
-[ ] Entidad `League` (con MinPoints, WorstResultsToDiscard) + handlers MediatR (backend)
-[ ] Añadir campo `FederationNumber` a entidad Fisherman existente + migración
-[ ] Migración EF Core
-[ ] UI React: League Management Dashboard (CRUD ligas, configuración puntuación)

### 🔲 Pendiente — Phase 3: Competitions & Results
-[ ] Entidad `Competition` (con CompetitionNumber, Venue, Zone, Subspecialty, Category, ParticipantCount) + handlers MediatR (backend)
-[ ] Entidad `CompetitionResult` (con AssignedSpotNumber, WeightInGrams, BiggestCatchWeight, Points, Ranking) + handlers MediatR (backend)
-[ ] Lógica de cálculo automático de puntos (ranking inverso, empates, mínimo 5)
-[ ] Migración EF Core
-[ ] UI React: Calendario de competiciones + inscripción para Fishermen
-[ ] UI React: Entrada de resultados por concurso (peso, pieza mayor, asignación de puestos)
-[ ] UI React: Clasificación de liga en tiempo real (por peso y por puntos/resta)
-[ ] UI React: Premio Pieza Mayor (por concurso y por temporada)
-[ ] Generación de Acta oficial (PDF) para FPCV — formato federativo

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
