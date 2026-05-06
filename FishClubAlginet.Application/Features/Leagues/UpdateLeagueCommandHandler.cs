using FishClubAlginet.Contracts.Dtos.Responses.League;

namespace FishClubAlginet.Application.Features.Leagues;

public record UpdateLeagueCommand(
    Guid Id,
    string Name,
    int MinPoints,
    int WorstResultsToDiscard) : IRequest<ErrorOr<LeagueDto>>;

public sealed class UpdateLeagueCommandHandler
    : IRequestHandler<UpdateLeagueCommand, ErrorOr<LeagueDto>>
{
    private readonly IGenericRepository<League, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateLeagueCommandHandler> _logger;

    public UpdateLeagueCommandHandler(
        IGenericRepository<League, Guid> repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateLeagueCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<LeagueDto>> Handle(UpdateLeagueCommand request, CancellationToken cancellationToken)
    {
        var league = _repository.GetAll()
            .FirstOrDefault(l => l.Id == request.Id && !l.IsDeleted);

        if (league is null)
        {
            _logger.LogWarning("League {LeagueId} not found for update", request.Id);
            return Errors.League.NotFound;
        }

        if (league.IsArchived)
        {
            _logger.LogWarning("Cannot update archived league {LeagueId}", request.Id);
            return Errors.League.CannotModifyArchived;
        }

        league.Update(request.Name, request.MinPoints, request.WorstResultsToDiscard);
        _repository.Update(league);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error updating league {LeagueId}: {Errors}",
                request.Id,
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));
            return saveResult.Errors;
        }

        _logger.LogInformation("League {LeagueId} updated successfully", league.Id);
        return MapToDto(league);
    }

    private static LeagueDto MapToDto(League league)
    {
        return new LeagueDto(
            league.Id,
            league.Name,
            league.Year,
            league.IsActive,
            league.IsArchived,
            league.MinPoints,
            league.WorstResultsToDiscard,
            league.Competitions.Count,
            league.LastUpdateUtc);
    }
}

public class UpdateLeagueCommandValidator : AbstractValidator<UpdateLeagueCommand>
{
    public UpdateLeagueCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("League.Id.Required")
            .WithMessage("League ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("League.Name.Required")
            .WithMessage("League name is required.")
            .MaximumLength(LeagueConstraints.NameMaxLength)
            .WithErrorCode("League.Name.MaxLength")
            .WithMessage($"League name must not exceed {LeagueConstraints.NameMaxLength} characters.");

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
