namespace FishClubAlginet.Application.Abstractions;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterUserDto registerDto);
    Task<string?> LoginAsync(LoginDto loginDto);
    Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
