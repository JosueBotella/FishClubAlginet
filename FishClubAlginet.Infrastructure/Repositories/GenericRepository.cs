using ErrorOr;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FishClubAlginet.Infrastructure.Repositories;

public class GenericRepository<T, TId> : IGenericRepository<T, TId>
    where T : BaseEntity<TId>
{
    private readonly AppDbContext _context;

    public GenericRepository(AppDbContext context) => _context = context;


    public async Task<ErrorOr<T>> AddAsync(T entity)
    {
        try
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity; 
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx)
            {
                if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                {
                    return Error.Conflict(
                        code: $"{typeof(T).Name}.Duplicate",
                        description: "Ya existe un registro con esos datos únicos.");
                }
            }

            return Error.Failure(
                code: "Database.SaveFailure",
                description: $"Error al guardar {typeof(T).Name}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Error.Unexpected(
                code: "System.Unexpected",
                description: ex.Message);
        }
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
