namespace FishClubAlginet.Infrastructure.Persistence.Seeds;

public static class AccountsSeed
{
    public static async Task SeedAsync(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        if (await context.Users.AnyAsync())
            return;

        var adminUser = new IdentityUser
        {
            UserName = SeedConstants.DefaultUserName,
            Email = SeedConstants.DefaultUserName,
            NormalizedEmail = SeedConstants.DefaultUserName.ToUpperInvariant(),
            NormalizedUserName = SeedConstants.DefaultUserName.ToUpperInvariant(),
            EmailConfirmed = true,
        };

        await userManager.CreateAsync(adminUser, SeedConstants.DefaultPassword);
        await userManager.AddToRoleAsync(adminUser, ApplicationConstants.Roles.Admin);
        await userManager.AddToRoleAsync(adminUser, ApplicationConstants.Roles.Fisherman);
    }
}
