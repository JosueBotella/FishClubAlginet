using ErrorOr;
using Microsoft.Data.SqlClient;

namespace FishClubAlginet.Infrastructure.Services;

public class UnitOfWorkService : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDictionary<Type, dynamic> _repositories = new Dictionary<Type, dynamic>();
    private bool _disposed = false;

    public UnitOfWorkService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();

                foreach (var repository in _repositories.Values)
                {
                    if (repository is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                _repositories.Clear();
            }

            _disposed = true;
        }
    }

    public IGenericRepository<T, TId> Repository<T, TId>()
      where T : BaseEntity<TId>
    {
        var entityType = typeof(T);

        if (_repositories.ContainsKey(entityType))
        {
            return _repositories[entityType];
        }

        var repositoryType = typeof(GenericRepository<,>);
        var closedGenericType = repositoryType.MakeGenericType(typeof(T), typeof(TId));

        var repository = Activator.CreateInstance(closedGenericType, _context);

        _repositories.Add(entityType, repository);
        return (IGenericRepository<T, TId>)repository;
    }
}
