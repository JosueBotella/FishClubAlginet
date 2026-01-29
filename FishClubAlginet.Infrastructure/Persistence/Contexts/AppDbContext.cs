namespace FishClubAlginet.Infrastructure.Persistence.Contexts;

public class AppDbContext : IdentityDbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> context)
        : base(context)
    {
    }

    // Optional: Only use OnConfiguring if options not already configured.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => base.OnConfiguring(optionsBuilder);

    
}
