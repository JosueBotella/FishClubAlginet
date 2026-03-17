namespace FishClubAlginet.Application.Validators;

public class IdentityRegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public IdentityRegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidatorsConstants.EmptyField)
            .EmailAddress().WithMessage(ValidatorsConstants.InvalidEmailFormat);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidatorsConstants.EmptyField)            
            .MinimumLength(6).WithMessage(ValidatorsConstants.MinimumLength);
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage(ValidatorsConstants.PasswordsDoNotMatch);
    }
}
