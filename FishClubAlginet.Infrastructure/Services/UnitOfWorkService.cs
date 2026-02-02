namespace FishClubAlginet.Infrastructure.Services;

public class UnitOfWorkService : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDictionary<Type, dynamic> _repositories = new Dictionary<Type, dynamic>();

    public UnitOfWorkService(AppDbContext context)
    {
        _context = context;
    }   

    public async Task<int> Save()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException concurrencyException)
        {
            Console.WriteLine($"Error de concurrencia{concurrencyException.Message}");            
        }
        return 0;
    }

    public void Dispose() => _context.Dispose();

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
