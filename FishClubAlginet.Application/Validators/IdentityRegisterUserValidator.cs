namespace FishClubAlginet.Application.Validators;

public class IdentityRegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public IdentityRegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithErrorCode("Auth.Email.Required")
                .WithMessage(ErrorMessages.Auth_Email_Required)
            .EmailAddress()
                .WithErrorCode("Auth.Email.InvalidFormat")
                .WithMessage(ErrorMessages.Auth_Email_InvalidFormat);

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithErrorCode("Auth.Password.Required")
                .WithMessage(ErrorMessages.Auth_Password_Required)
            .MinimumLength(6)
                .WithErrorCode("Auth.Password.MinLength")
                .WithMessage(ErrorMessages.Auth_Password_MinLength);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
                .WithErrorCode("Auth.ConfirmPassword.MustMatch")
                .WithMessage(ErrorMessages.Auth_ConfirmPassword_MustMatch);
    }
}
