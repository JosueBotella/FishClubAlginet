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
        else
        {
            if (!await userManager.CheckPasswordAsync(adminUser, SeedConstants.DefaultPassword))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                await userManager.ResetPasswordAsync(adminUser, token, SeedConstants.DefaultPassword);
            }
            if (!await userManager.IsInRoleAsync(adminUser, ApplicationConstants.Roles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, ApplicationConstants.Roles.Admin);
            }
            if (!await userManager.IsInRoleAsync(adminUser, ApplicationConstants.Roles.Fisherman))
            {
                await userManager.AddToRoleAsync(adminUser, ApplicationConstants.Roles.Fisherman);
            }
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
        else
        {
            if (!await userManager.CheckPasswordAsync(fishermanUser, SeedConstants.DefaultPassword))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(fishermanUser);
                await userManager.ResetPasswordAsync(fishermanUser, token, SeedConstants.DefaultPassword);
            }
            if (!await userManager.IsInRoleAsync(fishermanUser, ApplicationConstants.Roles.Fisherman))
            {
                await userManager.AddToRoleAsync(fishermanUser, ApplicationConstants.Roles.Fisherman);
            }
        }
    }
}
