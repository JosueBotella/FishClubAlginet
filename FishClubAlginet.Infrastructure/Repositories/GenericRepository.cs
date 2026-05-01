

namespace FishClubAlginet.Infrastructure.Repositories;

/// <summary>
/// Repositorio genérico. Solo "stagea" los cambios en el DbContext (Add/Update/Remove).
/// La persistencia real (SaveChangesAsync) se delega al IUnitOfWork desde el handler,
/// para mantener un patrón Unit of Work consistente y permitir transacciones por caso de uso.
/// </summary>
public class GenericRepository<T, TId> : IGenericRepository<T, TId>
    where T : BaseEntity<TId>
{
    private readonly AppDbContext _context;

    public GenericRepository(AppDbContext context) => _context = context;


    public async Task<T> AddAsync(T entity)
    {
        await _context.AddAsync(entity);
        return entity;
    }

    public virtual async Task<T?> GetById(TId id)
        => await _context.Set<T>()
            .FirstAsync(a => a.Id.Equals(id));


    public IQueryable<T> GetAll()
        => _context.Set<T>();

    public void Update(T entity)
    {
        entity.LastUpdateUtc = DateTime.UtcNow;
        _context.Set<T>().Update(entity);
    }

    public async Task<bool> SoftDelete(TId id)
    {
        T? entity = await GetById(id);

        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedTimeUtc = DateTime.UtcNow;

        _context.Set<T>().Update(entity);
        return true;
    }

    public async Task<bool> HardDelete(TId id)
    {
        T? entity = await GetById(id);
        if (entity is null)
        {
            return false;
        }

        _context.Set<T>().Remove(entity);
        return true;
    }
}
