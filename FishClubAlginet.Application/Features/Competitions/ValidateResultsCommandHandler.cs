namespace FishClubAlginet.Application.Features.Competitions;

public record ValidateResultsCommand(Guid CompetitionId) : IRequest<ErrorOr<Success>>;

public sealed class ValidateResultsCommandHandler
    : IRequestHandler<ValidateResultsCommand, ErrorOr<Success>>
{
    private readonly IGenericRepository<Competition, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ValidateResultsCommandHandler(
        IGenericRepository<Competition, Guid> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        ValidateResultsCommand request,
        CancellationToken cancellationToken)
    {
        var competition = await _repository.GetById(request.CompetitionId);
        if (competition is null || competition.IsDeleted)
            return Errors.Competition.NotFound;

        if (competition.Status == CompetitionStatus.ResultsValidated)
            return Errors.Competition.AlreadyValidated;

        if (competition.Status != CompetitionStatus.ResultsDraft)
            return Errors.Competition.NotInResultsDraft;

        competition.ValidateResults();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}

public class ValidateResultsCommandValidator : AbstractValidator<ValidateResultsCommand>
{
    public ValidateResultsCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty()
            .WithErrorCode("Competition.CompetitionId.Required")
            .WithMessage("CompetitionId is required.");
    }
}
