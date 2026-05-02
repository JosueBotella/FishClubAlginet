using FishClubAlginet.Application.Features.Events.Commands.Fishermen;

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


public class FisherManAddCommandHandler : IRequestHandler<FisherManCommand, ErrorOr<int>>
{
    private readonly IGenericRepository<Fisherman,int> _genericRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FisherManAddCommandHandler> _logger;

    public FisherManAddCommandHandler(
        IGenericRepository<Fisherman, int> genericRepository,
        IUnitOfWork unitOfWork,
        ILogger<FisherManAddCommandHandler> logger)
    {
        _genericRepository = genericRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<int>> Handle(FisherManCommand command, CancellationToken cancellationToken)
    {
        // Use the factory method to create the fisherman
        var fisherman = Fisherman.Create(
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            command.DocumentType,
            command.DocumentNumber,
            command.FederationLicense,
            new Address
            {
                Street = command.AddressStreet ?? string.Empty,
                City = command.AddressCity ?? string.Empty,
                ZipCode = command.AddressZipCode ?? string.Empty,
                Province = command.AddressProvince ?? string.Empty
            }
        );

        // Raise the domain event BEFORE saving to ensure it's captured by the interceptor
        fisherman.RaiseDomainEvent(new FishermanAddedDomainEvent
        {
            Id = 0, // Will be set by the database
            FirstName = fisherman.FirstName,
            LastName = fisherman.LastName,
            DocumentNumber = fisherman.DocumentNumber
        });

        // El repositorio sólo "stagea" en el ChangeTracker; el UoW persiste todo
        // junto en una transacción ACID, capturando también los OutboxMessages
        // generados por el ConvertDomainEventsToOutboxMessagesInterceptor.
        await _genericRepository.AddAsync(fisherman);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error creating Fisherman: {Errors}",
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));

            // Si la causa es un duplicado, lo refinamos a un error específico de Fisherman.
            if (saveResult.FirstError.Code == "Database.UniqueConstraintViolation")
            {
                return Error.Conflict(
                    code: $"{nameof(Fisherman)}.Duplicate",
                    description: "A fisherman with these unique values already exists.");
            }

            return Error.Failure(
                code: "FISHERMAN_SAVE_FAILED",
                description: "Could not create the fisherman. Please try again.");
        }

        _logger.LogInformation("Fisherman created successfully with ID: {FishermanId}", fisherman.Id);
        return fisherman.Id;
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

