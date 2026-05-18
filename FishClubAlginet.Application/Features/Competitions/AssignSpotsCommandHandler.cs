namespace FishClubAlginet.Application.Features.Competitions;

public record AssignSpotsCommand(Guid CompetitionId) : IRequest<ErrorOr<Success>>;

public sealed class AssignSpotsCommandHandler
    : IRequestHandler<AssignSpotsCommand, ErrorOr<Success>>
{
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignSpotsCommandHandler(
        IGenericRepository<Competition, Guid> competitionRepository,
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IUnitOfWork unitOfWork)
    {
        _competitionRepository = competitionRepository;
        _resultRepository = resultRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        AssignSpotsCommand request,
        CancellationToken cancellationToken)
    {
        var competition = await _competitionRepository.GetById(request.CompetitionId);
        if (competition is null || competition.IsDeleted)
            return Errors.Competition.NotFound;

        // Allow spot assignment when RegistrationOpen or Closed (admin may need to reassign)
        if (competition.Status != CompetitionStatus.RegistrationOpen
            && competition.Status != CompetitionStatus.Closed)
            return Errors.Competition.InvalidStatusTransition;

        var results = _resultRepository.GetAll()
            .Where(r => r.CompetitionId == request.CompetitionId && !r.IsDeleted)
            .OrderBy(r => r.RegistrationDate)
            .ToList();

        if (results.Count == 0)
            return Errors.Competition.NoResultsToAssign;

        // Sequential lottery: 1..N in registration order
        for (int i = 0; i < results.Count; i++)
        {
            results[i].AssignSpot(i + 1);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}

public class AssignSpotsCommandValidator : AbstractValidator<AssignSpotsCommand>
{
    public AssignSpotsCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty()
            .WithErrorCode("Competition.CompetitionId.Required")
            .WithMessage("CompetitionId is required.");
    }
}
