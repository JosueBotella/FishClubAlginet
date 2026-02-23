namespace FishClubAlginet.Application.Features.Auth.Commands;


public record FisherManCommand(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    TypeNationalIdentifier DocumentType,
    string DocumentNumber,
    string? FederationLicense,
    string? AddressStreet = null,
    string? AddressCity = null,
    string? AddressZipCode = null,
    string? AddressProvince = null
) : IRequest<ErrorOr<int>>; 


public class FisherManAddCommandHandler : IRequestHandler<FisherManCommand, int>
{
    private readonly IGenericRepository<Fisherman,int> _genericRepository;

    public FisherManAddCommandHandler(IGenericRepository<Fisherman, int> genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public async Task<ErrorOr<int>> Handle(FisherManCommand command, CancellationToken cancellationToken)
    {       

        var addFisher = new Fisherman
        {
            Id=0,
            FirstName = command.FirstName,
            LastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            DocumentType = command.DocumentType,
            DocumentNumber = command.DocumentNumber,
            FederationLicense = command.FederationLicense,     
            Address = new Address
            {
                Street = command.AddressStreet ?? string.Empty,
                City = command.AddressCity ?? string.Empty,
                ZipCode = command.AddressZipCode ?? string.Empty,
                Province = command.AddressProvince ?? string.Empty
            }
        };

        var result  = await _genericRepository.AddAsync(addFisher);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Id;
        
    }

  
    public class FisherManCommandValidator : AbstractValidator<FisherManCommand>
    {
        public FisherManCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithErrorCode(ValidatorsConstants.FisherManValidationConstants.FirstNameRequiredErrorCode)
                .WithMessage(ValidatorsConstants.FisherManValidationConstants.FirstNameRequiredErrorMessage)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.FirstNameNotWhitespaceErrorMessage)
                .MaximumLength(FisherManConstraints.FistNameMaxLength)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.FirstNameMaxLengthErrorMessage,
                        FisherManConstraints.FistNameMaxLength));

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(ValidatorsConstants.FisherManValidationConstants.LastNameRequiredErrorMessage)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.LastNameNotWhitespaceErrorMessage)
                .MaximumLength(FisherManConstraints.LastNameMaxLength)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.LastNameMaxLengthErrorMessage,
                        FisherManConstraints.LastNameMaxLength));

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today)
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.BirthDateInPastErrorMessage)
                .Must(BeAtLeastMinimumAge)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.MinimumAgeMessageErrorMessage,
                        FisherManConstraints.MinimumAge));

            RuleFor(x => x.DocumentType)
                .Must(dt => Enum.IsDefined(typeof(TypeNationalIdentifier), dt))
                .WithMessage(ValidatorsConstants.FisherManValidationConstants.InvalidDocumentTypeErrorMessage);

            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage(ValidatorsConstants.FisherManValidationConstants.DocumentNumberRequiredErrorMessage)
                .Must(num => !string.IsNullOrWhiteSpace(num))
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.DocumentNumberNotWhitespaceErrorMessage)
                .MinimumLength(FisherManConstraints.DocumentNumberMinLength)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.DocumentNumberMinLengthErrorMessage,
                        FisherManConstraints.DocumentNumberMinLength))
                .MaximumLength(FisherManConstraints.DocumentNumberMaxLength)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.DocumentNumberMaxLengthErrorMessage,
                        FisherManConstraints.DocumentNumberMaxLength))
                .Matches(ValidatorsConstants.FisherManValidationConstants.DocumentNumberRegex)
                    .WithErrorCode(ValidatorsConstants.FisherManValidationConstants.DocumentNumberInvalidFormatErrorCode)
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.DocumentNumberInvalidFormatErrorMessage);

            RuleFor(x => x.FederationLicense)
                .Cascade(CascadeMode.Stop)
                .Must(license => license == null || license.Trim().Length <= FisherManConstraints.FederationLicenseMaxLength)
                .WithErrorCode(errorCode: ValidatorsConstants.FisherManValidationConstants.FederationLicenseMaxLengthErrorCode)
                .WithMessage(string.Format(
                    ValidatorsConstants.FisherManValidationConstants.FederationLicenseMaxLengthErrorMessage,
                    FisherManConstraints.FederationLicenseMaxLength));
        }

        private bool BeAtLeastMinimumAge(DateTime date)
        {
            var today = DateTime.Today;
            var age = today.Year - date.Year;
            if (date.Date > today.AddYears(-age))
                age--;

            return age >= FisherManConstraints.MinimumAge;
        }
    }  
    
}

