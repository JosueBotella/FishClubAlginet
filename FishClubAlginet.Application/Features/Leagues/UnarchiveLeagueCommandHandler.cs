using FishClubAlginet.Contracts.Dtos.Responses.League;

namespace FishClubAlginet.Application.Features.Leagues;

public record UnarchiveLeagueCommand(Guid Id) : IRequest<ErrorOr<LeagueDto>>;

public sealed class UnarchiveLeagueCommandHandler
    : IRequestHandler<UnarchiveLeagueCommand, ErrorOr<LeagueDto>>
{
    private readonly IGenericRepository<League, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnarchiveLeagueCommandHandler> _logger;

    public UnarchiveLeagueCommandHandler(
        IGenericRepository<League, Guid> repository,
        IUnitOfWork unitOfWork,
        ILogger<UnarchiveLeagueCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<LeagueDto>> Handle(
        UnarchiveLeagueCommand request,
        CancellationToken cancellationToken)
    {
        var league = _repository.GetAll()
            .FirstOrDefault(l => l.Id == request.Id && !l.IsDeleted);

        if (league is null)
        {
            _logger.LogWarning("League {LeagueId} not found for unarchiving", request.Id);
            return Errors.League.NotFound;
        }

        if (!league.IsArchived)
        {
            _logger.LogWarning("League {LeagueId} is not archived", request.Id);
            return Errors.LeagueErrors.NotArchived;
        }

        league.Unarchive();
        _repository.Update(league);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsError)
        {
            _logger.LogError(
                "Error unarchiving league {LeagueId}: {Errors}",
                request.Id,
                string.Join(", ", saveResult.Errors.Select(e => e.Description)));
            return saveResult.Errors;
        }

        _logger.LogInformation("League {LeagueId} ({LeagueYear}) unarchived successfully",
            league.Id, league.Year);

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

public class UnarchiveLeagueCommandValidator : AbstractValidator<UnarchiveLeagueCommand>
{
    public UnarchiveLeagueCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("League.Id.Required")
            .WithMessage("LeagueId is required.");
    }
}
