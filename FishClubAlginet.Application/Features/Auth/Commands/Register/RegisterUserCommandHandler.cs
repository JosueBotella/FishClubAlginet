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
            return Error.Validation("Auth.PasswordMismatch", ErrorMessages.Auth_PasswordMismatch);

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
                .Select(e => Error.Validation(code: e.Code, description: e.Description))
                .ToList();
        }

        var loginDto = new LoginDto { UserName = command.Email, Password = command.Password };
        var token = await _authService.LoginAsync(loginDto);

        if (token is null)
            return Error.Failure("Auth.TokenGenerationFailed", ErrorMessages.Auth_TokenGenerationFailed);

        return token;
    }

    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithErrorCode("Auth.Email.Required").WithMessage(ErrorMessages.Auth_Email_Required)
                .EmailAddress().WithErrorCode("Auth.Email.InvalidFormat").WithMessage(ErrorMessages.Auth_Email_InvalidFormat);

            RuleFor(x => x.Password)
                .NotEmpty().WithErrorCode("Auth.Password.Required").WithMessage(ErrorMessages.Auth_Password_Required)
                .MinimumLength(6).WithErrorCode("Auth.Password.MinLength").WithMessage(ErrorMessages.Auth_Password_MinLength);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithErrorCode("Auth.ConfirmPassword.MustMatch").WithMessage(ErrorMessages.Auth_ConfirmPassword_MustMatch);
        }
    }
}
