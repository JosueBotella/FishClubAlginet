# HANDOFF — FishClub Alginet

> Documento de contexto para continuar el desarrollo en otra herramienta (Warp, OpenCode, etc.).
> Estado a fecha: **mayo 2026**.
>
> **Lectura recomendada antes de tocar código** (en este orden):
> 1. Este `HANDOFF.md` (resumen ejecutivo)
> 2. `FishClubAlginet_context.md` (especificación funcional completa con roadmap)
> 3. `README.md` (instrucciones de arranque)

---

## 1. Qué es el proyecto

Plataforma de gestión para el **Club de Pesca V42 (Alginet, Valencia)**. Gestiona pescadores socios, ligas anuales y concursos federados FPCV (Federación de Pesca de la Comunidad Valenciana).

**Estado funcional actual**: backend completo de Identity + gestión de Fishermen + Users + Roles. Frontend React con login, gestión de usuarios, gestión de pescadores con soft delete, perfil de usuario y cambio de contraseña. Todo dockerizado.

**Pendiente** (Phase 2 en adelante): ligas, concursos, resultados, clasificaciones, acta FPCV. Detalle completo en `FishClubAlginet_context.md` secciones Phase 2-6.

---

## 2. Stack técnico

| Capa | Tecnología |
|---|---|
| Backend | ASP.NET Core **.NET 10**, Clean Architecture (Core / Application / Infrastructure / API / Contracts) |
| ORM | Entity Framework Core + SQL Server 2022 |
| CQRS | MediatR (commands, queries, pipeline) |
| Validación | FluentValidation (vía pipeline MediatR) |
| Result pattern | `ErrorOr<T>` (NuGet `ErrorOr`) |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Outbox | Custom interceptor de SaveChanges + `ProcessOutboxMessagesJob` (HostedService) |
| Tests backend | xUnit + Moq + FluentAssertions |
| Frontend | React 19 + TypeScript + Vite 6 |
| UI | Mantine v7 + @tabler/icons-react |
| HTTP | Axios con interceptor JWT |
| Routing | React Router v7 |
| Tests frontend | Vitest 2.1 + React Testing Library + jsdom |
| Containers | Docker Compose (dev + prod) |

---

## 3. Arquitectura del backend (Clean Architecture)

```
FishClubAlginet.Core            → Entidades, Value Objects, Interfaces, Domain Events
FishClubAlginet.Application     → Commands/Queries (CQRS), Handlers, Validators, DTOs
FishClubAlginet.Contracts       → DTOs Request/Response compartidos, Enums (TypeNationalIdentifier)
FishClubAlginet.Infrastructure  → DbContext, Repositories, EF Configs, Interceptors, Seeds
FishClubAlginet.API             → Controllers, BackgroundJobs, Program.cs
FishClubAlginet.Tests           → Unit tests
fishclubalginet-frontend/       → SPA React (proyecto npm separado)
```

**Reglas de dependencia (críticas)**:
- `Core` no depende de nadie
- `Application` depende solo de `Core` (y NuGets como ErrorOr, MediatR, FluentValidation)
- `Application` **NO conoce EF Core ni `DbUpdateException`** — el mapeo de excepciones EF a `ErrorOr` está centralizado en `Infrastructure/Services/UnitOfWorkService.cs`
- `Infrastructure` depende de `Core` + `Application`
- `API` orquesta todo

---

## 4. Patrones y convenciones del código

### 4.1. Unit of Work consistente (refactor reciente — IMPORTANTE)

Todo handler que mute estado:

```csharp
public class FooCommandHandler : IRequestHandler<FooCommand, ErrorOr<int>>
{
    private readonly IGenericRepository<Entity, int> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<ErrorOr<int>> Handle(FooCommand cmd, CancellationToken ct)
    {
        await _repo.AddAsync(entity);          // sólo "stagea" en ChangeTracker

        var save = await _unitOfWork.SaveChangesAsync(ct);
        if (save.IsError)
        {
            // _unitOfWork ya devuelve Error.Conflict / Error.Failure según excepción EF
            if (save.FirstError.Code == "Database.UniqueConstraintViolation")
                return Error.Conflict($"{nameof(Entity)}.Duplicate", "...");
            return Error.Failure("ENTITY_SAVE_FAILED", "...");
        }
        return entity.Id;
    }
}
```

**`IGenericRepository.AddAsync`** devuelve `Task<T>`, NO `Task<ErrorOr<T>>` (el repo solo stagea, no persiste, no decide errores). Errores reales suben vía el UoW.

### 4.2. ErrorOr en handlers

```csharp
public record FooCommand(int Id) : IRequest<ErrorOr<bool>>;

public async Task<ErrorOr<bool>> Handle(FooCommand req, CancellationToken ct)
{
    if (!valid) return Error.Validation("Foo.Invalid", "...");
    if (!found) return Error.NotFound("Foo.NotFound", "...");
    return true;
}
```

