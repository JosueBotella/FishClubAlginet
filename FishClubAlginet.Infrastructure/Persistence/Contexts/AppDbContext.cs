namespace FishClubAlginet.Infrastructure.Persistence.Contexts;

public class AppDbContext : IdentityDbContext
{
    public DbSet<Fisherman> Fishermen { get; set; }
    public DbSet<League> Leagues { get; set; }
    public DbSet<CompetitionResult> CompetitionResults { get; set; }
    public DbSet<Competition> Competitions { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> context)
        : base(context)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // El interceptor ConvertDomainEventsToOutboxMessagesInterceptor se registra
    // externamente via DI (AddSingleton + AddDbContext((sp, o) => o.AddInterceptors(...)))
    // para permitir inyección de dependencias en el interceptor en el futuro.
}
