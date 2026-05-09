namespace FishClubAlginet.Application.Features.Competitions;

public record RemoveRegistrationCommand(Guid ResultId) : IRequest<ErrorOr<Success>>;

public sealed class RemoveRegistrationCommandHandler
    : IRequestHandler<RemoveRegistrationCommand, ErrorOr<Success>>
{
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRegistrationCommandHandler(
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IGenericRepository<Competition, Guid> competitionRepository,
        IUnitOfWork unitOfWork)
    {
        _resultRepository = resultRepository;
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        RemoveRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _resultRepository.GetById(request.ResultId);
        if (result is null || result.IsDeleted)
            return Errors.Competition.NotFound;

        var competition = await _competitionRepository.GetById(result.CompetitionId);

        result.IsDeleted = true;
        result.DeletedTimeUtc = DateTime.UtcNow;
        result.LastUpdateUtc = DateTime.UtcNow;

        if (competition is not null && competition.ParticipantCount > 0)
        {
            competition.ParticipantCount--;
            competition.LastUpdateUtc = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
