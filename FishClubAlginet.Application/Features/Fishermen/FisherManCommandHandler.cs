using FishClubAlginet.Core.Domain.Entities;

namespace FishClubAlginet.Application.Features.Auth.Commands;


public record FisherManCommand(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    TypeNationalIdentifier DocumentType,
    string DocumentNumber,
    string FederationLicense
) : IRequest<ErrorOr<string>>; 


public class FisherManCommandHandler : IRequestHandler<FisherManCommand, string>
{
    private readonly IGenericRepository<Fisherman,int> _genericRepository;

    public FisherManCommandHandler(IGenericRepository<Fisherman, int> genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public async Task<ErrorOr<string>> Handle(FisherManCommand command, CancellationToken cancellationToken)
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
        };

        var result  = await _genericRepository.Insert(addFisher);

        if ()
        {
           
           
        }

              

        return result;
    }

    public class FisherManCommandValidator : AbstractValidator<FisherManCommand>
    {
        public FisherManCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("El nombre no puede ser vacío o solo espacios.")
                .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es obligatorio.")
                .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("El apellido no puede ser vacío o solo espacios.")
                .MaximumLength(100).WithMessage("El apellido no puede tener más de 100 caracteres.");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today).WithMessage("La fecha de nacimiento debe ser anterior a la fecha actual.")
                .Must(BeAtLeastMinimumAge).WithMessage("El pescador debe tener al menos 16 años.");

            RuleFor(x => x.DocumentType)
                .Must(dt => Enum.IsDefined(typeof(TypeNationalIdentifier), dt))
                .WithMessage("El tipo de documento no es válido.");

            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage("El número de documento es obligatorio.")
                .Must(num => !string.IsNullOrWhiteSpace(num)).WithMessage("El número de documento no puede ser vacío o solo espacios.")
                .MinimumLength(4).WithMessage("El número de documento debe tener al menos 4 caracteres.")
                .MaximumLength(50).WithMessage("El número de documento no puede tener más de 50 caracteres.")
                .Matches(@"^[A-Za-z0-9\-\s]+$").WithMessage("El número de documento solo puede contener letras, números, guiones y espacios.");

            RuleFor(x => x.FederationLicense)
                .Cascade(CascadeMode.Stop)
                .Must(license => license == null || license.Trim().Length <= 50)
                .WithMessage("La licencia de la federación no puede tener más de 50 caracteres.");
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
}

