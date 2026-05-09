namespace FishClubAlginet.Application.Features.Competitions;

public record GetCompetitionsByLeagueQuery(Guid LeagueId)
    : IRequest<ErrorOr<CompetitionDto[]>>;

public sealed class GetCompetitionsByLeagueQueryHandler
    : IRequestHandler<GetCompetitionsByLeagueQuery, ErrorOr<CompetitionDto[]>>
{
    private readonly IGenericRepository<League, Guid> _leagueRepository;
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;

    public GetCompetitionsByLeagueQueryHandler(
        IGenericRepository<League, Guid> leagueRepository,
        IGenericRepository<Competition, Guid> competitionRepository)
    {
        _leagueRepository = leagueRepository;
        _competitionRepository = competitionRepository;
    }

    public Task<ErrorOr<CompetitionDto[]>> Handle(
        GetCompetitionsByLeagueQuery request,
        CancellationToken cancellationToken)
    {
        var leagueExists = _leagueRepository
            .GetAll()
            .Any(l => l.Id == request.LeagueId && !l.IsDeleted);

        if (!leagueExists)
            return Task.FromResult<ErrorOr<CompetitionDto[]>>(Errors.League.NotFound);

        var dtos = _competitionRepository
            .GetAll()
            .Where(c => c.LeagueId == request.LeagueId && !c.IsDeleted)
            .OrderBy(c => c.CompetitionNumber)
            .Select(c => new CompetitionDto(
                c.Id,
                c.LeagueId,
                c.CompetitionNumber,
                c.Name,
                c.Date,
                c.StartTime,
                c.EndTime,
                c.Venue,
                c.Zone,
                c.Subspecialty.ToString(),
                c.Category.ToString(),
                c.Status.ToString(),
                c.MaxSpots,
                c.ParticipantCount,
                c.LastUpdateUtc))
            .ToArray();

        return Task.FromResult<ErrorOr<CompetitionDto[]>>(dtos);
    }
}
