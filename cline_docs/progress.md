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

## 🔧 Fase 3.5: Estabilización del Outbox Pattern (EN CURSO)

> Bugs silenciosos detectados el 2026-05-14 al auditar el código para `PROJECT_STATUS.md`. Bloquean cualquier uso del Outbox fuera de la entidad `Fisherman`. Resolver antes de añadir nuevos `DomainEvent` a Ligas/Competiciones.

### TASK-A — Interceptor genérico para todas las entidades con eventos
- [ ] Crear interfaz no genérica `IHasDomainEvents` en `Core/Domain/Entities/BaseEntity.cs` con los métodos `GetDomainEvents()`, `ClearDomainEvents()`, `RaiseDomainEvent(IDomainEvent)`.
- [ ] Hacer que `BaseEntity<TId>` implemente `IHasDomainEvents`.
- [ ] Cambiar `Entries<BaseEntity<int>>()` → `Entries<IHasDomainEvents>()` en `ConvertDomainEventsToOutboxMessagesInterceptor.cs:19`.
- [ ] Test de integración: crear `League`, llamar `Activate()`, persistir, verificar que existe un `OutboxMessage` con `Type = "LeagueActivatedDomainEvent"`.

### TASK-B — Resolver de tipos generalizado en el job
- [ ] Sustituir el namespace hardcoded en `ProcessOutboxMessagesJob.cs:53`. Dos opciones:
  - **Opción 1 (mínimo cambio):** persistir `domainEvent.GetType().AssemblyQualifiedName` en `OutboxMessage.Type` en lugar del `Name` simple. El job lo deserializa con `Type.GetType(outboxMessage.Type)`.
  - **Opción 2 (más robusto):** construir un diccionario `string → Type` al startup escaneando el ensamblado de Application: `typeof(IDomainEvent).Assembly.GetTypes().Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && !t.IsAbstract)`.
- [ ] Decidir y documentar la opción elegida en `systemPatterns.md` (crear si no existe).
- [ ] Migración EF Core si se cambia el `MaxLength` de `OutboxMessage.Type` (Opción 1 implica strings más largos).
- [ ] Verificar retro-compatibilidad con mensajes ya persistidos en la tabla (si quedan `Error` sin reprocesar, decidir si purgar o migrar).

### TASK-C — Cerrar TODOs de `Fisherman` (Update + Delete con domain events)
- [ ] Crear `FishClubAlginet.Application/Features/Events/Commands/Fishermen/FishermanUpdatedDomainEvent.cs` siguiendo el patrón de `FishermanAddedDomainEvent`.
- [ ] Crear `FishClubAlginet.Application/Features/Events/Commands/Fishermen/FishermanDeletedDomainEvent.cs`.
- [ ] Descomentar `Fisherman.Update()` en `Fisherman.cs:53`.
- [ ] Descomentar `Fisherman.Delete()` en `Fisherman.cs:76`.
- [ ] Crear handlers stub: `FishermanUpdatedDomainEventHandler` y `FishermanDeletedDomainEventHandler` (aunque solo loguen) para que el job no escupa "Type not found".
- [ ] Test de handler de Fisherman: `Update()` y `Delete()` raise the expected events.

### Definition of Done de Fase 3.5
- [ ] Los tres eventos (`FishermanAdded`, `FishermanUpdated`, `FishermanDeleted`) circulan por el Outbox end-to-end.
- [ ] Un evento nuevo en `League` (ejemplo: `LeagueActivatedDomainEvent`) también circula end-to-end sin tocar el interceptor ni el job.
- [ ] Cero registros con `Error IS NOT NULL` en `OutboxMessages` tras un ciclo limpio.

---

## Fase 4 (pendiente, después de 3.5)

- [ ] Implementar asignación de puestos de sorteo (`AssignSpotsCommand`).
- [ ] Implementar entrada de resultados bulk (`EnterResultsCommand`): `DidAttend`, `WeightInGrams`, `BiggestCatchWeight`.
- [ ] Implementar transición de estado (`RegistrationOpen` → `Closed` → `ResultsDraft` → `ResultsValidated`).
- [ ] Clasificación general por liga (suma de puntos descartando `worstResultsToDiscard` peores).
- [ ] Tests del módulo Competitions (ahora mismo sin cobertura).
- [ ] Revisar concurrencia en `RegisterFishermanCommandHandler` (race condition potencial cuando dos pescadores compiten por el último spot).

## Deuda técnica detectada (no bloqueante)

- [ ] Eliminar o implementar `IFishermanRepository` (declarada vacía en `Core/Abstractions/IFishermanRepository.cs`).
- [ ] Convertir `Competition` a Rich Domain Model (hoy es anémico, contraste con `League`).
- [ ] Añadir `ValidationBehavior<TRequest,TResponse>` al pipeline de MediatR. Hoy los validators están registrados pero **no se disparan automáticamente** al enviar un Command.
- [ ] Squashar las dos migraciones "iniciales" (`InitialSqlServer` + `Initial`) antes del primer deploy real.
- [ ] Revisar si quedan referencias al antiguo `JWT_SECRET_KEY` commiteado en historial; considerar rotación + force-push si llegó a remoto.
