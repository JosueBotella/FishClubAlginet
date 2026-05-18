using FishClubAlginet.Core.Domain.Services;

namespace FishClubAlginet.Application.Features.Competitions;

public record MoveToResultsDraftCommand(Guid CompetitionId) : IRequest<ErrorOr<Success>>;

public sealed class MoveToResultsDraftCommandHandler
    : IRequestHandler<MoveToResultsDraftCommand, ErrorOr<Success>>
{
    private readonly IGenericRepository<Competition, Guid> _repository;
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IGenericRepository<League, Guid> _leagueRepository;
    private readonly IPointsCalculator _pointsCalculator;
    private readonly IUnitOfWork _unitOfWork;

    public MoveToResultsDraftCommandHandler(
        IGenericRepository<Competition, Guid> repository,
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IGenericRepository<League, Guid> leagueRepository,
        IPointsCalculator pointsCalculator,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _resultRepository = resultRepository;
        _leagueRepository = leagueRepository;
        _pointsCalculator = pointsCalculator;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        MoveToResultsDraftCommand request,
        CancellationToken cancellationToken)
    {
        var competition = await _repository.GetById(request.CompetitionId);
        if (competition is null || competition.IsDeleted)
            return Errors.Competition.NotFound;

        if (competition.Status != CompetitionStatus.Closed)
            return Errors.Competition.NotInClosed;

        var league = await _leagueRepository.GetById(competition.LeagueId);
        var minPoints = league?.MinPoints ?? 5;

        var results = _resultRepository
            .GetAll()
            .Where(r => r.CompetitionId == request.CompetitionId && !r.IsDeleted)
            .ToList();

        _pointsCalculator.CalculateAndAssign(results, minPoints);

        competition.MoveToResultsDraft();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}

public class MoveToResultsDraftCommandValidator : AbstractValidator<MoveToResultsDraftCommand>
{
    public MoveToResultsDraftCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty()
            .WithErrorCode("Competition.CompetitionId.Required")
            .WithMessage("CompetitionId is required.");
    }
}
