# Domain Model — FishClubAlginet

> Snapshot del modelo de dominio real verificado contra el codigo en `FishClubAlginet.Core/Domain/Entities/` el 2026-05-14.
> Reemplaza la version obsoleta que listaba entidades por implementar.

---

## Base generica

### `BaseEntity<TId>` *(Core/Domain/Entities)*
Clase abstracta de la que heredan todas las entidades del dominio.

```csharp
public abstract class BaseEntity<TId>
{
    public required TId Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedTimeUtc { get; set; }
    public DateTime LastUpdateUtc { get; set; }

    // Domain events (in-memory, drenados por el SaveChangesInterceptor)
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    public void ClearDomainEvents();
    public void RaiseDomainEvent(IDomainEvent domainEvent);
}
```

**BUG conocido (ver `cline_docs/activeContext.md`):** el interceptor `ConvertDomainEventsToOutboxMessagesInterceptor` filtra hoy por `BaseEntity<int>`, por lo que **solo los domain events de `Fisherman` llegan al Outbox**. League, Competition, CompetitionResult y OutboxMessage son `BaseEntity<Guid>` y sus eventos se descartan silenciosamente. Pendiente de TASK-A.

---

## Entidades

### `Fisherman : BaseEntity<int>` — Socio del club

| Campo | Tipo | Notas |
|---|---|---|
| FirstName, LastName | string | Max 50 cada uno |
| DateOfBirth | DateTime | |
| DocumentType | `TypeNationalIdentifier` enum | DNI por defecto |
| DocumentNumber | string | Max 20, min 10 |
| FederationLicense | string? | Max 20. ID federativo amplio (opcional) |
| FederationNumber | string? | Identificador federativo unico, ej. `V-552` (opcional) |
| RegionalLicense | string? | Licencia GVA (opcional) |
| Address | `Address` (Value Object) | Owned entity |
| UserId | string? | FK logica a `AspNetUsers.Id` |

**Computed:** `IsMinor => DateOfBirth > DateTime.UtcNow.AddYears(-18)`

**Comportamiento de dominio:**
- DONE `Fisherman.Create(...)` — Factory method.
- TODO `Fisherman.Update(...)` — Comentado, pendiente de TASK-C.
- TODO `Fisherman.Delete()` — Comentado, pendiente de TASK-C.

**Identidad por `int`** (autoincrement). Es la unica entidad del dominio que no usa `Guid` como Id; consecuencia: hoy es la unica que efectivamente emite eventos al Outbox.

---

### `League : BaseEntity<Guid>` — Temporada anual del club

| Campo | Tipo | Notas |
|---|---|---|
| Name | string | Max 100 |
| Year | int | >= 2000 |
| IsActive | bool | Solo una `League` activa a la vez en todo el club |
| IsArchived | bool | |
| MinPoints | int | Default 5. Minimo de puntos por asistir y pescar 0 g |
| WorstResultsToDiscard | int | Default 0. Para clasificacion por puntos |
| Competitions | `ICollection<Competition>` | Navegacion, 1:N |

**Comportamiento de dominio (Rich Model — referencia de estilo):**
- `League.Create(name, year, minPoints, worstResultsToDiscard)` — Factory.
- `League.Update(name, minPoints, worstResultsToDiscard)`
- `League.Activate()` / `Deactivate()` / `Archive()`
- `League.Unarchive()` *(pendiente de implementar — Fase 4)* — reversión a `IsArchived=false, IsActive=false`.

**Ciclo de vida de estado:**
```
Created(IsActive=false, IsArchived=false)
    │  Activate()
    ▼
Active(IsActive=true)
    │  Archive()
    ▼
Archived(IsArchived=true, IsActive=false)  ◄──── Unarchive() (pendiente)
```

**Visibilidad:** las ligas archivadas **no aparecen** en la lista principal del Admin; solo son accesibles desde la vista de histórico (`/admin/leagues/archived`). Ver `cline_docs/progress.md → 4.C`.

**Constraints:** `LeagueConstraints.NameMaxLength = 100`, `LeagueConstraints.MinYear = 2000`.

---

### `Competition : BaseEntity<Guid>` — Jornada de pesca dentro de una Liga

