using FishClubAlginet.Contracts.Dtos.Responses.League;

namespace FishClubAlginet.Application.Features.Leagues;

public record GetLeagueByIdQuery(Guid Id) : IRequest<ErrorOr<LeagueDto>>;

public class GetLeagueByIdQueryHandler
    : IRequestHandler<GetLeagueByIdQuery, ErrorOr<LeagueDto>>
{
    private readonly IGenericRepository<League, Guid> _repository;

    public GetLeagueByIdQueryHandler(IGenericRepository<League, Guid> repository)
    {
        _repository = repository;
    }

    public Task<ErrorOr<LeagueDto>> Handle(GetLeagueByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = _repository.GetAll()
            .Where(l => l.Id == request.Id && !l.IsDeleted)
            .Select(l => new LeagueDto(
                l.Id,
                l.Name,
                l.Year,
                l.IsActive,
                l.IsArchived,
                l.MinPoints,
                l.WorstResultsToDiscard,
                l.Competitions.Count,
                l.LastUpdateUtc))
            .FirstOrDefault();

        if (dto is null)
        {
            return Task.FromResult<ErrorOr<LeagueDto>>(Errors.League.NotFound);
        }

        return Task.FromResult<ErrorOr<LeagueDto>>(dto);
    }
}
