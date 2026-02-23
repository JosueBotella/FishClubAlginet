using FishClubAlginet.Infrastructure.Persistence.Seeds;

namespace FishClubAlginet.Infrastructure.Persistence.DbInitializer;

public static class DbInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            var context = services.GetRequiredService<AppDbContext>();

            //await context.Database.EnsureDeletedAsync(); // Cuidado con esto
            await context.Database.MigrateAsync();
            await AccountsSeed.SeedAsync(context);
            await FishermanSeed.SeedAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
            throw;
        }
    }
}
