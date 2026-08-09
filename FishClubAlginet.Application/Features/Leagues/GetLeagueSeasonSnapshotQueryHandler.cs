using FishClubAlginet.Contracts.Dtos.Responses.League;

namespace FishClubAlginet.Application.Features.Leagues;

public record GetLeagueSeasonSnapshotQuery(Guid LeagueId)
    : IRequest<ErrorOr<LeagueSeasonSnapshotDto>>;

public sealed class GetLeagueSeasonSnapshotQueryHandler
    : IRequestHandler<GetLeagueSeasonSnapshotQuery, ErrorOr<LeagueSeasonSnapshotDto>>
{
    private readonly IGenericRepository<LeagueSeasonSnapshot, Guid> _snapshotRepository;

    public GetLeagueSeasonSnapshotQueryHandler(
        IGenericRepository<LeagueSeasonSnapshot, Guid> snapshotRepository)
    {
        _snapshotRepository = snapshotRepository;
    }

    public Task<ErrorOr<LeagueSeasonSnapshotDto>> Handle(
        GetLeagueSeasonSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = _snapshotRepository.GetAll()
            .FirstOrDefault(s => s.LeagueId == request.LeagueId && !s.IsDeleted);

        if (snapshot is null)
        {
            return Task.FromResult<ErrorOr<LeagueSeasonSnapshotDto>>(Errors.Snapshot.NotFound);
        }

        var dto = new LeagueSeasonSnapshotDto(
            snapshot.Id,
            snapshot.LeagueId,
            snapshot.Year,
            snapshot.ArchivedAtUtc,
            snapshot.SnapshotDataJson);

        return Task.FromResult<ErrorOr<LeagueSeasonSnapshotDto>>(dto);
    }
}