En el controller: `result.Match(ok => Ok(ok), errors => Problem(errors))`.

### 4.3. Tests backend (xUnit + Moq, AAA)

```csharp
[Fact]
public async Task Handle_WhenValid_ShouldPersistAndReturnOk()
{
    // Arrange
    var repo = new Mock<IGenericRepository<X, int>>();
    var uow = new Mock<IUnitOfWork>();
    uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
       .ReturnsAsync((ErrorOr<int>)1);
    var handler = new FooCommandHandler(repo.Object, uow.Object, NullLogger);

    // Act
    var result = await handler.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsError);
    uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

**Patrón clave**: verificar **explícitamente** que `SaveChangesAsync` se llama (cuando debe) y que NO se llama (cuando no debe — p.ej. en NotFound).

### 4.4. JWT y autorización

`Program.cs` configura el handler con dos detalles críticos:

```csharp
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;   // ⚠️ NO TOCAR — sin esto, RoleClaimType="role" no casa
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RoleClaimType = "role",
        NameClaimType = JwtRegisteredClaimNames.Email,
        // ...
    };
});
```

**Si añades nuevos claims**: emítelos en `AuthService.GenerateJwtTokenAsync` con nombres cortos (`"role"`, `"sub"`, etc.) y conserva `MapInboundClaims = false`.

### 4.5. Outbox pattern (Domain Events)

Para entidades con eventos de dominio:

```csharp
fisherman.RaiseDomainEvent(new FishermanAddedDomainEvent { ... });
await _repo.AddAsync(fisherman);
await _unitOfWork.SaveChangesAsync(ct);  // El interceptor convierte el evento en OutboxMessage
                                          // dentro de la MISMA transacción ACID
```

`ProcessOutboxMessagesJob` (HostedService) procesa los outbox messages periódicamente.

### 4.6. Frontend — patrones consistentes

- Páginas en `src/pages/<Feature>/<Page>.tsx`
- API clients en `src/api/<feature>Api.ts`, todas pasan por `apiClient` (axios singleton con interceptor JWT)
- Endpoints en `src/constants/endpoints.ts` como rutas relativas (`api/account/login`), el proxy de Vite/Nginx las redirige al backend
- Forms con `@mantine/form`, notificaciones con `@mantine/notifications`
- Tests en `__tests__/<Component>.test.tsx`, render con helper `src/test/renderWithProviders.tsx` que envuelve en `MantineProvider`

---

## 5. Cómo arrancar localmente (camino Docker, recomendado)

```bash
git clone https://github.com/JosueBotella/FishClubAlginet.git
cd FishClubAlginet

# Variables de entorno (passwords y JWT secret)
cp .env.example .env
# Editar .env y poner SA_PASSWORD (8+ chars, may+min+dig+símbolo) y JWT_SECRET_KEY (32+ chars)

