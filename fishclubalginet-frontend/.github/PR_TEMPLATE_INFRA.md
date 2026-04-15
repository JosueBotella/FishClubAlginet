## feat: Migración infraestructura backend — PostgreSQL → SQL Server Express

### Descripcion

Migración del acceso a datos de PostgreSQL a SQL Server Express 2025 como parte de la rama `react_migration`. No hay cambios funcionales: la API expone los mismos endpoints con el mismo comportamiento.

### Cambios realizados

**Infrastructure (acceso a datos)**
- Sustituido `Npgsql.EntityFrameworkCore.PostgreSQL` por `Microsoft.EntityFrameworkCore.SqlServer` en `FishClubAlginet.Infrastructure.csproj`
- Cambiado `UseNpgsql()` → `UseSqlServer()` en `Program.cs`
- Eliminado `global using Npgsql` de `GlobalUsing.cs`
- Regeneradas migraciones EF Core para SQL Server

**GenericRepository**
- Cambiada deteccion de duplicados: `PostgresException (SqlState 23505)` → `SqlException (Number 2627/2601)`

**Connection strings**
- `appsettings.json` y `appsettings.Development.json` actualizados a `Server=localhost\SQLEXPRESS;Database=FishClubAlginetDb;Trusted_Connection=True;TrustServerCertificate=True`

**Fixes adicionales**
- Corregido trailing slash en CORS origin (`http://localhost:5173/` → `http://localhost:5173`) en `ApplicationConstants.cs`
- Corregido `SecretKey` placeholder `"SEE_USER_SECRETS"` en `appsettings.Development.json` (causaba error HS256 key size)
- Reordenado middleware: `UseCors()` antes de `UseHttpsRedirection()` en `Program.cs`

### Tests

Todos los tests unitarios existentes pasan sin cambios. Son tests de capa Application con mocks (no tocan el provider de DB).

### Checklist

- [x] Compilacion sin errores
- [x] Tests unitarios pasan
- [x] API arranca y conecta a SQL Server Express
- [x] Login funciona correctamente
- [x] Seeds se ejecutan (roles + usuario admin)
- [x] Sin referencias residuales a Npgsql/PostgreSQL
