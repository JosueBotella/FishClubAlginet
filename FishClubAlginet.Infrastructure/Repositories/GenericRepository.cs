

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
            if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx
                && (sqlEx.Number == 2627 || sqlEx.Number == 2601)) // UNIQUE constraint violation
            {
                return Error.Conflict(
                    code: $"{typeof(T).Name}.Duplicate",
                    description: "A record with these unique values already exists.");
            }

            return Error.Failure(
                code: "Database.SaveFailure",
                description: "Failed to save the record. Please try again.");
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
