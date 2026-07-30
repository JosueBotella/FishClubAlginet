using System.Text.Json;
using FishClubAlginet.Contracts.Dtos.Responses.League;

namespace FishClubAlginet.Application.Features.Leagues;

public record ArchiveLeagueCommand(Guid Id) : IRequest<ErrorOr<LeagueDto>>;

public sealed class ArchiveLeagueCommandHandler
    : IRequestHandler<ArchiveLeagueCommand, ErrorOr<LeagueDto>>
{
    private readonly IGenericRepository<League, Guid> _repository;
    private readonly IGenericRepository<LeagueSeasonSnapshot, Guid> _snapshotRepository;
    private readonly ISender _sender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ArchiveLeagueCommandHandler> _logger;

    public ArchiveLeagueCommandHandler(
        IGenericRepository<League, Guid> repository,
        IGenericRepository<LeagueSeasonSnapshot, Guid> snapshotRepository,
        ISender sender,
        IUnitOfWork unitOfWork,
        ILogger<ArchiveLeagueCommandHandler> logger)
    {
        _repository = repository;
        _snapshotRepository = snapshotRepository;
        _sender = sender;
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

        // Generate standings matrix snapshot
        var standingsResult = await _sender.Send(new GetLeagueStandingsMatrixQuery(request.Id), cancellationToken);
        var snapshotJson = standingsResult.IsError
            ? "{}"
            : JsonSerializer.Serialize(standingsResult.Value);

        var snapshot = LeagueSeasonSnapshot.Create(league.Id, league.Year, snapshotJson);
        await _snapshotRepository.AddAsync(snapshot);

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

        _logger.LogInformation("League {LeagueId} ({LeagueYear}) archived successfully with snapshot",
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
