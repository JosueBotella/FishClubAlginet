using ErrorOr;

namespace FishClubAlginet.Application.Abstractions;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T, TId> Repository<T, TId>() where T : BaseEntity<TId>;

    /// <summary>
    /// Persiste todos los cambios pendientes del DbContext en una transacción.
    /// Devuelve el número de filas afectadas, o un Error mapeado a partir de las
    /// excepciones de EF Core (sin filtrar tipos de Infrastructure al caller),
    /// para mantener Clean Architecture: Application no conoce DbUpdateException.
    /// </summary>
    Task<ErrorOr<int>> SaveChangesAsync(CancellationToken cancellationToken = default);
}
