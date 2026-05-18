namespace FishClubAlginet.Application.Features.Competitions;

public record UpdateCompetitionResultCommand(
    Guid ResultId,
    bool DidAttend,
    int WeightInGrams,
    int? BiggestCatchWeight) : IRequest<ErrorOr<Success>>;

public sealed class UpdateCompetitionResultCommandHandler
    : IRequestHandler<UpdateCompetitionResultCommand, ErrorOr<Success>>
{
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCompetitionResultCommandHandler(
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IGenericRepository<Competition, Guid> competitionRepository,
        IUnitOfWork unitOfWork)
    {
        _resultRepository = resultRepository;
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateCompetitionResultCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _resultRepository.GetById(request.ResultId);
        if (result is null || result.IsDeleted)
            return Errors.Competition.NotFound;

        var competition = await _competitionRepository.GetById(result.CompetitionId);
        if (competition is null || competition.IsDeleted)
            return Errors.Competition.NotFound;

        // Raw result recorded here; Points/Ranking are calculated later by
        // IPointsCalculator when the competition moves to ResultsDraft.
        result.RecordResult(request.DidAttend, request.WeightInGrams, request.BiggestCatchWeight);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
