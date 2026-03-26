using FishClubAlginet.Infrastructure.Persistence.Seeds;
using Microsoft.AspNetCore.Identity;

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
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            //await context.Database.EnsureDeletedAsync(); // Cuidado con esto
            await context.Database.MigrateAsync();
            await RolesSeed.SeedAsync(roleManager);
            await AccountsSeed.SeedAsync(context, userManager);
            await FishermanSeed.SeedAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
