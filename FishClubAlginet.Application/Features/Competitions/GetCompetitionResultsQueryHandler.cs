namespace FishClubAlginet.Application.Features.Competitions;

public record GetCompetitionResultsQuery(Guid CompetitionId)
    : IRequest<ErrorOr<CompetitionResultDto[]>>;

public sealed class GetCompetitionResultsQueryHandler
    : IRequestHandler<GetCompetitionResultsQuery, ErrorOr<CompetitionResultDto[]>>
{
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;

    public GetCompetitionResultsQueryHandler(
        IGenericRepository<Competition, Guid> competitionRepository,
        IGenericRepository<CompetitionResult, Guid> resultRepository)
    {
        _competitionRepository = competitionRepository;
        _resultRepository = resultRepository;
    }

    public Task<ErrorOr<CompetitionResultDto[]>> Handle(
        GetCompetitionResultsQuery request,
        CancellationToken cancellationToken)
    {
        var competition = _competitionRepository
            .GetAll()
            .FirstOrDefault(c => c.Id == request.CompetitionId && !c.IsDeleted);

        if (competition is null)
        {
            return Task.FromResult<ErrorOr<CompetitionResultDto[]>>(
                Errors.Competition.NotFound);
        }

        var minWeight = competition.BiggestCatchMinWeightInGrams ?? 0;

        // Load all results for the competition, ordered by weight descending
        // Ranking is calculated in-memory: ties share the same rank
        var rawResults = _resultRepository
            .GetAll()
            .Where(r => r.CompetitionId == request.CompetitionId && !r.IsDeleted)
            .OrderByDescending(r => r.Points)
            .ThenByDescending(r => r.BiggestCatchWeight ?? 0)
            .Select(r => new
            {
                r.Id,
                r.CompetitionId,
                r.FishermanId,
                r.AssignedSpotNumber,
                r.DidAttend,
                r.WeightInGrams,
                r.BiggestCatchWeight,
                r.Points,
                r.IsValidated,
                r.RegistrationDate
            })
            .ToList();

        // Calculate the maximum biggest catch weight in this competition meeting the minimum requirement
        var maxBiggestCatchWeight = rawResults
            .Where(r => r.DidAttend && (r.BiggestCatchWeight ?? 0) >= minWeight)
            .Select(r => r.BiggestCatchWeight)
            .Max() ?? 0;

        // Assign rankings in-memory: same Points + BiggestCatch = same rank
        var rank = 1;
        var dtos = new List<CompetitionResultDto>(rawResults.Count);

        for (int i = 0; i < rawResults.Count; i++)
        {
            if (i > 0
                && rawResults[i].Points == rawResults[i - 1].Points
                && (rawResults[i].BiggestCatchWeight ?? 0) == (rawResults[i - 1].BiggestCatchWeight ?? 0))
            {
                // Tie — keep same rank
            }
            else
            {
                rank = i + 1;
            }

            var r = rawResults[i];
            var isBiggestCatch = maxBiggestCatchWeight > 0 && r.BiggestCatchWeight.HasValue && r.BiggestCatchWeight.Value == maxBiggestCatchWeight;

            dtos.Add(new CompetitionResultDto(
                r.Id,
                r.CompetitionId,
                r.FishermanId,
                r.AssignedSpotNumber,
                r.DidAttend,
                r.WeightInGrams,
                r.BiggestCatchWeight,
                r.Points,
                rank,
                r.IsValidated,
                r.RegistrationDate,
                isBiggestCatch));
        }

        return Task.FromResult<ErrorOr<CompetitionResultDto[]>>(dtos.ToArray());
    }
}