# Levantar todo
docker compose up --build
```

URLs:
- Frontend: http://localhost:5173
- API: http://localhost:5000 (Scalar UI en `/scalar`)
- SQL Server: `localhost:1433` (sa + password de `.env`)

Comandos útiles:

```bash
docker compose down -v           # parar y borrar la DB
docker compose logs -f api       # logs en vivo del backend
docker compose up -d --build api # rebuild solo del API
```

Producción:

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

### Camino sin Docker (más antiguo, sigue funcionando)

Requiere SDK .NET 10, SQL Server Express local, Node 20. Ver `README.md` sección "Primeros Pasos (instalación manual)".

---

## 6. Decisiones recientes destacables (esta sesión)

### Arquitectura
- **Refactor a Unit of Work consistente**: el repositorio nunca persiste, siempre lo hace el handler vía `IUnitOfWork.SaveChangesAsync()`. El UoW (en Infrastructure) traduce excepciones EF a `ErrorOr<int>` con códigos genéricos (`Database.UniqueConstraintViolation`, `Database.Concurrency`, `Database.SaveFailure`). Los handlers (en Application) NO conocen EF.
- **Bug de autorización JWT** resuelto con `MapInboundClaims = false` (sin esto, `[Authorize(Roles="Admin")]` daba 403 aunque el JWT llevara los roles correctos).
- **Bug de soft delete** resuelto: la persistencia real del cambio (antes se quedaba en el ChangeTracker y se perdía).

### Funcionalidad
- **Perfil de usuario** (`/profile`): vista de cuenta + datos del Fisherman si existen + cambio de contraseña.
- **Gestión de roles** (modal `EditUserModal` desde `AdminUsersPage`): checkboxes con cálculo de diff e invocación selectiva a `assignRole`/`removeRole`.

### Infraestructura
- Dockerización completa (dev y prod compose, healthchecks, multi-stage build, Nginx en prod).
- `Routes.ChangePassword` sin uso eliminado.

### Testing
- Tests backend: `RemoveRoleCommandHandlerTests` añadido (simétrico a Assign).
- Tests frontend: Vitest + RTL + jsdom configurado. 16 tests cubriendo `EditUserModal` (diff de roles), `getMyProfile` (manejo de 404 → null) y `ProfilePage` (carga + cambio password).

---

## 7. Tests actuales

### Backend (`FishClubAlginet.Tests/`)
- `AssignRoleCommandHandlerTests`
- `BlockUserCommandHandlerTests`
- `ChangePasswordCommandHandlerTests`
- `FisherManAddCommanHandlerTests` (refactorizado al patrón UoW)
- `FisherManGetAllQueriesHandlerTests`
- `GetAllUsersQueryHandlerTests`
- `GetFishermanByUserIdQueryHandlerTests`
- `RemoveRoleCommandHandlerTests`
- `SoftDeleteFishermanCommandHandlerTests` (refactorizado al patrón UoW)
- `UnblockUserCommandHandlerTests`

Ejecutar: `dotnet test FishClubAlginet.Tests/`

### Frontend (`fishclubalginet-frontend/src/`)
- `api/__tests__/fishermenApi.test.ts` (4 tests)
- `pages/AdminUsers/__tests__/EditUserModal.test.tsx` (7 tests)
- `pages/Profile/__tests__/ProfilePage.test.tsx` (5 tests)

Ejecutar: `cd fishclubalginet-frontend && npm test`

### Sin test específico (deuda técnica conocida)
- `UnitOfWorkService.SaveChangesAsync`: el mapeo de excepciones EF a `Error` no se testea unitariamente porque `SqlException` no tiene constructor público. Cubierto indirectamente vía mocks en handlers. Si se necesita test directo: usar TestContainers con SQL Server real.

---

## 8. Roadmap pendiente (resumen — detalle en `FishClubAlginet_context.md`)

| Fase | Contenido | Estado |
|---|---|---|
| **Phase 2** | Gestión de Ligas (entidad `League`, regla 1 activa global, ampliación de `Fisherman` con `FederationNumber`, UI Admin) | 🔲 |
| **Phase 3** | Concursos y Resultados (entidades `Competition`, `CompetitionResult`, máquina de estados, algoritmo `PointsCalculator` documentado) | 🔲 |
| **Phase 4** | Clasificaciones (servicios `WeightStandingCalculator`, `PointsStandingCalculator`, Pieza Mayor, snapshots opcionales) | 🔲 |
| **Phase 5** | Acta Oficial FPCV (Word + PDF, plantilla con texto legal, especies capturadas, firmas) | 🔲 |
| **Phase 6** | Estadísticas y Reporting (opcional, dashboard con gráficos) | 🔲 |

Decisiones de negocio confirmadas:
- **Una sola liga activa** simultáneamente en el club (no por subespecialidad)
- En el Acta, el campo `< 14 años / > 14 años` se **calcula** desde `Fisherman.DateOfBirth` vs `Competition.Date`
- `Venue` y `Zone` son **strings libres** (con autocompletado opcional en frontend)

---

## 9. Cuestiones abiertas

⚠️ **Sistema "RESTA" en clasificación de liga por puntos**: el Excel `LIGA RESTA 2025.xls` contiene una columna "RESTA" cuyo significado preciso no es deducible de los datos (solo Juan Alcaraz tiene 2,5 aplicado, sin patrón evidente). El cliente confirmará la fórmula más adelante. **No implementar la columna hasta tener la regla**. Detalle en Phase 4 del `context.md`.

---

## 10. Glosario de dominio

| Término | Significado |
|---|---|
| **Fisherman** | Pescador socio del club. Tiene ficha con datos personales, dirección, licencias |
| **FederationLicense** | Licencia federativa textual (puede ser larga) |
| **FederationNumber** | Identificador federativo único corto (formato `V-552`). **Pendiente de añadir como campo en Phase 2** |
| **Liga** (League) | Temporada anual (1 enero – 31 diciembre) que agrupa concursos |
| **Concurso** (Competition) | Jornada de pesca dentro de una liga, en un escenario y zona concretos |
| **CompetitionResult** | Inscripción + resultado de un pescador en un concurso (combina ambos) |
| **Pesquera** (`AssignedSpotNumber`) | Puesto físico asignado por sorteo el día del concurso |
| **Pieza Mayor (PM)** | Captura más grande, se premia por concurso y por temporada |
| **Acta** | Documento oficial firmado por el jurado, con formato federativo FPCV |
| **FPCV** | Federación de Pesca de la Comunidad Valenciana |
| **V42** | Código del club dentro de FPCV |
| **Subespecialidad** | `Mar` o `AguaDulce` |
| **Categoría** | `Seniors` o `Juvenil` |
| **Sistema de puntos** | Ranking inverso por peso, mínimo 5 pts, empates comparten media |
| **Sistema RESTA** | Variante de la clasificación por puntos con descarte (regla pendiente del cliente) |

---

## 11. Archivos clave para arrancar a navegar el código

### Backend
- `FishClubAlginet.API/Program.cs` — wiring DI, JWT, CORS, middlewares
- `FishClubAlginet.API/Controllers/` — todos los controllers REST
- `FishClubAlginet.Application/Features/` — handlers MediatR organizados por feature
- `FishClubAlginet.Core/Domain/Entities/` — entidades de dominio
- `FishClubAlginet.Core/Abstractions/` — interfaces (`IGenericRepository`, `IUnitOfWork`, `IAuthService`)
- `FishClubAlginet.Infrastructure/Services/UnitOfWorkService.cs` — punto único de mapeo excepciones EF → `Error`
- `FishClubAlginet.Infrastructure/Repositories/GenericRepository.cs` — repo genérico (solo stagea, no persiste)
- `FishClubAlginet.Infrastructure/Persistence/Seeds/` — datos iniciales

### Frontend
- `fishclubalginet-frontend/src/App.tsx` — routing principal
- `fishclubalginet-frontend/src/auth/` — `AuthContext`, `ProtectedRoute`
- `fishclubalginet-frontend/src/api/apiClient.ts` — singleton axios con interceptor JWT
- `fishclubalginet-frontend/src/api/*Api.ts` — clientes API por feature
- `fishclubalginet-frontend/src/pages/` — páginas
- `fishclubalginet-frontend/src/layout/AppLayout.tsx` — sidebar + header con roles
- `fishclubalginet-frontend/src/test/setup.ts` y `renderWithProviders.tsx` — infraestructura de tests

### Docker / configuración
- `docker-compose.yml` y `docker-compose.prod.yml` — orquestación
- `FishClubAlginet.API/Dockerfile` — multi-stage build .NET 10
- `fishclubalginet-frontend/Dockerfile` y `Dockerfile.dev` — build prod (Nginx) y dev (Vite HMR)
- `.env.example` — plantilla de variables sensibles

### Documentación
- `FishClubAlginet_context.md` — **fuente de verdad funcional**, roadmap completo
- `README.md` — instrucciones de arranque
- Carpeta de referencia con Excels reales del club: `Concurso 18/` (en `C:\Users\spawndevuser\Downloads\Concurso 18` localmente, no versionada en repo)

---

## 12. Comandos rápidos cheatsheet

```bash
# Arrancar todo
docker compose up --build

# Tests backend
dotnet test FishClubAlginet.Tests/

# Tests frontend
cd fishclubalginet-frontend && npm test

# Aplicar migraciones (sin Docker)
dotnet ef database update --project FishClubAlginet.Infrastructure --startup-project FishClubAlginet.API

# Crear nueva migración
dotnet ef migrations add NombreMigracion --project FishClubAlginet.Infrastructure --startup-project FishClubAlginet.API

# Resetear DB en Docker
docker compose down -v && docker compose up

# Inspeccionar DB
docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -d FishClubAlginetDb
```

---

## 13. Para la próxima IA / sesión

**Si te pide arrancar Phase 2 (Ligas)**:
1. Lee `FishClubAlginet_context.md` sección "Phase 2: Gestión de Ligas"
2. Crea entidad `League` en `FishClubAlginet.Core/Domain/Entities/`
3. Configuración EF en `FishClubAlginet.Infrastructure/Persistence/Configurations/`
4. Handlers en `FishClubAlginet.Application/Features/Leagues/Commands` y `.../Queries`
5. Sigue el patrón UoW (ver `SoftDeleteFishermanCommandHandler` como referencia limpia)
6. Tests siguiendo el patrón de `SoftDeleteFishermanCommandHandlerTests`
7. Migración EF: `dotnet ef migrations add AddLeagues ...`
8. Frontend: nueva página en `pages/AdminLeagues/`, modal de creación/edición

**Si te pide arrancar Phase 3 (Concursos)**:
- Algoritmo `PointsCalculator` está documentado paso a paso en el `context.md`. Usa el caso del 18º concurso (27 participantes, 25 pts al primero, empates de pos 14-15 a 11,5 pts, etc.) como suite de tests del cálculo
- Para entender los datos reales: revisar `18º - CONCURSO.xls` y `LIGA POR PESO 2025.xls` en la carpeta `Concurso 18/`

**No olvidar**:
- Tests obligatorios para todo handler nuevo (frontend y backend)
- Mantener `MapInboundClaims = false` en `Program.cs`
- Repo solo stagea, UoW persiste
- Application no conoce EF
- Errores con códigos consistentes (`Entidad.Caso`, ej: `League.Duplicate`, `Competition.NotFound`)
