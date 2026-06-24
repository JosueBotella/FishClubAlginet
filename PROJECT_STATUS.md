# FishClubAlginet — Project Status Manifest

**Snapshot:** 2026-05-14
**Repo:** `C:\GIT\JosueBotella\FishClubAlginet`
**Branch activa:** sin verificar (ejecuta `git status` al cargar)
**Confianza global de este documento:** alta en lo verificado en código, media en lo no verificable en runtime sin levantar el stack.

---

## 1. Tech Stack Actual

### Backend (.NET 10.0, target `net10.0` en todos los proyectos)

| Paquete | Versión | Rol |
|---|---|---|
| Microsoft.EntityFrameworkCore | 10.0.7 | ORM |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.7 | Provider SQL Server |
| Microsoft.EntityFrameworkCore.Tools / Design | 10.0.7 | Migraciones |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.7 | Identity stores |
| Microsoft.Extensions.Identity.Core | 10.0.7 | Identity abstractions |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.7 | JWT auth |
| System.IdentityModel.Tokens.Jwt | 8.17.0 | JWT generation |
| Microsoft.AspNetCore.OpenApi | 10.0.7 | OpenAPI builder nativo .NET |
| Scalar.AspNetCore | 2.14.6 | UI de docs (sustituye a Swagger UI) |
| Swashbuckle.AspNetCore | 10.1.7 | Generación XML/Swagger (coexiste con Scalar) |
| MediatR | 14.1.0 | CQRS + Pipeline |
| FluentValidation | 12.1.1 | Validación |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 | Auto-registro de validators |
| ErrorOr | 2.0.1 | Result pattern sin excepciones |

### Frontend (React 19 / Vite 6)

