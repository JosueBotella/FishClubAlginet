using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FishClubAlginet.Application.Abstractions;
using FishClubAlginet.Contracts.Dtos.Responses.Competition;
using FishClubAlginet.Core.Domain.Common.Errors;
using FishClubAlginet.Core.Domain.Entities;
using MediatR;
using ErrorOr;

namespace FishClubAlginet.Application.Features.Leagues;

public record GetSeasonBiggestCatchQuery(Guid LeagueId)
    : IRequest<ErrorOr<SeasonBiggestCatchDto>>;

public sealed class GetSeasonBiggestCatchQueryHandler
    : IRequestHandler<GetSeasonBiggestCatchQuery, ErrorOr<SeasonBiggestCatchDto>>
{
    private readonly IGenericRepository<League, Guid> _leagueRepository;
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IGenericRepository<Fisherman, int> _fishermanRepository;

    public GetSeasonBiggestCatchQueryHandler(
        IGenericRepository<League, Guid> leagueRepository,
        IGenericRepository<Competition, Guid> competitionRepository,
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IGenericRepository<Fisherman, int> fishermanRepository)
    {
        _leagueRepository = leagueRepository;
        _competitionRepository = competitionRepository;
        _resultRepository = resultRepository;
        _fishermanRepository = fishermanRepository;
    }

    public Task<ErrorOr<SeasonBiggestCatchDto>> Handle(
        GetSeasonBiggestCatchQuery request,
        CancellationToken cancellationToken)
    {
        var league = _leagueRepository.GetAll()
            .FirstOrDefault(l => l.Id == request.LeagueId && !l.IsDeleted);

        if (league is null)
        {
            return Task.FromResult<ErrorOr<SeasonBiggestCatchDto>>(Errors.League.NotFound);
        }

        // 1. Get all competitions for this league
        var competitions = _competitionRepository.GetAll()
            .Where(c => c.LeagueId == request.LeagueId && !c.IsDeleted)
            .ToList();

        if (!competitions.Any())
        {
            return Task.FromResult<ErrorOr<SeasonBiggestCatchDto>>(Errors.League.NoCatchesRecorded);
        }

        var compIds = competitions.Select(c => c.Id).ToList();

        // 2. Get all valid attended results with biggest catch weights
        var results = _resultRepository.GetAll()
            .Where(r => compIds.Contains(r.CompetitionId) && r.DidAttend && r.BiggestCatchWeight.HasValue && r.BiggestCatchWeight.Value > 0 && !r.IsDeleted)
            .ToList();

        if (!results.Any())
        {
            return Task.FromResult<ErrorOr<SeasonBiggestCatchDto>>(Errors.League.NoCatchesRecorded);
        }

        // 3. Filter results that meet or exceed their competition's minimum weight requirement (if configured)
        var validResults = results
            .Where(r =>
            {
                var comp = competitions.First(c => c.Id == r.CompetitionId);
                var minWeight = comp.BiggestCatchMinWeightInGrams ?? 0;
                return (r.BiggestCatchWeight ?? 0) >= minWeight;
            })
            .OrderByDescending(r => r.BiggestCatchWeight ?? 0)
            .ThenBy(r => r.RegistrationDate) // Tie breaker: first registered
            .ToList();

        var winningResult = validResults.FirstOrDefault();

        if (winningResult is null)
        {
            return Task.FromResult<ErrorOr<SeasonBiggestCatchDto>>(Errors.League.NoCatchesRecorded);
        }

        // 4. Resolve details for the winning catch
        var fisherman = _fishermanRepository.GetAll()
            .FirstOrDefault(f => f.Id == winningResult.FishermanId);

        var winningCompetition = competitions.First(c => c.Id == winningResult.CompetitionId);

        var fishermanName = fisherman != null ? $"{fisherman.FirstName} {fisherman.LastName}" : $"Fisherman #{winningResult.FishermanId}";

        var dto = new SeasonBiggestCatchDto(
            league.Id,
            league.Name,
            winningResult.FishermanId,
            fishermanName,
            winningResult.BiggestCatchWeight ?? 0,
            winningCompetition.Id,
            winningCompetition.Name ?? string.Empty,
            winningCompetition.CompetitionNumber,
            winningCompetition.Date);

        return Task.FromResult<ErrorOr<SeasonBiggestCatchDto>>(dto);
    }
}
