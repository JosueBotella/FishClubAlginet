namespace FishClubAlginet.Application.Features.Leagues;

public record LeagueFishermanStandingDto(
    int FishermanId,
    string FullName,
    int TotalWeightGrams,
    decimal TotalPoints,
    decimal PointsAfterDiscard,
    int CompetitionsAttended);

public record LeagueStandingsDto(
    Guid LeagueId,
    string LeagueName,
    int Year,
    int WorstResultsToDiscard,
    IReadOnlyList<LeagueFishermanStandingDto> ByWeight,
    IReadOnlyList<LeagueFishermanStandingDto> ByPoints);

public record GetLeagueStandingsQuery(Guid LeagueId)
    : IRequest<ErrorOr<LeagueStandingsDto>>;

public sealed class GetLeagueStandingsQueryHandler
    : IRequestHandler<GetLeagueStandingsQuery, ErrorOr<LeagueStandingsDto>>
{
    private readonly IGenericRepository<League, Guid> _leagueRepository;
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IGenericRepository<Fisherman, int> _fishermanRepository;

    public GetLeagueStandingsQueryHandler(
        IGenericRepository<League, Guid> leagueRepository,
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IGenericRepository<Fisherman, int> fishermanRepository)
    {
        _leagueRepository = leagueRepository;
        _resultRepository = resultRepository;
        _fishermanRepository = fishermanRepository;
    }

    public Task<ErrorOr<LeagueStandingsDto>> Handle(
        GetLeagueStandingsQuery request,
        CancellationToken cancellationToken)
    {
        var league = _leagueRepository.GetAll()
            .FirstOrDefault(l => l.Id == request.LeagueId && !l.IsDeleted);

        if (league is null)
            return Task.FromResult<ErrorOr<LeagueStandingsDto>>(Errors.League.NotFound);

        // Collect all results for competitions of this league that have been attended
        var results = _resultRepository.GetAll()
            .Where(r => !r.IsDeleted
                     && r.DidAttend
                     && r.Competition.LeagueId == request.LeagueId)
            .Select(r => new
            {
                r.FishermanId,
                r.WeightInGrams,
                r.Points
            })
            .ToList();

        // Collect fisherman names
        var fishermanIds = results.Select(r => r.FishermanId).Distinct().ToList();
        var fishermen = _fishermanRepository.GetAll()
            .Where(f => fishermanIds.Contains(f.Id))
            .Select(f => new { f.Id, FullName = f.FirstName + " " + f.LastName })
            .ToDictionary(f => f.Id, f => f.FullName);

        // Group by fisherman and compute aggregates
        var grouped = results
            .GroupBy(r => r.FishermanId)
            .Select(g =>
            {
                var allPoints = g.Select(r => r.Points).OrderBy(p => p).ToList();
                var discard = Math.Min(league.WorstResultsToDiscard, allPoints.Count);
                var pointsAfterDiscard = allPoints.Skip(discard).Sum();
                var name = fishermen.TryGetValue(g.Key, out var n) ? n : $"Fisherman #{g.Key}";

                return new LeagueFishermanStandingDto(
                    FishermanId: g.Key,
                    FullName: name,
                    TotalWeightGrams: g.Sum(r => r.WeightInGrams),
                    TotalPoints: g.Sum(r => r.Points),
                    PointsAfterDiscard: pointsAfterDiscard,
                    CompetitionsAttended: g.Count());
            })
            .ToList();

        var byWeight = grouped
            .OrderByDescending(s => s.TotalWeightGrams)
            .ThenByDescending(s => s.TotalPoints)
            .ToList();

        var byPoints = grouped
            .OrderByDescending(s => s.PointsAfterDiscard)
            .ThenByDescending(s => s.TotalWeightGrams)
            .ToList();

        var dto = new LeagueStandingsDto(
            league.Id,
            league.Name,
            league.Year,
            league.WorstResultsToDiscard,
            byWeight,
            byPoints);

        return Task.FromResult<ErrorOr<LeagueStandingsDto>>(dto);
    }
}