| Campo | Tipo | Notas |
|---|---|---|
| LeagueId, League | FK Guid + Navegacion | Cascade delete |
| CompetitionNumber | int | Ordinal dentro de la liga (1, 2, ... 18). **Unico por LeagueId** |
| Name | string | |
| Date | DateTime | |
| StartTime, EndTime | TimeSpan | |
| Venue | string | Escenario libre ("Bellus", "Pinedo"...) |
| Zone | string | Zona libre ("Norte", "C", "A1-A2-A3"...) |
| Subspecialty | `Subspecialty` enum | nvarchar(20) — Mar / AguaDulce |
| Category | `Category` enum | nvarchar(20) — Seniors / Juvenil |
| Status | `CompetitionStatus` enum | nvarchar(30), default `Planned` |
| MaxSpots | int | |
| ParticipantCount | int | |

**Anti-patron detectado:** setters publicos en todos los campos (Anemic Model). Contrasta con `League` que tiene Rich Model. Refactor pendiente en deuda tecnica (`cline_docs/progress.md`).

**Máquina de estados (`CompetitionStatus`) — actualizada 2026-05-15:**

```
                    ┌──────────────────────────────────────┐
                    │  Admin confirma + ≤ 30 días desde    │
                    │  el cierre (ReopenRegistrationCmd)   │
                    ▼                                      │
Planned ──────► RegistrationOpen ──────────────────► Closed
  │                                (CloseRegistration)    │
  │ (OpenRegistration)                                     │
  │                                                        ▼
  └──────────────────────────────────────────────►  ResultsDraft ──► ResultsValidated
                                                    (resultados
                                                     imputados)
```

| Transición | Command existente | Regla de negocio | Confirmación UI |
|---|---|---|---|
| `Planned → RegistrationOpen` | `OpenRegistrationCommand` | estado == Planned | ✅ modal |
| `RegistrationOpen → Closed` | `CloseRegistrationCommand` | estado == RegistrationOpen | ✅ modal |
| `Closed → RegistrationOpen` | `ReopenRegistrationCommand` *(pendiente)* | estado == Closed **y** ≤ 30 días desde cierre | ✅ modal + aviso plazo |
| `Closed → ResultsDraft` | *(pendiente, part of EnterResults flow)* | estado == Closed | ✅ modal |
| `ResultsDraft → ResultsValidated` | *(pendiente)* | estado == ResultsDraft | ✅ modal |

**Guardia de UI para imputar resultados:** el modal de entrada de peso solo se muestra cuando `status == Closed || status == ResultsDraft`. Ver `cline_docs/progress.md → 4.B`.

---

### `CompetitionResult : BaseEntity<Guid>` — Inscripcion + resultado de un pescador en un concurso

> **Combina inscripcion y resultado en una sola entidad.** Sustituye lo que en el modelo inicial se llamaba `CompetitionRegistration` + `FishingSpot`.

| Campo | Tipo | Notas |
|---|---|---|
| CompetitionId, Competition | FK Guid | Cascade |
| FishermanId, Fisherman | FK int | Restrict |
| AssignedSpotNumber | int? | num de pesquera por sorteo. **Unico por CompetitionId** cuando NOT NULL |
| RegistrationDate | DateTime | |
| IsValidated | bool | Default false |
| DidAttend | bool | Default false |
| WeightInGrams | int | Default 0 |
| BiggestCatchWeight | int? | Para premio "Pieza Mayor" |
| Points | decimal(18,2) | Default 0. Calculados al validar resultados |
| Ranking | int | Default 0. Posicion en el concurso |

**Constraints unicos:**
- `(CompetitionId, FishermanId)` — un pescador, un resultado por concurso
- `(CompetitionId, AssignedSpotNumber)` WHERE `AssignedSpotNumber IS NOT NULL` — no dos pescadores en la misma pesquera

---

### `OutboxMessage : BaseEntity<Guid>` — Mensajeria de eventos de dominio

