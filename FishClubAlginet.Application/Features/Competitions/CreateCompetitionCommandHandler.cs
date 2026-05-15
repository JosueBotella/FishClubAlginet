namespace FishClubAlginet.Application.Features.Competitions;

public record CreateCompetitionCommand(
    Guid LeagueId,
    int CompetitionNumber,
    string? Name,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Venue,
    string Zone,
    Subspecialty Subspecialty,
    Category Category,
    int MaxSpots) : IRequest<ErrorOr<Guid>>;

public sealed class CreateCompetitionCommandHandler
    : IRequestHandler<CreateCompetitionCommand, ErrorOr<Guid>>
{
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IGenericRepository<League, Guid> _leagueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCompetitionCommandHandler> _logger;

    public CreateCompetitionCommandHandler(
        IGenericRepository<Competition, Guid> competitionRepository,
        IGenericRepository<League, Guid> leagueRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateCompetitionCommandHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _leagueRepository = leagueRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        var league = await _leagueRepository.GetById(request.LeagueId);
        if (league is null || league.IsDeleted)
        {
            return Errors.League.NotFound;
        }

        if (league.IsArchived)
        {
            return Errors.League.CannotModifyArchived;
        }

        if (!league.IsActive)
        {
            return Errors.League.NotActive;
        }

        var duplicateNumber = _competitionRepository
            .GetAll()
            .Any(c => c.LeagueId == request.LeagueId
                   && c.CompetitionNumber == request.CompetitionNumber
                   && !c.IsDeleted);

        if (duplicateNumber)
        {
            return Errors.Competition.DuplicateNumber;
        }

        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            LeagueId = request.LeagueId,
            CompetitionNumber = request.CompetitionNumber,
            Name = request.Name ?? string.Empty,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Venue = request.Venue,
            Zone = request.Zone,
            Subspecialty = request.Subspecialty,
            Category = request.Category,
            Status = CompetitionStatus.Planned,
            MaxSpots = request.MaxSpots,
            ParticipantCount = 0,
            LastUpdateUtc = DateTime.UtcNow
        };

        await _competitionRepository.AddAsync(competition);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error creating competition: {Errors}",
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));

            if (saveResult.FirstError.Code == "Database.UniqueConstraintViolation")
            {
                return Errors.Competition.DuplicateNumber;
            }

            return Error.Failure(
                code: "COMPETITION_SAVE_FAILED",
                description: "Could not create the competition. Please try again.");
        }

        _logger.LogInformation("Competition created successfully with ID: {CompetitionId}", competition.Id);
        return competition.Id;
    }
}

public class CreateCompetitionCommandValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionCommandValidator()
    {
        RuleFor(x => x.LeagueId)
            .NotEmpty()
            .WithErrorCode("Competition.LeagueId.Required")
            .WithMessage("LeagueId is required.");

        RuleFor(x => x.CompetitionNumber)
            .GreaterThan(0)
            .WithErrorCode("Competition.CompetitionNumber.Range")
            .WithMessage("CompetitionNumber must be greater than 0.");

        RuleFor(x => x.Date)
            .GreaterThan(DateTime.UtcNow.Date)
            .WithErrorCode("Competition.Date.Future")
            .WithMessage("Competition date must be in the future.");

        RuleFor(x => x.Venue)
            .NotEmpty()
            .WithErrorCode("Competition.Venue.Required")
            .WithMessage("Venue is required.")
            .MaximumLength(100)
            .WithErrorCode("Competition.Venue.MaxLength")
            .WithMessage("Venue must not exceed 100 characters.");

        RuleFor(x => x.Zone)
            .NotEmpty()
            .WithErrorCode("Competition.Zone.Required")
            .WithMessage("Zone is required.")
            .MaximumLength(50)
            .WithErrorCode("Competition.Zone.MaxLength")
            .WithMessage("Zone must not exceed 50 characters.");

        RuleFor(x => x.MaxSpots)
            .GreaterThan(0)
            .WithErrorCode("Competition.MaxSpots.Range")
            .WithMessage("MaxSpots must be greater than 0.");

        RuleFor(x => x.Subspecialty)
            .IsInEnum()
            .WithErrorCode("Competition.Subspecialty.Invalid")
            .WithMessage("Subspecialty value is not valid.");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithErrorCode("Competition.Category.Invalid")
            .WithMessage("Category value is not valid.");
    }
}