| Paquete | Versión |
|---|---|
| react / react-dom | 19.1.0 |
| react-router-dom | 7.5.0 |
| @mantine/* (core, form, hooks, notifications) | 7.17.0 |
| axios | 1.9.0 |
| jwt-decode | 4.0.0 |
| vite | 6.3.0 |
| typescript | 5.8.3 |
| eslint + typescript-eslint | 9.25 / 8.30 |

### Base de datos

* **SQL Server 2022** (en Docker, imagen `mcr.microsoft.com/mssql/server:2022-latest`).
* **3 migraciones aplicadas** *(orden de timestamp)*:
  1. `20260414144815_InitialSqlServer`
  2. `20260506183057_Initial` *(nombre confuso, parece refactor post-inicial)*
  3. `20260508180238_AddCompetitions`

### Testing

* xUnit + Moq + NSubstitute. **Cobertura buena en Leagues/Fishermen/Users/Auth/Validators; ausente en Competitions.**

---

## 2. Arquitectura del Sistema

### Topología de capas (Clean Architecture, regla de dependencia respetada)

```
Contracts  ──────────────►  Core (Domain) ◄────────  Application
                                  ▲                       ▲
                                  └────  Infrastructure ──┘
                                                ▲
                                              API
```

* **Contracts** — DTOs + Enums (sin dependencias externas).
* **Core** — Entidades, Value Objects, Domain Events, contratos de repositorio (`IGenericRepository`, `IUnitOfWork`).
* **Application** — Handlers MediatR (Commands + Queries), Validators, abstracciones de servicios (`IAuthService`, `IUserManagementService`).
* **Infrastructure** — `AppDbContext` (IdentityDbContext), implementación de repos/UoW, EF configurations, interceptors, seeds, services concretos.
* **API** — Controllers (delgados), `Program.cs`, BackgroundJobs (Outbox), composition root.

### Flujo de una petición (write path)

```
HTTP Request
   │
   ▼
Controller (UsersController, FisherMenController, ...)
   │   delega vía MediatR.Send(command)
   ▼
CommandHandler (Application/Features/<Feature>)
   │   1. Validación automática vía FluentValidation pipeline (no implementada como behavior — revisar)
   │   2. Llama a IUnitOfWork.Repository<T,TId>() para Add/Update
   │   3. Invoca métodos de dominio (entity.Activate(), entity.RaiseDomainEvent(...))
   │   4. await IUnitOfWork.SaveChangesAsync()  →  ErrorOr<int>
   ▼
SaveChangesInterceptor (ConvertDomainEventsToOutboxMessagesInterceptor)
   │   Drena domain events del ChangeTracker, los serializa como OutboxMessage
   ▼
SQL Server (transacción única: entity + outbox)
   ▼
Controller mapea ErrorOr → IActionResult / ProblemDetails
```

### Flujo asíncrono (Outbox)

```
ProcessOutboxMessagesJob (BackgroundService, tick 10s)
   │
   ▼
SELECT TOP 20 OutboxMessages WHERE ProcessedOnUtc IS NULL ORDER BY OccurredOnUtc
   │
   ▼
Type.GetType("FishClubAlginet.Application.Features.Events.Commands.Fishermen." + Type)
   │
   ▼
JsonSerializer.Deserialize → IDomainEvent
   │
   ▼
IPublisher.Publish(domainEvent)  →  MediatR notification handlers
   │
   ▼
Mark ProcessedOnUtc = UtcNow
```

### Patrones aplicados (verificados en código)

* **CQRS** vía MediatR (Commands + Queries separados, un handler por feature).
* **Result pattern** vía `ErrorOr<T>` (no excepciones para flujos de negocio).
* **Generic Repository + Unit of Work** (Application define el contrato; Infrastructure lo implementa).
* **Domain Events + Outbox Pattern** (interceptor + background job).
* **Rich Domain Model** en `League` (factory + métodos `Activate/Archive/Update`).
* **Async/Await** generalizado, con `CancellationToken` en `IUnitOfWork.SaveChangesAsync`.
* **Dependency Injection** estándar de ASP.NET Core (Scoped para repos/UoW/services, HostedService para outbox).
* **Global Exception Handling** vía `GlobalExceptionHandler` + `ProblemDetails`.

### Anti-patrones / inconsistencias detectadas

* **Anemic Model en `Competition`** (todos setters públicos), frente a **Rich Model en `League`**. Inconsistencia de estilo.
* (Eliminado) `IFishermanRepository` ha sido eliminada por violar YAGNI.

---

## 3. Estado de la Implementación

### ✅ Módulos 100% funcionales (handler + controller + tests)

| Módulo | Operaciones |
|---|---|
| **Auth** | Login, Register, ChangePassword (+ tests del ChangePassword handler y validators de identidad) |
| **Users** | GetAll, Create, Block, Unblock, AssignRole, RemoveRole (+ tests de cada handler) |
| **Leagues** | Create, Update, Activate, Archive, GetAll, GetById, GetActive (+ tests completos) |
| **Fishermen** *(parcial → ver §3.2)* | Add, GetAll, GetByUserId, SoftDelete (+ tests) |

### 🟡 Componentes a medio terminar

1. **`Fisherman.Update()` y `Fisherman.Delete()`** — TODOs explícitos en `Fisherman.cs:53` y `Fisherman.cs:76`. El código está escrito y comentado, falta crear los `FishermanUpdatedDomainEvent` / `FishermanDeletedDomainEvent` y descomentar.

2. **`IFishermanRepository`** — ~interfaz declarada sin métodos~ **Resuelto: Eliminada por YAGNI.**

3. **Competitions** — handlers escritos (Create, OpenRegistration, CloseRegistration, RegisterFisherman, RemoveRegistration, UpdateResult, GetResults, GetByLeague) **pero sin tests**. Falta también revisar concurrencia en `RegisterFishermanCommandHandler` (riesgo de race si dos pescadores reservan el último spot — no verificado a fondo).

4. **Validación automática vía MediatR pipeline** — `FluentValidation` está registrado (`AddValidatorsFromAssembly`) pero **no veo un `ValidationBehavior<TRequest,TResponse>`** registrado. Esto significa que las validations existen como clases pero no se disparan automáticamente al enviar un Command; el handler tiene que invocarlas a mano. *Confianza media: solo he revisado `Program.cs`, no he hecho grep exhaustivo.*

### 🔴 Bugs / Race conditions detectados durante esta auditoría

> **Aclaración honesta:** estos bugs los he encontrado yo al revisar el código para generar este manifest. **No los habíamos discutido previamente.**

#### **BUG-1: Outbox solo captura eventos de `Fisherman` (CRÍTICO)**

* **Ubicación:** `FishClubAlginet.Infrastructure/Persistence/Interceptors/ConvertDomainEventsToOutboxMessagesInterceptor.cs:19`
* **Síntoma:** El interceptor escanea únicamente `dbContext.ChangeTracker.Entries<BaseEntity<int>>()`.
* **Causa:** Solo `Fisherman` hereda de `BaseEntity<int>`. `League`, `Competition`, `CompetitionResult` y `OutboxMessage` heredan de `BaseEntity<Guid>`. Por lo tanto, **cualquier `RaiseDomainEvent()` invocado en League/Competition se pierde silenciosamente** (se limpia del entity al guardar pero nunca llega al Outbox).
* **Impacto:** Cualquier consumidor de eventos de dominio en Leagues/Competitions (notificaciones, proyecciones, side effects) jamás se ejecutará.

#### **BUG-2: ProcessOutboxMessagesJob solo deserializa tipos del namespace Fishermen**

* **Ubicación:** `FishClubAlginet.API/Infrastructure/BackgroundJobs/ProcessOutboxMessagesJob.cs:53`
* **Síntoma:** Hardcoded `var typeName = $"FishClubAlginet.Application.Features.Events.Commands.Fishermen.{outboxMessage.Type}, FishClubAlginet.Application";`
* **Impacto:** Aunque arreglemos BUG-1, los eventos de otros bounded contexts seguirán fallando con "Type not found".

#### **OBSERVACIÓN-3: Migración con nombre confuso**

* Existen `InitialSqlServer` (2026-04-14) e `Initial` (2026-05-06). El segundo, con nombre "Initial", es post-inicial. Revisar si fue un rebuild manual de migraciones; si sí, conviene squashar antes de production.

---

## 4. Contexto de Infraestructura

### Máquina objetivo

* **Lenovo Yoga 7 Pro** — AMD Ryzen AI 9 (x86_64, no ARM → no aplica el escenario "Apple Silicon" que avisamos en el README).
* **WSL2** con distro Ubuntu (usuario `spawndevuser`, host `SpawnDev`).
* **Docker Engine Community 29.4.3** instalado **nativamente en la distro WSL** (NO Docker Desktop con WSL Integration). Cliente y daemon corren en la misma WSL.
* Docker Compose plugin v5.1.3 (`/usr/libexec/docker/cli-plugins/docker-compose`).

### Estado de la sesión (lo que ya está hecho)

* Usuario `spawndevuser` añadido al grupo `docker` (`sudo usermod -aG docker spawndevuser`). **Requiere re-login completo de la sesión WSL para activarse** (`wsl --shutdown` desde PowerShell + reabrir).
* `/var/run/docker.sock` permisos `srw-rw---- root docker`.
* `.env` real generado con `SA_PASSWORD=68|fRHw.8V4@` y `JWT_SECRET_KEY` de 64 chars. **Está en `.gitignore`.**

### Comandos de arranque (memorizar / copiar)

```bash
# 1. (solo una vez por sesión WSL, si fue necesario aplicar el grupo docker)
wsl --shutdown   # desde PowerShell en Windows; luego reabrir terminal WSL

# 2. Verificar que docker responde
docker info

# 3. Levantar stack de desarrollo
cd ~/fishclubalginet            # o C:\GIT\JosueBotella\FishClubAlginet desde Windows
docker compose up -d
docker compose logs -f api      # ver el seed/migraciones del primer arranque

# 4. (opcional) Portainer en :19100
docker compose -f docker-compose.tools.yml up -d

# 5. Apagar
docker compose down              # mantiene la BD
docker compose down -v           # tira también la BD
```

### Puertos expuestos (host → contenedor)

| Servicio | Puerto host | Puerto interno | URL |
|---|---|---|---|
| Frontend (Vite dev) | 5173 | 5173 | http://localhost:5173 |
| API (.NET) | 5000 | 8080 | http://localhost:5000 (Scalar: /scalar) |
| SQL Server | 1433 | 1433 | DBeaver: localhost,1433 — user `sa` |
| Portainer | 19100 | 9000 | http://localhost:19100 |

### Variables de entorno críticas (referencia para el `.env`)

```
SA_PASSWORD              # SQL Server SA, requisitos de complejidad MS
JWT_SECRET_KEY           # Mínimo 32 chars, ya cargado
JWT_ISSUER               # FishClubAlginetAPI
JWT_AUDIENCE             # FishClubAlginetUsers
JWT_DURATION_MINUTES     # 60
PUBLIC_HTTP_PORT         # 80 (solo prod compose)
PORTAINER_PORT           # 19100
```

---

## 5. Próximos Pasos — Micro-Commitments

> Tres tareas atómicas, ordenadas por relación valor/esfuerzo. Cada una < 1h, cada una commiteable sola.

### ✦ **TASK-A — Arreglar el interceptor de Outbox para que escanee también `BaseEntity<Guid>`** *(15-30 min)*

**Por qué primero:** desbloquea TODO el dominio (League, Competition) para emitir eventos. Sin esto, el Outbox es decorativo para 4 de 5 entidades.

**Archivo:** `FishClubAlginet.Infrastructure/Persistence/Interceptors/ConvertDomainEventsToOutboxMessagesInterceptor.cs`

**Cambio sugerido** (extraer una interfaz común no genérica):

```csharp
// En Core/Domain/Entities/BaseEntity.cs — añadir interfaz no genérica
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}

public abstract class BaseEntity<TId> : IHasDomainEvents { /* ... */ }
```

```csharp
// En el interceptor — cambiar línea 19
.Entries<IHasDomainEvents>()
```

**Definition of done:** test de integración que crea una `League`, llama a `Activate()` (raise event), persiste y verifica que existe un `OutboxMessage` con `Type = "LeagueActivatedDomainEvent"`.

---

### ✦ **TASK-B — Generalizar el resolver de tipos en `ProcessOutboxMessagesJob`** *(20-40 min)*

**Por qué después de A:** sin A no llegan los eventos; con A llegan pero el job los rechaza por namespace.

**Archivo:** `FishClubAlginet.API/Infrastructure/BackgroundJobs/ProcessOutboxMessagesJob.cs:53`

**Estrategia:** persistir el `AssemblyQualifiedName` del tipo en `OutboxMessage.Type` en lugar del simple `Name`, o construir un diccionario `string → Type` al startup escaneando el ensamblado de Application con `typeof(IDomainEvent).IsAssignableFrom(t)`.

**Definition of done:** sigue funcionando para `FishermanAddedDomainEvent`, y además funciona para cualquier nuevo event sin tocar el job.

---

### ✦ **TASK-C — Descomentar `Fisherman.Update()` y `Fisherman.Delete()` con domain events** *(30-45 min)*

**Por qué tercero:** queda cerrado el patrón de eventos de Fisherman y aumenta cobertura funcional sin desbloquear nada del Outbox (ya estaba operativo para Fisherman).

**Archivos:**

1. `FishClubAlginet.Core/Domain/Entities/Fisherman.cs:53` — descomentar `Update()`.
2. `FishClubAlginet.Core/Domain/Entities/Fisherman.cs:76` — descomentar `Delete()`.
3. Crear `FishClubAlginet.Application/Features/Events/Commands/Fishermen/FishermanUpdatedDomainEvent.cs` y `FishermanDeletedDomainEvent.cs` con el mismo patrón que `FishermanAddedDomainEvent`.
4. (opcional pero recomendable) Crear handler stub `FishermanUpdatedDomainEventHandler` y `FishermanDeletedDomainEventHandler`, aunque solo logueen, para que el job no escupa "Type not found".

**Definition of done:** test de handler de `Fisherman` que llama a `Update()` y verifica que el evento se ha encolado en domain events.

---

### Tareas más grandes para roadmap (no ahora)

* Añadir `ValidationBehavior<,>` al pipeline de MediatR para que los validators corran solos.
* Tests de `Competitions`. Especial cuidado con concurrencia en `RegisterFishermanCommandHandler` (último spot disponible).
* ~Implementar `IFishermanRepository` o eliminar la interfaz fantasma~ **(Resuelto)**.
* Squashar migraciones antes del primer deploy real.

---

## 6. Fragmentos de Código Clave

### `IGenericRepository<T, TId>` *(Application/Abstractions)*

```csharp
public interface IGenericRepository<T, TId>
    where T : BaseEntity<TId>
{
    Task<T> AddAsync(T entity);
    Task<T?> GetById(TId id);
    IQueryable<T> GetAll();
    void Update(T entity);
    Task<bool> SoftDelete(TId id);
    Task<bool> HardDelete(TId id);
}
```

### `IUnitOfWork` *(Application/Abstractions)*

```csharp
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T, TId> Repository<T, TId>() where T : BaseEntity<TId>;
    Task<ErrorOr<int>> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### `BaseEntity<TId>` *(Core/Domain/Entities)*

