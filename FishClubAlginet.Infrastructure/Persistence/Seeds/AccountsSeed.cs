namespace FishClubAlginet.Infrastructure.Persistence.Seeds;

public static class AccountsSeed
{
    public static async Task SeedAsync(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        // 1. Seed / Ensure Admin User
        var adminUser = await userManager.FindByEmailAsync(SeedConstants.DefaultAdminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                UserName = SeedConstants.DefaultAdminEmail,
                Email = SeedConstants.DefaultAdminEmail,
                NormalizedEmail = SeedConstants.DefaultAdminEmail.ToUpperInvariant(),
                NormalizedUserName = SeedConstants.DefaultAdminEmail.ToUpperInvariant(),
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(adminUser, SeedConstants.DefaultPassword);
            await userManager.AddToRoleAsync(adminUser, ApplicationConstants.Roles.Admin);
            await userManager.AddToRoleAsync(adminUser, ApplicationConstants.Roles.Fisherman);
        }

        // 2. Seed / Ensure Regular Fisherman User
        var fishermanUser = await userManager.FindByEmailAsync(SeedConstants.DefaultFishermanEmail);
        if (fishermanUser is null)
        {
            fishermanUser = new IdentityUser
            {
                UserName = SeedConstants.DefaultFishermanEmail,
                Email = SeedConstants.DefaultFishermanEmail,
                NormalizedEmail = SeedConstants.DefaultFishermanEmail.ToUpperInvariant(),
                NormalizedUserName = SeedConstants.DefaultFishermanEmail.ToUpperInvariant(),
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(fishermanUser, SeedConstants.DefaultPassword);
            await userManager.AddToRoleAsync(fishermanUser, ApplicationConstants.Roles.Fisherman);
        }
    }
}
