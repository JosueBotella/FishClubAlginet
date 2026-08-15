namespace FishClubAlginet.Infrastructure.Persistence.DbInitializer;

public static class DbInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<AppDbContext>>();

        const int maxRetries = 10;
        var retryCount = 0;

        while (true)
        {
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

                await context.Database.MigrateAsync();
                await RolesSeed.SeedAsync(roleManager);
                await AccountsSeed.SeedAsync(context, userManager);
                await FishermanSeed.SeedAsync(context);
                logger.LogInformation("Database initialized and seeded successfully.");
                break;
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                retryCount++;
                logger.LogWarning(ex, "Database connection not ready yet. Retrying {RetryCount}/{MaxRetries} in 3 seconds...", retryCount, maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database after {MaxRetries} retries.", maxRetries);
                throw;
            }
        }
    }
}
