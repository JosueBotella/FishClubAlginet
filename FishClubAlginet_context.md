# FishClubAlginet — Contexto del Proyecto

## Objetivo
Plataforma de gestión para club de pesca local (Alginet). Gestiona socios (Fishermen), Ligas anuales y Competiciones.

## Stack Técnico
- **Backend:** .NET 9, Clean Architecture
- **Frontend:** React + TypeScript (migración desde Blazor WebAssembly)
- **UI:** TBD — sustituye Radzen Blazor
- **ORM:** Entity Framework Core + SQL Server
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
- Cada Competición tiene un número fijo de puestos (Fishing Spots)
- Las inscripciones requieren validación manual por Admin

## Estándares i18n
- Backend sin strings en español hardcodeados
- Errores en `Errors.cs` con códigos únicos (ej: `"Auth.InvalidCredentials"`)
- Mensajes de usuario en `.resx` o manejados por el frontend via Error Codes
- Logs técnicos en inglés

---

## Modelo de Dominio

### Entidades existentes
- **Fisherman:** Socio del club. Campos: FirstName, LastName, DateOfBirth, DocumentType, DocumentNumber, FederationLicense, Address (Value Object), UserId, IsDeleted.

### Entidades a implementar (Phase 2-3)
- **League:** Id (Guid), Name, Year (int), IsActive. → 1:N con Competitions
- **Competition:** Id (Guid), Name, Date, StartTime, EndTime, Location, MaxSpots. → Pertenece a League, 1:N con CompetitionRegistrations
- **CompetitionRegistration:** Id (Guid), FishermanId, CompetitionId, RegistrationDate, IsValidated (default: false), AssignedSpotNumber (nullable). Asignación de puesto solo tras validación.
- **FishingSpot:** Id (Guid), SpotNumber (int), IsOccupied (bool). Derivado de Competition.MaxSpots.

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
  - [] Crear nueva rama `react-migration` desde `master`
**Auth**
- [ ] Login (muro de login)
- [ ] Logout
- [ ] Gestión de JWT (almacenamiento, refresh, interceptor HTTP)
- [ ] Rutas protegidas por rol (Admin / Fisherman)

**Layout y navegación**
- [ ] Layout con sidebar diferenciado por rol
- [ ] Mostrar nombre del usuario logado junto al logout
- [ ] Enlace Home y enlace Perfil en navegación

**Admin — Users**
- [ ] Grid de usuarios (email, roles, estado bloqueo)
- [ ] Crear usuario Admin/Fisherman (modal/dialog)
- [ ] Bloquear/Desbloquear usuario
- [ ] Deshabilitar acciones sobre el propio usuario logado
- [ ] Search/filter en grid
- [ ] Paginación

**Admin — Fishermen**
- [ ] Grid de pescadores
- [ ] Soft Delete (botón → IsDeleted=true, confirmación, notificación)
- [ ] Filtrar eliminados del grid
- [ ] Vista histórico de pescadores eliminados
- [ ] Search/filter en grid
- [ ] Paginación

**Perfil de usuario**
- [ ] Vista readonly con datos del Fisherman
- [ ] Cambio de contraseña (validar contraseña actual)
- [ ] Notificaciones de éxito/error

**Gestión de roles**
- [ ] Asignar/quitar roles a usuarios

### 🔲 Pendiente — Phase 2: League Management
- Entidad `League` + handlers MediatR (backend)
- Migración EF Core
- UI React: League Management Dashboard

### 🔲 Pendiente — Phase 3: Competitions & Registration
- Entidades `Competition` + `CompetitionRegistration` (backend)
- Migración EF Core
- UI React: Calendario de competiciones + inscripción para Fishermen

---

## URLs de desarrollo
- **API:** https://localhost:7179
- **React:** TBD (Vite dev server, puerto por definir)
- **Repo:** https://github.com/JosueBotella/FishClubAlginet

## Ejecutar proyecto
```bash
dotnet ef database update --project FishClubAlginet.Infrastructure --startup-project FishClubAlginet.API
# Luego arrancar API + React dev server simultáneamente
```
