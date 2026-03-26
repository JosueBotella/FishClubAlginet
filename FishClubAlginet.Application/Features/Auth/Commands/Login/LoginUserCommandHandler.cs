namespace FishClubAlginet.Application.Features.Auth.Commands;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<ErrorOr<string>>;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, ErrorOr<string>>
{
    private readonly IAuthService _authService;

    public LoginUserHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ErrorOr<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var loginDto = new LoginDto
        {
            UserName = command.Email,
            Password = command.Password
        };

        try
        {
            var token = await _authService.LoginAsync(loginDto);

            if (string.IsNullOrEmpty(token))
                return Error.Validation("Auth.InvalidCredentials", ErrorMessages.Auth_InvalidCredentials);

            return token;
        }
        catch (Exception ex)
        {
            return Error.Failure("Auth.LoginFailed", ex.Message);
        }
    }
}
