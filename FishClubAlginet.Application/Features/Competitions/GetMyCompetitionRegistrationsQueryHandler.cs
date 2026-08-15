using FishClubAlginet.Contracts.Dtos.Responses.Competition;

namespace FishClubAlginet.Application.Features.Competitions;

public record GetMyCompetitionRegistrationsQuery(string UserId)
    : IRequest<ErrorOr<List<MyCompetitionRegistrationDto>>>;

public sealed class GetMyCompetitionRegistrationsQueryHandler
    : IRequestHandler<GetMyCompetitionRegistrationsQuery, ErrorOr<List<MyCompetitionRegistrationDto>>>
{
    private readonly IGenericRepository<Fisherman, int> _fishermanRepository;
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IGenericRepository<League, Guid> _leagueRepository;
    private readonly ILogger<GetMyCompetitionRegistrationsQueryHandler> _logger;

    public GetMyCompetitionRegistrationsQueryHandler(
        IGenericRepository<Fisherman, int> fishermanRepository,
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IGenericRepository<Competition, Guid> competitionRepository,
        IGenericRepository<League, Guid> leagueRepository,
        ILogger<GetMyCompetitionRegistrationsQueryHandler> logger)
    {
        _fishermanRepository = fishermanRepository;
        _resultRepository = resultRepository;
        _competitionRepository = competitionRepository;
        _leagueRepository = leagueRepository;
        _logger = logger;
    }

    public Task<ErrorOr<List<MyCompetitionRegistrationDto>>> Handle(
        GetMyCompetitionRegistrationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fisherman = _fishermanRepository.GetAll()
                .FirstOrDefault(f => f.UserId == request.UserId && !f.IsDeleted);

            if (fisherman is null)
            {
                return Task.FromResult<ErrorOr<List<MyCompetitionRegistrationDto>>>(
                    Errors.FishermanErrors.NotFound);
            }

            var results = _resultRepository.GetAll()
                .Where(r => r.FishermanId == fisherman.Id && !r.IsDeleted)
                .ToList();

            if (!results.Any())
            {
                return Task.FromResult<ErrorOr<List<MyCompetitionRegistrationDto>>>(
                    new List<MyCompetitionRegistrationDto>());
            }

            var competitionIds = results.Select(r => r.CompetitionId).Distinct().ToList();
            var competitions = _competitionRepository.GetAll()
                .Where(c => competitionIds.Contains(c.Id) && !c.IsDeleted)
                .ToDictionary(c => c.Id);

            var leagueIds = competitions.Values.Select(c => c.LeagueId).Distinct().ToList();
            var leagues = _leagueRepository.GetAll()
                .Where(l => leagueIds.Contains(l.Id))
                .ToDictionary(l => l.Id);

            var dtos = new List<MyCompetitionRegistrationDto>();

            foreach (var r in results)
            {
                if (!competitions.TryGetValue(r.CompetitionId, out var comp))
                    continue;

                var leagueName = leagues.TryGetValue(comp.LeagueId, out var league)
                    ? league.Name
                    : string.Empty;

                dtos.Add(new MyCompetitionRegistrationDto(
                    ResultId: r.Id,
                    CompetitionId: comp.Id,
                    CompetitionName: comp.Name,
                    CompetitionNumber: comp.CompetitionNumber,
                    LeagueId: comp.LeagueId,
                    LeagueName: leagueName,
                    Date: comp.Date,
                    StartTime: comp.StartTime,
                    EndTime: comp.EndTime,
                    Venue: comp.Venue,
                    Zone: comp.Zone,
                    Subspecialty: comp.Subspecialty.ToString(),
                    Category: comp.Category.ToString(),
                    Status: comp.Status.ToString(),
                    AssignedSpotNumber: r.AssignedSpotNumber,
                    WeightInGrams: r.WeightInGrams,
                    BiggestCatchWeight: r.BiggestCatchWeight,
                    Points: r.Points,
                    Ranking: r.Ranking,
                    IsValidated: r.IsValidated,
                    DidAttend: r.DidAttend,
                    RegistrationDate: r.RegistrationDate));
            }

            var sorted = dtos.OrderByDescending(d => d.Date).ToList();
            return Task.FromResult<ErrorOr<List<MyCompetitionRegistrationDto>>>(sorted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my competition registrations for user {UserId}", request.UserId);
            return Task.FromResult<ErrorOr<List<MyCompetitionRegistrationDto>>>(
                Error.Failure(ValidatorsConstants.UnexpectedErrorCode, ValidatorsConstants.UnexpectedErrorMessage));
        }
    }
}
