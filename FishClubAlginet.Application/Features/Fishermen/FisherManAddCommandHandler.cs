using FluentValidation;

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
                .NotEmpty().WithMessage(ValidatorsConstants.FisherManValidationConstants.FirstNameRequired)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.FirstNameNotWhitespace)
                .MaximumLength(FisherManConstraints.FistNameMaxLength)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.FirstNameMaxLength,
                        FisherManConstraints.FistNameMaxLength));

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(ValidatorsConstants.FisherManValidationConstants.LastNameRequired)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.LastNameNotWhitespace)
                .MaximumLength(FisherManConstraints.LastNameMaxLength)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.LastNameMaxLength,
                        FisherManConstraints.LastNameMaxLength));

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today)
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.BirthDateInPast)
                .Must(BeAtLeastMinimumAge)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.MinimumAgeMessage,
                        FisherManConstraints.MinimumAge));

            RuleFor(x => x.DocumentType)
                .Must(dt => Enum.IsDefined(typeof(TypeNationalIdentifier), dt))
                .WithMessage(ValidatorsConstants.FisherManValidationConstants.InvalidDocumentType);

            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage(ValidatorsConstants.FisherManValidationConstants.DocumentNumberRequired)
                .Must(num => !string.IsNullOrWhiteSpace(num))
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.DocumentNumberNotWhitespace)
                .MinimumLength(FisherManConstraints.DocumentNumberMinLength)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.DocumentNumberMinLength,
                        FisherManConstraints.DocumentNumberMinLength))
                .MaximumLength(FisherManConstraints.DocumentNumberMaxLength)
                    .WithMessage(string.Format(
                        ValidatorsConstants.FisherManValidationConstants.DocumentNumberMaxLength,
                        FisherManConstraints.DocumentNumberMaxLength))
                .Matches(ValidatorsConstants.FisherManValidationConstants.DocumentNumberRegex)
                    .WithMessage(ValidatorsConstants.FisherManValidationConstants.DocumentNumberInvalidFormat);

            RuleFor(x => x.FederationLicense)
                .Cascade(CascadeMode.Stop)
                .Must(license => license == null || license.Trim().Length <= FisherManConstraints.FederationLicenseMaxLength)
                .WithMessage(string.Format(
                    ValidatorsConstants.FisherManValidationConstants.FederationLicenseMaxLengthMessage,
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


    private bool BeAtLeastMinimumAge(DateTime dob)
    {
        const int MinimumAge = 10;
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age))
            age--;
        return age >= MinimumAge;
    }
    
}

