# Contexto Activo: Fase 3.5 — Estabilización del Outbox Pattern

## Estado
Fase 3 (Concursos y Resultados) cerrada funcionalmente. Antes de continuar con la Fase 3 extendida (AssignSpots, EnterResults, ClasificacionGeneral) hay que **estabilizar el Outbox Pattern**, que tiene dos bugs silenciosos detectados el 2026-05-14.

## 🔴 Foco actual: bugs críticos del Outbox

### BUG-1 — El interceptor solo captura eventos de `Fisherman`
- **Archivo:** `FishClubAlginet.Infrastructure/Persistence/Interceptors/ConvertDomainEventsToOutboxMessagesInterceptor.cs:19`
- **Síntoma:** `dbContext.ChangeTracker.Entries<BaseEntity<int>>()` filtra por `BaseEntity<int>`. Solo `Fisherman` cumple. Los `RaiseDomainEvent()` invocados en `League`, `Competition`, `CompetitionResult` (todas `BaseEntity<Guid>`) **se pierden silenciosamente**: se limpian del entity al guardar pero nunca se persisten como `OutboxMessage`.
- **Impacto:** Cualquier side effect basado en eventos de dominio de Ligas o Competiciones jamás se dispara.

### BUG-2 — El job solo deserializa tipos del namespace Fishermen
- **Archivo:** `FishClubAlginet.API/Infrastructure/BackgroundJobs/ProcessOutboxMessagesJob.cs:53`
- **Síntoma:** `var typeName = $"FishClubAlginet.Application.Features.Events.Commands.Fishermen.{outboxMessage.Type}, FishClubAlginet.Application";` hardcoded.
- **Impacto:** Aunque arreglemos BUG-1, los eventos de otros bounded contexts saldrán como "Type not found" y quedarán con `Error` rellenado en la tabla `OutboxMessages`.

### TODOs pendientes ligados
- `FishClubAlginet.Core/Domain/Entities/Fisherman.cs:53` — `Update()` con `FishermanUpdatedDomainEvent` (comentado).
- `FishClubAlginet.Core/Domain/Entities/Fisherman.cs:76` — `Delete()` con `FishermanDeletedDomainEvent` (comentado).

## Lo que sigue funcionando
- CRUD de concursos dentro de una liga (Admin).
- Inscripción de pescadores a concursos abiertos.
- Consulta de resultados con ranking en tiempo real.
- Outbox **solo para eventos de Fisherman** (un único evento implementado: `FishermanAddedDomainEvent`).

## Lo que está bloqueado por los bugs
- Cualquier nuevo `DomainEvent` que se quiera disparar desde `League.Activate/Archive/Update`, `Competition.*`, o `CompetitionResult.*` (silently no-op hasta arreglar BUG-1).
- Notificaciones / proyecciones / side effects de Ligas y Competiciones.

## Rama activa
`branch_phase_three` (cerrar y mergear tras la estabilización antes de abrir `branch_outbox_fix`).

## Próximos commits (atómicos, en este orden)
1. **TASK-A** — Generalizar interceptor con interfaz `IHasDomainEvents` no genérica. *Ver `progress.md` → Fase 3.5*.
2. **TASK-B** — Resolver de tipos basado en `AssemblyQualifiedName` o diccionario al startup.
3. **TASK-C** — Descomentar `Fisherman.Update()` y `Fisherman.Delete()` + crear los dos domain events + handlers stub.

## Notas para el siguiente sesión de Cline
- El `PROJECT_STATUS.md` en la raíz del repo tiene el análisis técnico completo (versiones, esquema DB, fragmentos clave).
- Stack ahora dockerizado: `docker compose up -d` lanza db + api + frontend. Variables en `.env` (gitignored).
- Portainer disponible en `http://localhost:19100` vía `docker-compose.tools.yml`.

---

## ✅ Completado en sesión 2026-05-15 (Capa Application Fase 3)

### Implementado
- `Behaviors/ValidationPipelineBehavior.cs` — pipeline MediatR que ejecuta FluentValidation automáticamente antes de cada handler. Usa constraint `IErrorOr` + dynamic cast.
- `DependencyInjection.cs` — `AddApplication()` registra MediatR + `AddOpenBehavior(ValidationPipelineBehavior)` + `AddValidatorsFromAssembly`. Migrado desde `Program.cs`.
- `Application.csproj` — añadidos `MediatR 14.1.0`, `FluentValidation.DI`, `DI.Abstractions`.
- Validators añadidos a `OpenRegistrationCommandHandler` y `CloseRegistrationCommandHandler` (patrón compacto).
- Tests: `ValidationPipelineBehaviorTests` (5 casos), `CreateCompetitionCommandHandlerTests` (8 casos), `RegisterFishermanCommandHandlerTests` (8 casos), `OpenCloseRegistrationCommandHandlerTests` (7 casos + Theory para transiciones).

### Deuda técnica cerrada
- `ValidationBehavior<,>` al pipeline de MediatR — resuelto. Ya no está en la lista de pendientes.

---

## 📋 Nuevos requisitos capturados (2026-05-15) — para Fase 4

> Requisitos funcionales adicionales sobre gestión de estados de concurso y ligas. Detalle completo en `cline_docs/progress.md → Fase 4`.

### Gestión de estados de Competition (UI + backend)
- **Todas las transiciones de estado** deben mostrarse con un **modal de confirmación** (Admin).
- **Reabrir inscripción** (`Closed → RegistrationOpen`): permitido solo si han pasado ≤ 30 días desde el cierre. Requiere nuevo `ReopenRegistrationCommand`.
- **Imputar resultados** (modal de peso): solo visible cuando el concurso está en estado `Closed` o `ResultsDraft`. Hoy no hay guardia de estado en el frontend.

### Gestión de ligas (UI + backend)
- **Archivar liga**: debe mostrar modal de confirmación antes de ejecutar. Hoy no hay modal.
- **Desarchivar liga**: nueva funcionalidad. Requiere nuevo `UnarchiveLeagueCommand` + endpoint PUT + modal de confirmación.
- **Ligas archivadas**: ocultas por defecto en la lista principal. Nueva página `AdminArchivedLeaguesPage` para ver históricos.
