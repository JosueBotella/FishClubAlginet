using FishClubAlginet.Contracts.Dtos.Responses.League;

namespace FishClubAlginet.Application.Features.Leagues;

public record ArchiveLeagueCommand(Guid Id) : IRequest<ErrorOr<LeagueDto>>;

public sealed class ArchiveLeagueCommandHandler
    : IRequestHandler<ArchiveLeagueCommand, ErrorOr<LeagueDto>>
{
    private readonly IGenericRepository<League, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ArchiveLeagueCommandHandler> _logger;

    public ArchiveLeagueCommandHandler(
        IGenericRepository<League, Guid> repository,
        IUnitOfWork unitOfWork,
        ILogger<ArchiveLeagueCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<LeagueDto>> Handle(ArchiveLeagueCommand request, CancellationToken cancellationToken)
    {
        var league = _repository.GetAll()
            .FirstOrDefault(l => l.Id == request.Id && !l.IsDeleted);

        if (league is null)
        {
            _logger.LogWarning("League {LeagueId} not found for archiving", request.Id);
            return Errors.League.NotFound;
        }

        if (league.IsArchived)
        {
            _logger.LogWarning("League {LeagueId} is already archived", request.Id);
            return Errors.League.AlreadyArchived;
        }

        league.Archive();
        _repository.Update(league);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error archiving league {LeagueId}: {Errors}",
                request.Id,
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));
            return saveResult.Errors;
        }

        _logger.LogInformation("League {LeagueId} ({LeagueYear}) archived successfully",
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
