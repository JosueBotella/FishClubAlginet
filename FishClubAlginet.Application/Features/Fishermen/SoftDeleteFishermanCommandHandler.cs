namespace FishClubAlginet.Application.Features.Fishermen;

public record SoftDeleteFishermanCommand(int Id) : IRequest<ErrorOr<bool>>;

public sealed class SoftDeleteFishermanCommandHandler
    : IRequestHandler<SoftDeleteFishermanCommand, ErrorOr<bool>>
{
    private readonly IGenericRepository<Fisherman, int> _repository;
    private readonly ILogger<SoftDeleteFishermanCommandHandler> _logger;

    public SoftDeleteFishermanCommandHandler(
        IGenericRepository<Fisherman, int> repository,
        ILogger<SoftDeleteFishermanCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(
        SoftDeleteFishermanCommand request,
        CancellationToken cancellationToken)
    {
        var deleted = await _repository.SoftDelete(request.Id);

        if (!deleted)
        {
            _logger.LogWarning("Fisherman with Id {Id} not found for soft delete", request.Id);
            return Error.NotFound(
                "Fisherman.NotFound",
                $"Fisherman with Id {request.Id} was not found.");
        }

        _logger.LogInformation("Fisherman with Id {Id} soft deleted", request.Id);
        return true;
    }
}
