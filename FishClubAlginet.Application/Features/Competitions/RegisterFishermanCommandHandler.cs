namespace FishClubAlginet.Application.Features.Competitions;

public record RegisterFishermanCommand(
    Guid CompetitionId,
    int FishermanId) : IRequest<ErrorOr<Guid>>;

public sealed class RegisterFishermanCommandHandler
    : IRequestHandler<RegisterFishermanCommand, ErrorOr<Guid>>
{
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IGenericRepository<Fisherman, int> _fishermanRepository;
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterFishermanCommandHandler> _logger;

    public RegisterFishermanCommandHandler(
        IGenericRepository<Competition, Guid> competitionRepository,
        IGenericRepository<Fisherman, int> fishermanRepository,
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegisterFishermanCommandHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _fishermanRepository = fishermanRepository;
        _resultRepository = resultRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(RegisterFishermanCommand request, CancellationToken cancellationToken)
    {
        var competition = await _competitionRepository.GetById(request.CompetitionId);
        if (competition is null || competition.IsDeleted)
        {
            return Errors.Competition.NotFound;
        }

        if (competition.Status != CompetitionStatus.RegistrationOpen)
        {
            return Errors.Competition.RegistrationNotOpen;
        }

        var fisherman = await _fishermanRepository.GetById(request.FishermanId);
        if (fisherman is null || fisherman.IsDeleted)
        {
            return Errors.FishermanErrors.NotFound;
        }

        var alreadyRegistered = _resultRepository
            .GetAll()
            .Any(r => r.CompetitionId == request.CompetitionId
                   && r.FishermanId == request.FishermanId);

        if (alreadyRegistered)
        {
            return Errors.Competition.AlreadyRegistered;
        }

        var result = CompetitionResult.Register(request.CompetitionId, request.FishermanId);

        await _resultRepository.AddAsync(result);

        competition.ParticipantCount++;
        competition.LastUpdateUtc = DateTime.UtcNow;

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error registering fisherman {FishermanId} to competition {CompetitionId}: {Errors}",
                request.FishermanId,
                request.CompetitionId,
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));

            if (saveResult.FirstError.Code == "Database.UniqueConstraintViolation")
            {
                return Errors.Competition.AlreadyRegistered;
            }

            return Error.Failure(
                code: "REGISTRATION_SAVE_FAILED",
                description: "Could not register the fisherman. Please try again.");
        }

        _logger.LogInformation(
            "Fisherman {FishermanId} registered to competition {CompetitionId} with result ID {ResultId}",
            request.FishermanId, request.CompetitionId, result.Id);

        return result.Id;
    }
}

public class RegisterFishermanCommandValidator : AbstractValidator<RegisterFishermanCommand>
{
    public RegisterFishermanCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty()
            .WithErrorCode("Competition.CompetitionId.Required")
            .WithMessage("CompetitionId is required.");

        RuleFor(x => x.FishermanId)
            .GreaterThan(0)
            .WithErrorCode("Competition.FishermanId.Range")
            .WithMessage("FishermanId must be greater than 0.");
    }
}
