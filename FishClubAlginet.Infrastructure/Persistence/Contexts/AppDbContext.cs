namespace FishClubAlginet.Infrastructure.Persistence.Contexts;

public class AppDbContext : IdentityDbContext
{
    public DbSet<Fisherman> Fishermen { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> context)
        : base(context)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
