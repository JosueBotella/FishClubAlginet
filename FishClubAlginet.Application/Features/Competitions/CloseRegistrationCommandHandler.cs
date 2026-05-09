namespace FishClubAlginet.Application.Features.Competitions;

public record CloseRegistrationCommand(Guid CompetitionId) : IRequest<ErrorOr<Success>>;

public sealed class CloseRegistrationCommandHandler
    : IRequestHandler<CloseRegistrationCommand, ErrorOr<Success>>
{
    private readonly IGenericRepository<Competition, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseRegistrationCommandHandler(
        IGenericRepository<Competition, Guid> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        CloseRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var competition = await _repository.GetById(request.CompetitionId);
        if (competition is null || competition.IsDeleted)
            return Errors.Competition.NotFound;

        if (competition.Status != CompetitionStatus.RegistrationOpen)
            return Errors.Competition.InvalidStatusTransition;

        competition.Status = CompetitionStatus.Closed;
        competition.LastUpdateUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
