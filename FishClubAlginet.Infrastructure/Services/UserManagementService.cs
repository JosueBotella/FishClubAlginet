using FishClubAlginet.Contracts.Dtos.Responses.Users;

namespace FishClubAlginet.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserManagementService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IList<UserDto>> GetAllUsersAsync()
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isLockedOut = await _userManager.IsLockedOutAsync(user);
            result.Add(new UserDto(user.Id, user.Email!, isLockedOut, roles));
        }

        return result;
    }

    public async Task<IdentityResult> BlockUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = $"User with id '{userId}' was not found."
            });

        return await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
    }

    public async Task<IdentityResult> UnblockUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = $"User with id '{userId}' was not found."
            });

        return await _userManager.SetLockoutEndDateAsync(user, null);
    }

    public async Task<IdentityResult> AssignRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = $"User with id '{userId}' was not found."
            });

        if (await _userManager.IsInRoleAsync(user, role))
            return IdentityResult.Success;

        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<ErrorOr<string>> CreateUserWithRoleAsync(string email, string password, string role)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return createResult.Errors
                .Select(e => Error.Failure(e.Code, e.Description))
                .ToList();
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return roleResult.Errors
                .Select(e => Error.Failure(e.Code, e.Description))
                .ToList();
        }

        return user.Id;
    }
}
