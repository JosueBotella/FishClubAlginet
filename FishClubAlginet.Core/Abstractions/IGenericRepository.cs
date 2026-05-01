namespace FishClubAlginet.Application.Abstractions;

public interface IGenericRepository<T, TId>
    where T : BaseEntity<TId>
{
    /// <summary>
    /// Stagea la entidad en el ChangeTracker. La persistencia real ocurre cuando
    /// el handler llama a IUnitOfWork.SaveChangesAsync().
    /// </summary>
    Task<T> AddAsync(T entity);

    Task<T?> GetById(TId id);

    IQueryable<T> GetAll();

    /// <summary>
    /// Marca la entidad como modificada en el ChangeTracker.
    /// La persistencia real ocurre vía IUnitOfWork.SaveChangesAsync().
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Marca la entidad como eliminada lógicamente (IsDeleted=true).
    /// Devuelve false si no existe la entidad.
    /// La persistencia real ocurre vía IUnitOfWork.SaveChangesAsync().
    /// </summary>
    Task<bool> SoftDelete(TId id);

    /// <summary>
    /// Marca la entidad para borrado físico.
    /// Devuelve false si no existe la entidad.
    /// La persistencia real ocurre vía IUnitOfWork.SaveChangesAsync().
    /// </summary>
    Task<bool> HardDelete(TId id);
}