```csharp
public abstract class BaseEntity<TId>
{
    public required TId Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedTimeUtc { get; set; }
    public DateTime LastUpdateUtc { get; set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();
    public void ClearDomainEvents() => _domainEvents.Clear();
    public void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
```

### `IAuthService` / `IUserManagementService` *(Application/Abstractions)*

```csharp
public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterUserDto registerDto);
    Task<string?> LoginAsync(LoginDto loginDto);
    Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}

public interface IUserManagementService
{
    Task<IList<UserDto>> GetAllUsersAsync();
    Task<PaginatedResult<UserDto>> GetUsersPagedAsync(int skip, int take, string? search);
    Task<IdentityResult> BlockUserAsync(string userId);
    Task<IdentityResult> UnblockUserAsync(string userId);
    Task<IdentityResult> AssignRoleAsync(string userId, string role);
    Task<IdentityResult> RemoveRoleAsync(string userId, string role);
    Task<ErrorOr<string>> CreateUserWithRoleAsync(string email, string password, string role);
}
```

### `AppDbContext` *(Infrastructure/Persistence/Contexts)*

```csharp
public class AppDbContext : IdentityDbContext
{
    public DbSet<Fisherman> Fishermen { get; set; }              // BaseEntity<int>
    public DbSet<League> Leagues { get; set; }                   // BaseEntity<Guid>
    public DbSet<Competition> Competitions { get; set; }         // BaseEntity<Guid>
    public DbSet<CompetitionResult> CompetitionResults { get; set; }  // BaseEntity<Guid>
    public DbSet<OutboxMessage> OutboxMessages { get; set; }     // BaseEntity<Guid>

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new ConvertDomainEventsToOutboxMessagesInterceptor());
        base.OnConfiguring(optionsBuilder);
    }
}
```

