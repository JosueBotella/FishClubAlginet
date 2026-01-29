namespace FishClubAlginet.Core.Application.Interfaces;

public interface IGenericRepository<T, TId>
    where T : BaseEntity<TId>
{
    Task<T> Insert(T entity);
    Task<T?> GetById(TId id);
    IQueryable<T> GetAll();
    void Update(T entity);
    Task<bool> SoftDelete(TId id);
    Task<bool> HardDelete(TId id);
}
