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

namespace FishClubAlginet.Application.Features.Competitions;

public record GetCompetitionBiggestCatchQuery(Guid CompetitionId)
    : IRequest<ErrorOr<CompetitionBiggestCatchDto>>;

public sealed class GetCompetitionBiggestCatchQueryHandler
    : IRequestHandler<GetCompetitionBiggestCatchQuery, ErrorOr<CompetitionBiggestCatchDto>>
{
    private readonly IGenericRepository<Competition, Guid> _competitionRepository;
    private readonly IGenericRepository<CompetitionResult, Guid> _resultRepository;
    private readonly IGenericRepository<Fisherman, int> _fishermanRepository;

    public GetCompetitionBiggestCatchQueryHandler(
        IGenericRepository<Competition, Guid> competitionRepository,
        IGenericRepository<CompetitionResult, Guid> resultRepository,
        IGenericRepository<Fisherman, int> fishermanRepository)
    {
        _competitionRepository = competitionRepository;
        _resultRepository = resultRepository;
        _fishermanRepository = fishermanRepository;
    }

    public Task<ErrorOr<CompetitionBiggestCatchDto>> Handle(
        GetCompetitionBiggestCatchQuery request,
        CancellationToken cancellationToken)
    {
        var competition = _competitionRepository.GetAll()
            .FirstOrDefault(c => c.Id == request.CompetitionId && !c.IsDeleted);

        if (competition is null)
        {
            return Task.FromResult<ErrorOr<CompetitionBiggestCatchDto>>(Errors.Competition.NotFound);
        }

        // 1. Get all attended results with a valid biggest catch weight
        var results = _resultRepository.GetAll()
            .Where(r => r.CompetitionId == request.CompetitionId && r.DidAttend && r.BiggestCatchWeight.HasValue && r.BiggestCatchWeight.Value > 0 && !r.IsDeleted)
            .ToList();

        if (!results.Any())
        {
            return Task.FromResult<ErrorOr<CompetitionBiggestCatchDto>>(Errors.Competition.NoCatchesRecorded);
        }

        // 2. Filter results that meet the minimum weight limit if configured
        var minWeight = competition.BiggestCatchMinWeightInGrams ?? 0;
        var validResults = results
            .Where(r => (r.BiggestCatchWeight ?? 0) >= minWeight)
            .OrderByDescending(r => r.BiggestCatchWeight ?? 0)
            .ThenBy(r => r.RegistrationDate)
            .ToList();

        var winningResult = validResults.FirstOrDefault();

        if (winningResult is null)
        {
            return Task.FromResult<ErrorOr<CompetitionBiggestCatchDto>>(Errors.Competition.NoCatchesRecorded);
        }

        // 3. Resolve fisherman details
        var fisherman = _fishermanRepository.GetAll()
            .FirstOrDefault(f => f.Id == winningResult.FishermanId);

        var fishermanName = fisherman != null ? $"{fisherman.FirstName} {fisherman.LastName}" : $"Fisherman #{winningResult.FishermanId}";

        var dto = new CompetitionBiggestCatchDto(
            competition.Id,
            competition.Name ?? string.Empty,
            winningResult.FishermanId,
            fishermanName,
            winningResult.BiggestCatchWeight ?? 0);

        return Task.FromResult<ErrorOr<CompetitionBiggestCatchDto>>(dto);
    }
}
