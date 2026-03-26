namespace FishClubAlginet.Application.Validators;

public class CreateFishermanValidator : AbstractValidator<CreateFishermanDto>
{
    public CreateFishermanValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
                .WithErrorCode(ValidatorsConstants.FisherManValidationConstants.FirstNameRequiredErrorCode)
                .WithMessage(ValidatorsConstants.FisherManValidationConstants.FirstNameRequiredErrorMessage);

        RuleFor(x => x.LastName)
            .NotEmpty()
                .WithErrorCode(ValidatorsConstants.FisherManValidationConstants.LastNameRequiredErrorCode)
                .WithMessage(ValidatorsConstants.FisherManValidationConstants.LastNameRequiredErrorMessage);

        RuleFor(x => x.DocumentNumber)
            .MustBeValidIdentification(dto => dto.DocumentType);
    }
}
