namespace FishClubAlginet.Application.Features.Competitions;

public record UpdateBiggestCatchConfigCommand(Guid CompetitionId, int? MinWeightInGrams)
    : IRequest<ErrorOr<Success>>;

public sealed class UpdateBiggestCatchConfigCommandHandler
    : IRequestHandler<UpdateBiggestCatchConfigCommand, ErrorOr<Success>>
{
    private readonly IGenericRepository<Competition, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBiggestCatchConfigCommandHandler(
        IGenericRepository<Competition, Guid> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateBiggestCatchConfigCommand request,
        CancellationToken cancellationToken)
    {
        var competition = await _repository.GetById(request.CompetitionId);
        if (competition is null || competition.IsDeleted)
            return Errors.Competition.NotFound;

        competition.SetBiggestCatchMinWeight(request.MinWeightInGrams);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
