using ErrorOr;
using Microsoft.Data.SqlClient;

namespace FishClubAlginet.Infrastructure.Services;

/// <summary>
/// Implementación mínima del Unit of Work sobre el AppDbContext scoped.
/// - No crea repositorios: el contenedor de DI ya inyecta IGenericRepository&lt;,&gt;
///   compartiendo el mismo DbContext de la petición.
/// - No hace Dispose del DbContext: su ciclo de vida pertenece al contenedor de DI.
/// </summary>
public sealed class UnitOfWorkService(AppDbContext context) : IUnitOfWork
{
    public async Task<ErrorOr<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx
                  && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
        {
            // 2627 = Violation of UNIQUE KEY constraint
            // 2601 = Cannot insert duplicate key row in object with unique index
            return Error.Conflict(
                code: "Database.UniqueConstraintViolation",
                description: "A record with these unique values already exists.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict(
                code: "Database.Concurrency",
                description: "The record was modified by another process. Reload and try again.");
        }
        catch (DbUpdateException)
        {
            return Error.Failure(
                code: "Database.SaveFailure",
                description: "Failed to save the record. Please try again.");
        }
        // Otras excepciones (errores de programación, fallos de red, etc.) se
        // propagan: las captura el ExceptionHandler global de la API.
    }
}
