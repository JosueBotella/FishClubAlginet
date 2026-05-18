namespace FishClubAlginet.Application.Features.Competitions;

public record GetCompetitionByIdQuery(Guid CompetitionId)
    : IRequest<ErrorOr<CompetitionDto>>;

public sealed class GetCompetitionByIdQueryHandler
    : IRequestHandler<GetCompetitionByIdQuery, ErrorOr<CompetitionDto>>
{
    private readonly IGenericRepository<Competition, Guid> _repository;

    public GetCompetitionByIdQueryHandler(IGenericRepository<Competition, Guid> repository)
    {
        _repository = repository;
    }

    public Task<ErrorOr<CompetitionDto>> Handle(
        GetCompetitionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var dto = _repository.GetAll()
            .Where(c => c.Id == request.CompetitionId && !c.IsDeleted)
            .Select(c => new CompetitionDto(
                c.Id,
                c.LeagueId,
                c.CompetitionNumber,
                c.Name,
                c.Date,
                c.StartTime,
                c.EndTime,
                c.Venue,
                c.Zone,
                c.Subspecialty.ToString(),
                c.Category.ToString(),
                c.Status.ToString(),
                c.MaxSpots,
                c.ParticipantCount,
                c.LastUpdateUtc))
            .FirstOrDefault();

        if (dto is null)
            return Task.FromResult<ErrorOr<CompetitionDto>>(Errors.Competition.NotFound);

        return Task.FromResult<ErrorOr<CompetitionDto>>(dto);
    }
}
