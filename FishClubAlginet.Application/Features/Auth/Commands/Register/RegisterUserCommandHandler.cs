namespace FishClubAlginet.Application.Features.Auth.Commands;


public record RegisterUserCommand( 
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<ErrorOr<string>>; 


public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, string>
{
    private readonly IAuthService _authService;

    public RegisterUserHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ErrorOr<string>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (command.Password != command.ConfirmPassword)
            return Error.Validation("Auth.PasswordMismatch", "Las contraseñas no coinciden.");

        var registerDto = new RegisterUserDto
        {
            
            Email = command.Email,
            Password = command.Password,
            ConfirmPassword = command.ConfirmPassword
        };

        IdentityResult result = await _authService.RegisterAsync(registerDto);

        if (!result.Succeeded)
        {
           
            return result.Errors
                .Select(e => Error.Validation(
                    code: e.Code,
                    description: e.Description))
                .ToList();
        }

        
        var loginDto = new LoginDto { UserName = command.Email, Password = command.Password };
        var token = await _authService.LoginAsync(loginDto);

        return token;
    }
}