| Campo | Tipo | Notas |
|---|---|---|
| OccurredOnUtc | DateTime | Momento en que se genero el evento |
| ProcessedOnUtc | DateTime? | NULL si pendiente; UtcNow si procesado |
| Type | string | Nombre simple del tipo de evento. **BUG-2:** hoy se persiste el `Name` simple, no `AssemblyQualifiedName` |
| Content | string (NVARCHAR MAX) | JSON serializado del `IDomainEvent` |
| Error | string? | Mensaje de error si falla el procesamiento |

**Flujo:**
1. Handler invoca `entity.RaiseDomainEvent(evt)`.
2. `IUnitOfWork.SaveChangesAsync()` dispara `ConvertDomainEventsToOutboxMessagesInterceptor` (SaveChangesInterceptor).
3. El interceptor drena `entity.GetDomainEvents()` y crea filas en `OutboxMessages` **en la misma transaccion** que la entidad -> garantia ACID.
4. `ProcessOutboxMessagesJob` (BackgroundService, tick 10 s) lee `WHERE ProcessedOnUtc IS NULL ORDER BY OccurredOnUtc`, deserializa y publica via `MediatR.IPublisher`.

---

## Value Objects

### `Address` *(Core/Domain/ValueObjects)*
Objeto valor incrustado en `Fisherman` (Owned Entity en EF, sin tabla propia). Campos tipicos: Street, City, ZipCode, Province.

---

## Enums (`FishClubAlginet.Contracts/Enums`)

| Enum | Valores |
|---|---|
| `Subspecialty` | Mar, AguaDulce |
| `Category` | Seniors, Juvenil |
| `CompetitionStatus` | Planned, RegistrationOpen, Closed, ResultsDraft, ResultsValidated |
| `TypeNationalIdentifier` | Dni, Nie, Passport (a confirmar) |

Todos se persisten como `nvarchar` mediante conversion en las EF Configurations (ver `Infrastructure/Persistence/Configurations/`).

---

## Domain Events implementados

### Solo uno hoy
- **`FishermanAddedDomainEvent`** — Disparado por `Fisherman.Create()`. Procesado por `FishermanAddedDomainEventHandler` (logging).

### Pendientes (TASK-C de `cline_docs/progress.md`)
- `FishermanUpdatedDomainEvent`
- `FishermanDeletedDomainEvent`

### Bloqueados por bugs (TASK-A)
Cualquier `RaiseDomainEvent()` en `League` (Activate, Archive, etc.) o `Competition`/`CompetitionResult` no se persiste al Outbox hasta arreglar el interceptor.

---

## Diagrama de relaciones

```
                              +------------------+
                              |   AspNetUsers    |  (Identity)
                              +--------+---------+
                                       | UserId
                                       v
+-------------+  1   N   +--------------------+
|   League    +----------> Competition         |
| <Guid>      |          | <Guid>              |
+-------------+          +---------+----------+
                                   | 1
                                   |
                                   | N
                          +--------v----------+  N   1   +------------------+
                          | CompetitionResult +----------> Fisherman         |
                          | <Guid>            |          | <int>             |
                          +-------------------+          | Address (owned)   |
                                                         +------------------+

+------------------+
|  OutboxMessage   |  (independiente, alimentado por interceptor)
|  <Guid>          |
+------------------+
```

---

## Reglas de negocio resumidas (extraidas de `FishClubAlginet_context.md`)

- **Una sola Liga activa simultaneamente** en todo el club.
- **Una Liga es anual** (1 enero - 31 diciembre).
- **Solo Admin** crea Ligas, Competiciones y gestiona usuarios.
- **Inscripciones validadas manualmente** por Admin.
- **Puntuacion por concurso:** ranking inverso por peso. Empates -> media de las posiciones. Minimo `League.MinPoints` (default 5) si asiste y pesco cero. Ausencia -> 0 puntos (no recibe `MinPoints`).
- **Clasificacion liga por peso:** suma directa de gramos acumulados.
- **Clasificacion liga por puntos (resta):** suma de puntos - `WorstResultsToDiscard` peores resultados.
- **Premio Pieza Mayor (PM):** mayor `BiggestCatchWeight` por concurso y por temporada.

---

*Para el roadmap de que falta implementar sobre este modelo, ver `cline_docs/progress.md`. Para los bugs activos del Outbox, ver `cline_docs/activeContext.md`.*
