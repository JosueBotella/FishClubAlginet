namespace FishClubAlginet.Application.Features.Leagues;

public record CreateLeagueCommand(
    string Name,
    int Year,
    int MinPoints,
    int WorstResultsToDiscard) : IRequest<ErrorOr<Guid>>;

public sealed class CreateLeagueCommandHandler
    : IRequestHandler<CreateLeagueCommand, ErrorOr<Guid>>
{
    private readonly IGenericRepository<League, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateLeagueCommandHandler> _logger;

    public CreateLeagueCommandHandler(
        IGenericRepository<League, Guid> repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateLeagueCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateLeagueCommand request, CancellationToken cancellationToken)
    {
        var duplicateExists = _repository.GetAll().Any(l => l.Year == request.Year && !l.IsDeleted);
        if (duplicateExists)
        {
            _logger.LogWarning("Attempted to create duplicate league for year {Year}", request.Year);
            return Errors.League.DuplicateYear;
        }

        var league = League.Create(
            request.Name,
            request.Year,
            request.MinPoints,
            request.WorstResultsToDiscard);

        await _repository.AddAsync(league);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error creating league: {Errors}",
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));

            if (saveResult.FirstError.Code == "Database.UniqueConstraintViolation")
            {
                return Errors.League.DuplicateYear;
            }

            return Error.Failure(
                code: "LEAGUE_SAVE_FAILED",
                description: "Could not create the league. Please try again.");
        }

        _logger.LogInformation("League created successfully with ID: {LeagueId}", league.Id);
        return league.Id;
    }
}

public class CreateLeagueCommandValidator : AbstractValidator<CreateLeagueCommand>
{
    public CreateLeagueCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("League.Name.Required")
            .WithMessage("League name is required.")
            .MaximumLength(LeagueConstraints.NameMaxLength)
            .WithErrorCode("League.Name.MaxLength")
            .WithMessage($"League name must not exceed {LeagueConstraints.NameMaxLength} characters.");

        RuleFor(x => x.Year)
            .InclusiveBetween(LeagueConstraints.MinYear, DateTime.UtcNow.Year + 1)
            .WithErrorCode("League.Year.Range")
            .WithMessage($"Year must be between {LeagueConstraints.MinYear} and {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.MinPoints)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("League.MinPoints.Range")
            .WithMessage("MinPoints must be greater than or equal to 0.");

        RuleFor(x => x.WorstResultsToDiscard)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("League.WorstResultsToDiscard.Range")
            .WithMessage("WorstResultsToDiscard must be greater than or equal to 0.");
    }
}
