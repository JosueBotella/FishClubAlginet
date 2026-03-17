namespace FishClubAlginet.Application.Features.Auth.Commands;


public record RegisterUserCommand( 
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<ErrorOr<string>>; 


public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ErrorOr<string>>
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
        if (token is null)
        {
            return Error.Failure("Auth.LoginFailed", "No se pudo generar el token de autenticación.");
        }
        return token;
    }
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .EmailAddress().WithMessage("El formato del email no es válido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");
        }
    }

}
