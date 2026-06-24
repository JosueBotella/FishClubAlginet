using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FishClubAlginet.Application.Abstractions;
using FishClubAlginet.Application.Features.Leagues;
using FishClubAlginet.Core.Domain.Entities;
using Moq;
using Xunit;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class GetSeasonBiggestCatchQueryHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockLeagueRepo;
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompetitionRepo;
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockFishermanRepo;
    private readonly GetSeasonBiggestCatchQueryHandler _handler;

    public GetSeasonBiggestCatchQueryHandlerTests()
    {
        _mockLeagueRepo = new Mock<IGenericRepository<League, Guid>>();
        _mockCompetitionRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockResultRepo = new Mock<IGenericRepository<CompetitionResult, Guid>>();
        _mockFishermanRepo = new Mock<IGenericRepository<Fisherman, int>>();

        _handler = new GetSeasonBiggestCatchQueryHandler(
            _mockLeagueRepo.Object,
            _mockCompetitionRepo.Object,
            _mockResultRepo.Object,
            _mockFishermanRepo.Object);
    }

    [Fact]
    public async Task Handle_LeagueNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        _mockLeagueRepo.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());

        var query = new GetSeasonBiggestCatchQuery(leagueId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
    }

    [Fact]
    public async Task Handle_NoValidCatchesInLeague_ShouldReturnNoCatchesRecordedError()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var league = League.Create("Liga 2026", 2026);
        league.Id = leagueId;
        league.IsActive = true;

        var comp1 = new Competition
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            CompetitionNumber = 1,
            Name = "Concurso 1",
            Date = DateTime.UtcNow.AddDays(-10),
            Status = CompetitionStatus.ResultsValidated
        };

        _mockLeagueRepo.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { comp1 }.AsQueryable());

        // Attendee has didAttend = true, but BiggestCatchWeight = null (no catches registered)
        var res1 = new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = comp1.Id,
            FishermanId = 1,
            DidAttend = true,
            WeightInGrams = 1000,
            BiggestCatchWeight = null,
            Points = 20,
            Ranking = 1
        };

        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { res1 }.AsQueryable());

        var query = new GetSeasonBiggestCatchQuery(leagueId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NoCatchesRecorded");
    }

    [Fact]
    public async Task Handle_MultipleCompetitions_ShouldIdentifyAbsoluteMaxBiggestCatchRespectingMinWeights()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var league = League.Create("Liga 2026", 2026);
        league.Id = leagueId;
        league.IsActive = true;

        var comp1 = new Competition
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            CompetitionNumber = 1,
            Name = "Concurso 1",
            Date = DateTime.UtcNow.AddDays(-10),
            Status = CompetitionStatus.ResultsValidated,
            BiggestCatchMinWeightInGrams = 2000 // Min weight is 2kg
        };

        var comp2 = new Competition
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            CompetitionNumber = 2,
            Name = "Concurso 2",
            Date = DateTime.UtcNow.AddDays(-5),
            Status = CompetitionStatus.ResultsValidated,
            BiggestCatchMinWeightInGrams = null // No min weight
        };

        var f1 = new Fisherman { Id = 1, FirstName = "Juan", LastName = "Gomez" };
        var f2 = new Fisherman { Id = 2, FirstName = "Pedro", LastName = "Ruiz" };

        // Catch in comp1 has weight 1500 (fails to meet the 2kg minimum)
        var res1 = new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = comp1.Id,
            FishermanId = 1,
            DidAttend = true,
            WeightInGrams = 3000,
            BiggestCatchWeight = 1500,
            Points = 20,
            Ranking = 1,
            RegistrationDate = DateTime.UtcNow.AddDays(-10)
        };

        // Catch in comp2 has weight 1800 (valid, as there is no minimum configured for comp2)
        var res2 = new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = comp2.Id,
            FishermanId = 2,
            DidAttend = true,
            WeightInGrams = 2500,
            BiggestCatchWeight = 1800,
            Points = 20,
            Ranking = 1,
            RegistrationDate = DateTime.UtcNow.AddDays(-5)
        };

        _mockLeagueRepo.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { comp1, comp2 }.AsQueryable());

        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { res1, res2 }.AsQueryable());

        _mockFishermanRepo.Setup(r => r.GetAll())
            .Returns(new List<Fisherman> { f1, f2 }.AsQueryable());

        var query = new GetSeasonBiggestCatchQuery(leagueId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        var dto = result.Value;
        dto.LeagueId.Should().Be(leagueId);
        dto.FishermanId.Should().Be(2); // Pedro Ruiz won because Juan's catch did not meet minWeight
        dto.FishermanName.Should().Be("Pedro Ruiz");
        dto.WeightInGrams.Should().Be(1800);
        dto.CompetitionId.Should().Be(comp2.Id);
        dto.CompetitionName.Should().Be("Concurso 2");
    }
}
