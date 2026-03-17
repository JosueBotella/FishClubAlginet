namespace FishClubAlginet.Application.Abstractions;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterUserDto registerDto);
    Task<string?> LoginAsync(LoginDto loginDto);
}
