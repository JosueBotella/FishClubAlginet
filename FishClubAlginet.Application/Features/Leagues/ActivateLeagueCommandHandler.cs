using FishClubAlginet.Contracts.Dtos.Responses.League;

namespace FishClubAlginet.Application.Features.Leagues;

public record ActivateLeagueCommand(Guid Id) : IRequest<ErrorOr<LeagueDto>>;

public sealed class ActivateLeagueCommandHandler
    : IRequestHandler<ActivateLeagueCommand, ErrorOr<LeagueDto>>
{
    private readonly IGenericRepository<League, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateLeagueCommandHandler> _logger;

    public ActivateLeagueCommandHandler(
        IGenericRepository<League, Guid> repository,
        IUnitOfWork unitOfWork,
        ILogger<ActivateLeagueCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<LeagueDto>> Handle(ActivateLeagueCommand request, CancellationToken cancellationToken)
    {
        var league = _repository.GetAll()
            .FirstOrDefault(l => l.Id == request.Id && !l.IsDeleted);

        if (league is null)
        {
            _logger.LogWarning("League {LeagueId} not found for activation", request.Id);
            return Errors.League.NotFound;
        }

        if (league.IsArchived)
        {
            _logger.LogWarning("Cannot activate archived league {LeagueId}", request.Id);
            return Errors.League.CannotModifyArchived;
        }

        if (league.IsActive)
        {
            _logger.LogWarning("League {LeagueId} is already active", request.Id);
            return Errors.League.AlreadyActive;
        }

        // Deactivate all other active leagues (there should be only one, but be safe)
        var activeLeagues = _repository.GetAll()
            .Where(l => l.IsActive && l.Id != request.Id && !l.IsDeleted)
            .ToList();

        foreach (var active in activeLeagues)
        {
            active.Deactivate();
            _repository.Update(active);
            _logger.LogInformation("Deactivated league {LeagueId} ({LeagueYear}) due to activation of {TargetLeagueId}",
                active.Id, active.Year, request.Id);
        }

        league.Activate();
        _repository.Update(league);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error activating league {LeagueId}: {Errors}",
                request.Id,
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));
            return saveResult.Errors;
        }

        _logger.LogInformation("League {LeagueId} ({LeagueYear}) activated successfully",
            league.Id, league.Year);
        return MapToDto(league);
    }

    private static LeagueDto MapToDto(League league)
    {
        return new LeagueDto(
            league.Id,
            league.Name,
            league.Year,
            league.IsActive,
            league.IsArchived,
            league.MinPoints,
            league.WorstResultsToDiscard,
            league.Competitions.Count,
            league.LastUpdateUtc);
    }
}
