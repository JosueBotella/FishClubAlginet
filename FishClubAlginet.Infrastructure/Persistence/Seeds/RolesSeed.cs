using FishClubAlginet.Application.Constants;
using Microsoft.AspNetCore.Identity;

namespace FishClubAlginet.Infrastructure.Persistence.Seeds;

public static class RolesSeed
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = [ApplicationConstants.Roles.Admin, ApplicationConstants.Roles.Fisherman];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
