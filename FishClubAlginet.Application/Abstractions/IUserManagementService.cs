using FishClubAlginet.Contracts.Dtos.Common;
using FishClubAlginet.Contracts.Dtos.Responses.Users;

namespace FishClubAlginet.Application.Abstractions;

public interface IUserManagementService
{
    Task<IList<UserDto>> GetAllUsersAsync();
    Task<PaginatedResult<UserDto>> GetUsersPagedAsync(int skip, int take, string? search);
    Task<IdentityResult> BlockUserAsync(string userId);
    Task<IdentityResult> UnblockUserAsync(string userId);
    Task<IdentityResult> AssignRoleAsync(string userId, string role);
    Task<IdentityResult> RemoveRoleAsync(string userId, string role);
    Task<ErrorOr<string>> CreateUserWithRoleAsync(string email, string password, string role);
}
