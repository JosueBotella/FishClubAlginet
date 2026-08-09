namespace FishClubAlginet.Application.Features.Fishermen;

public record RestoreFishermanCommand(int Id) : IRequest<ErrorOr<bool>>;

public sealed class RestoreFishermanCommandHandler
    : IRequestHandler<RestoreFishermanCommand, ErrorOr<bool>>
{
    private readonly IGenericRepository<Fisherman, int> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RestoreFishermanCommandHandler> _logger;

    public RestoreFishermanCommandHandler(
        IGenericRepository<Fisherman, int> repository,
        IUnitOfWork unitOfWork,
        ILogger<RestoreFishermanCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(
        RestoreFishermanCommand request,
        CancellationToken cancellationToken)
    {
        var fisherman = await _repository.GetById(request.Id);

        if (fisherman is null)
        {
            _logger.LogWarning("Fisherman with Id {Id} not found for restore", request.Id);
            return Error.NotFound(
                "Fisherman.NotFound",
                $"Fisherman with Id {request.Id} was not found.");
        }

        if (!fisherman.IsDeleted)
        {
            _logger.LogWarning("Fisherman with Id {Id} is not deleted", request.Id);
            return Error.Validation(
                "Fisherman.NotDeleted",
                $"Fisherman with Id {request.Id} is not deleted.");
        }

        fisherman.Restore();
        fisherman.RaiseDomainEvent(new FishermanRestoredDomainEvent { Id = fisherman.Id });

        _repository.Update(fisherman);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error persisting restore for Fisherman {Id}: {Errors}",
                request.Id,
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));
            return saveResult.Errors;
        }

        _logger.LogInformation("Fisherman with Id {Id} restored successfully", request.Id);
        return true;
    }
}
