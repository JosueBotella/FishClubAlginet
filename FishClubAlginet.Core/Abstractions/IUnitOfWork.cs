namespace FishClubAlginet.Application.Abstractions;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T, TId> Repository<T, TId>() where T : BaseEntity<TId>;
    Task<int> Save();
}