### Esquema DB (resumen, derivado de migraciones)

```
AspNetUsers, AspNetRoles, AspNetUserRoles, ...   (Identity tables, schema dbo)

Fishermen
  Id INT PK identity
  FirstName, LastName, DocumentType, DocumentNumber,
  FederationLicense?, FederationNumber?, RegionalLicense?,
  Address (owned: Street, City, PostalCode, Province),
  UserId? (FK lógica → AspNetUsers)
  IsDeleted, DeletedTimeUtc, LastUpdateUtc

Leagues
  Id UNIQUEIDENTIFIER PK
  Name (max 100), Year (>=2000), IsActive, IsArchived,
  MinPoints (default 5), WorstResultsToDiscard (default 0)
  IsDeleted, ..., LastUpdateUtc

Competitions
  Id UNIQUEIDENTIFIER PK
  LeagueId FK → Leagues (cascade)
  CompetitionNumber INT  → UNIQUE (LeagueId, CompetitionNumber)
  Name, Date, StartTime, EndTime, Venue, Zone
  Subspecialty nvarchar(20), Category nvarchar(20)
  Status nvarchar(30) DEFAULT 'Planned'
  MaxSpots, ParticipantCount

CompetitionResults
  Id UNIQUEIDENTIFIER PK
  CompetitionId FK → Competitions (cascade)
  FishermanId FK → Fishermen (restrict)
  AssignedSpotNumber INT?  → UNIQUE (CompetitionId, AssignedSpotNumber) WHERE NOT NULL
  → UNIQUE (CompetitionId, FishermanId)
  RegistrationDate, IsValidated, DidAttend
  WeightInGrams, BiggestCatchWeight?, Points DECIMAL(18,2), Ranking

OutboxMessages
  Id UNIQUEIDENTIFIER PK
  OccurredOnUtc, ProcessedOnUtc?, Error?
  Type NVARCHAR  (nombre simple del tipo, NO assembly-qualified — ver BUG-2)
  Content NVARCHAR(MAX)  (JSON serializado del IDomainEvent)
```

