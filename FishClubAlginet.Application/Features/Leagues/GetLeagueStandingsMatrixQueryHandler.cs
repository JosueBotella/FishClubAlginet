using FishClubAlginet.Application.Abstractions;
using FishClubAlginet.Contracts.Dtos.Responses.Competition;
using MediatR;
using ErrorOr;

namespace FishClubAlginet.Application.Features.Leagues;

public record GetLeagueStandingsMatrixQuery(Guid LeagueId)
    : IRequest<ErrorOr<LeagueStandingsMatrixDto>>;

public sealed class GetLeagueStandingsMatrixQueryHandler
    : IRequestHandler<GetLeagueStandingsMatrixQuery, ErrorOr<LeagueStandingsMatrixDto>>
{
    private readonly IGenericRepository<League, Guid> _leagueRepository;
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IGenericRepository<Fisherman, int> _fishermanRepository;

    public GetLeagueStandingsMatrixQueryHandler(
        IGenericRepository<League, Guid> leagueRepository,
        IGenericRepository<Competition, Guid> competitionRepository,
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IGenericRepository<Fisherman, int> fishermanRepository)
    {
        _leagueRepository = leagueRepository;
        _competitionRepository = competitionRepository;
        _resultRepository = resultRepository;
        _fishermanRepository = fishermanRepository;
    }

    public Task<ErrorOr<LeagueStandingsMatrixDto>> Handle(
        GetLeagueStandingsMatrixQuery request,
        CancellationToken cancellationToken)
    {
        var league = _leagueRepository.GetAll()
            .FirstOrDefault(l => l.Id == request.LeagueId && !l.IsDeleted);

        if (league is null)
            return Task.FromResult<ErrorOr<LeagueStandingsMatrixDto>>(Errors.League.NotFound);

        // 1. Get all active competitions for the league, ordered by competition number
        var competitions = _competitionRepository.GetAll()
            .Where(c => c.LeagueId == request.LeagueId && !c.IsDeleted)
            .OrderBy(c => c.CompetitionNumber)
            .Select(c => new CompetitionHeaderDto(c.Id, c.CompetitionNumber, c.Name, c.Date))
            .ToList();

        var compIds = competitions.Select(c => c.Id).ToList();

        // 2. Get all results for these competitions
        var results = _resultRepository.GetAll()
            .Where(r => compIds.Contains(r.CompetitionId) && !r.IsDeleted)
            .ToList();

        // 3. Resolve fisherman names
        var fishermanIds = results.Select(r => r.FishermanId).Distinct().ToList();
        var fishermen = _fishermanRepository.GetAll()
            .Where(f => fishermanIds.Contains(f.Id))
            .ToDictionary(f => f.Id, f => f.FirstName + " " + f.LastName);

        // 4. Group by fisherman and calculate detailed matrix with Discards
        var rows = new List<FishermanMatrixRowDto>();
        foreach (var fid in fishermanIds)
        {
            var fName = fishermen.TryGetValue(fid, out var name) ? name : $"Fisherman #{fid}";
            var fResults = results.Where(r => r.FishermanId == fid).ToList();

            // Calculate discards: order attended results by Points ascending
            var attendedResults = fResults.Where(r => r.DidAttend).OrderBy(r => r.Points).ToList();
            var discardCount = Math.Min(league.WorstResultsToDiscard, attendedResults.Count);
            var discardedIds = attendedResults.Take(discardCount).Select(r => r.Id).ToHashSet();

            var cellMap = new Dictionary<Guid, CompetitionCellDto>();
            foreach (var comp in competitions)
            {
                var res = fResults.FirstOrDefault(r => r.CompetitionId == comp.Id);
                if (res is null)
                {
                    // Absent / Not registered
                    cellMap[comp.Id] = new CompetitionCellDto(0, 0, 0, false, false);
                }
                else
                {
                    var isDiscarded = discardedIds.Contains(res.Id);
                    cellMap[comp.Id] = new CompetitionCellDto(
                        res.WeightInGrams,
                        res.Points,
                        res.Ranking,
                        res.DidAttend,
                        isDiscarded);
                }
            }

            var totalWeight = fResults.Where(r => r.DidAttend).Sum(r => r.WeightInGrams);
            var totalPoints = fResults.Where(r => r.DidAttend).Sum(r => r.Points);
            var pointsAfterDiscard = fResults.Where(r => r.DidAttend && !discardedIds.Contains(r.Id)).Sum(r => r.Points);
            var attendedCount = fResults.Count(r => r.DidAttend);

            rows.Add(new FishermanMatrixRowDto(
                fid,
                fName,
                totalWeight,
                totalPoints,
                pointsAfterDiscard,
                attendedCount,
                cellMap));
        }

        // Sort rows by Points (PointsAfterDiscard DESC, then TotalWeight DESC) and Weight (TotalWeight DESC, then PointsAfterDiscard DESC)
        var byPoints = rows.OrderByDescending(r => r.PointsAfterDiscard).ThenByDescending(r => r.TotalWeightGrams).ToList();
        var byWeight = rows.OrderByDescending(r => r.TotalWeightGrams).ThenByDescending(r => r.PointsAfterDiscard).ToList();

        var dto = new LeagueStandingsMatrixDto(
            league.Id,
            league.Name,
            league.Year,
            league.WorstResultsToDiscard,
            competitions,
            byPoints,
            byWeight);

        return Task.FromResult<ErrorOr<LeagueStandingsMatrixDto>>(dto);
    }
}
