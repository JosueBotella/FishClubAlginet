namespace FishClubAlginet.Core.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T, TId> Repository<T, TId>() where T : BaseEntity<TId>;
    Task<int> Save();
}