### Entidad `League` — referencia de Rich Domain Model bien hecho

```csharp
public class League : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public bool IsActive { get; set; }
    public bool IsArchived { get; set; }
    public int MinPoints { get; set; } = 5;
    public int WorstResultsToDiscard { get; set; } = 0;
    public ICollection<Competition> Competitions { get; set; } = new List<Competition>();

    public static League Create(string name, int year, int minPoints = 5, int worstResultsToDiscard = 0) { /* ... */ }
    public void Update(string name, int minPoints, int worstResultsToDiscard) { /* ... */ }
    public void Activate() { /* ... */ }
    public void Deactivate() { /* ... */ }
    public void Archive() { /* ... */ }
}
```

---

## Anexo — Decisiones de Sesión (Dockerización completa)

* `docker-compose.yml` (dev) + `docker-compose.prod.yml` (Nginx + builds optimizados) + `docker-compose.tools.yml` (Portainer).
* `Program.cs` modificado: `UseHttpsRedirection` se omite cuando `DOTNET_RUNNING_IN_CONTAINER=true`.
* `vite.config.ts`: proxy target leído desde `VITE_PROXY_TARGET` env.
* `appsettings.json` y `appsettings.Development.json`: connection string y JWT secret **vaciados** (vienen por env vars).
* `.gitignore`: añadidos `.env`, `.env.local`, `.env.prod`. `!.env.example` para no excluir la plantilla.
* JWT secret previo (`QfCVryLEt3oYp1d…`) ya rotado al nuevo del `.env`. **El antiguo sigue en el historial de Git si fue commiteado; considera force-push + rotación adicional si llegó a remoto.**

---

*Fin del manifest. Generado a partir de inspección directa del repo el 2026-05-14.*
