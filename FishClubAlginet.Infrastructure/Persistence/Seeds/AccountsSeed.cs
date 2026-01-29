namespace FishClubAlginet.Infrastructure.Persistence.Seeds;

public static class AccountsSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var passwordHasher = new PasswordHasher<IdentityUser>();
        var users = new IdentityUser[]
        {
            new IdentityUser
            {
                UserName = "jbotella@mail.com",
                Email = "jbotella@mail.com",
                NormalizedEmail = "jbotella@mail.com",
                EmailConfirmed = true,
                NormalizedUserName = "Josue",
                PasswordHash = passwordHasher.HashPassword(
                    user: new IdentityUser { UserName = "jbotella@mail.com" },
                    password: "a5848b"
                ),
            }
        };



        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
