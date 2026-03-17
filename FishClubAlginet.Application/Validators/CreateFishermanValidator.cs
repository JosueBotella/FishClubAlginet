namespace FishClubAlginet.Application.Validators;

public class CreateFishermanValidator : AbstractValidator<CreateFishermanDto>
{
    public CreateFishermanValidator()
    {
        // REGLA 1: El nombre es obligatorio
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Name is required");

        // REGLA 2: El apellido es obligatorio
        RuleFor(x => x.LastName)
            .NotEmpty();

        // REGLA 3: Validación compleja del DNI (usando nuestra extensión)
        RuleFor(x => x.DocumentNumber)
            .MustBeValidIdentification(dto => dto.DocumentType);
    }
}
