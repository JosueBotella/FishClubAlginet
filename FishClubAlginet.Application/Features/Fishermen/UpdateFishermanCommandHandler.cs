using FishClubAlginet.Application.Features.Events.Commands.Fishermen;

namespace FishClubAlginet.Application.Features.Fishermen;

public record UpdateFishermanCommand(
    int Id,
    string FirstName,
    string LastName,
    string? FederationLicense,
    string? AddressStreet,
    string? AddressCity,
    string? AddressZipCode,
    string? AddressProvince
) : IRequest<ErrorOr<bool>>;

public sealed class UpdateFishermanCommandHandler
    : IRequestHandler<UpdateFishermanCommand, ErrorOr<bool>>
{
    private readonly IGenericRepository<Fisherman, int> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFishermanCommandHandler> _logger;

    public UpdateFishermanCommandHandler(
        IGenericRepository<Fisherman, int> repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFishermanCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(
        UpdateFishermanCommand request,
        CancellationToken cancellationToken)
    {
        var fisherman = await _repository.GetById(request.Id);

        if (fisherman is null)
        {
            _logger.LogWarning("Fisherman with Id {Id} not found for update", request.Id);
            return Error.NotFound(
                "Fisherman.NotFound",
                $"Fisherman with Id {request.Id} was not found.");
        }

        // Mutate state via domain method, then raise event before saving
        // (same pattern as FisherManAddCommandHandler — event raised in handler, not in entity)
        fisherman.Update(
            request.FirstName,
            request.LastName,
            request.FederationLicense,
            new Address
            {
                Street = request.AddressStreet ?? string.Empty,
                City = request.AddressCity ?? string.Empty,
                ZipCode = request.AddressZipCode ?? string.Empty,
                Province = request.AddressProvince ?? string.Empty
            });

        fisherman.RaiseDomainEvent(new FishermanUpdatedDomainEvent
        {
            Id = fisherman.Id,
            FirstName = fisherman.FirstName,
            LastName = fisherman.LastName
        });

        _repository.Update(fisherman);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error persisting update for Fisherman {Id}: {Errors}",
                request.Id,
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));
            return saveResult.Errors;
        }

        _logger.LogInformation("Fisherman with Id {Id} updated successfully", request.Id);
        return true;
    }

    public class UpdateFishermanCommandValidator : AbstractValidator<UpdateFishermanCommand>
    {
        public UpdateFishermanCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithErrorCode("Fisherman.Id.Required")
                .WithMessage("Fisherman Id must be greater than zero.");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithErrorCode("Fisherman.FirstName.Required")
                .WithMessage("First name is required.")
                .MaximumLength(FisherManConstraints.FistNameMaxLength)
                .WithMessage($"First name cannot exceed {FisherManConstraints.FistNameMaxLength} characters.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithErrorCode("Fisherman.LastName.Required")
                .WithMessage("Last name is required.")
                .MaximumLength(FisherManConstraints.LastNameMaxLength)
                .WithMessage($"Last name cannot exceed {FisherManConstraints.LastNameMaxLength} characters.");
        }
    }
}
