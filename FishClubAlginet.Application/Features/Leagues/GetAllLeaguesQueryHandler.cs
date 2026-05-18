using FishClubAlginet.Contracts.Dtos.Common;

namespace FishClubAlginet.Application.Features.Leagues;

public record GetAllLeaguesQueryResponse(
    Guid Id,
    string Name,
    int Year,
    bool IsActive,
    bool IsArchived,
    int MinPoints,
    int WorstResultsToDiscard,
    int CompetitionsCount,
    DateTime LastUpdateUtc);

public record GetAllLeaguesQuery(
    int Skip,
    int Take,
    int? Year = null,
    bool? Archived = null   // null = solo no archivadas, true = solo archivadas, false explícito = solo no archivadas
) : IRequest<ErrorOr<PaginatedResult<GetAllLeaguesQueryResponse>>>;

public class GetAllLeaguesQueryHandler
    : IRequestHandler<GetAllLeaguesQuery, ErrorOr<PaginatedResult<GetAllLeaguesQueryResponse>>>
{
    private readonly IGenericRepository<League, Guid> _repository;

    public GetAllLeaguesQueryHandler(IGenericRepository<League, Guid> repository)
    {
        _repository = repository;
    }

    public Task<ErrorOr<PaginatedResult<GetAllLeaguesQueryResponse>>> Handle(
        GetAllLeaguesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _repository.GetAll()
            .Where(l => !l.IsDeleted);

        // Por defecto (null) y false → excluye archivadas. true → solo archivadas.
        if (request.Archived == true)
            query = query.Where(l => l.IsArchived);
        else
            query = query.Where(l => !l.IsArchived);

        if (request.Year.HasValue)
        {
            query = query.Where(l => l.Year == request.Year.Value);
        }

        var totalCount = query.Count();

        var items = query
            .OrderByDescending(l => l.Year)
            .ThenBy(l => l.Name)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(l => new GetAllLeaguesQueryResponse(
                l.Id,
                l.Name,
                l.Year,
                l.IsActive,
                l.IsArchived,
                l.MinPoints,
                l.WorstResultsToDiscard,
                l.Competitions.Count,
                l.LastUpdateUtc))
            .ToList();

        var result = new PaginatedResult<GetAllLeaguesQueryResponse>(items, totalCount);
        return Task.FromResult<ErrorOr<PaginatedResult<GetAllLeaguesQueryResponse>>>(result);
    }
}
