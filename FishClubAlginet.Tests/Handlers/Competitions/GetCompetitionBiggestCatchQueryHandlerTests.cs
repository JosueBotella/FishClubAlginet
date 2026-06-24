using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FishClubAlginet.Application.Abstractions;
using FishClubAlginet.Application.Features.Competitions;
using FishClubAlginet.Core.Domain.Entities;
using Moq;
using Xunit;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class GetCompetitionBiggestCatchQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompetitionRepo;
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockFishermanRepo;
    private readonly GetCompetitionBiggestCatchQueryHandler _handler;

    public GetCompetitionBiggestCatchQueryHandlerTests()
    {
        _mockCompetitionRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockResultRepo = new Mock<IGenericRepository<CompetitionResult, Guid>>();
        _mockFishermanRepo = new Mock<IGenericRepository<Fisherman, int>>();

        _handler = new GetCompetitionBiggestCatchQueryHandler(
            _mockCompetitionRepo.Object,
            _mockResultRepo.Object,
            _mockFishermanRepo.Object);
    }

    [Fact]
    public async Task Handle_CompetitionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition>().AsQueryable());

        var query = new GetCompetitionBiggestCatchQuery(competitionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.NotFound");
    }

    [Fact]
    public async Task Handle_NoValidCatchesInCompetition_ShouldReturnNoCatchesRecordedError()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var comp = new Competition
        {
            Id = competitionId,
            LeagueId = Guid.NewGuid(),
            CompetitionNumber = 1,
            Name = "Concurso 1",
            Date = DateTime.UtcNow,
            Status = CompetitionStatus.ResultsValidated
        };

        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { comp }.AsQueryable());

        // Attendee registered, but weight is 0 and BiggestCatchWeight is null
        var res1 = new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = competitionId,
            FishermanId = 1,
            DidAttend = true,
            WeightInGrams = 0,
            BiggestCatchWeight = null,
            Points = 0,
            Ranking = 1
        };

        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { res1 }.AsQueryable());

        var query = new GetCompetitionBiggestCatchQuery(competitionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.NoCatchesRecorded");
    }

    [Fact]
    public async Task Handle_CalculatesCorrectBiggestCatchRespectingCompetitionMinWeight()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var comp = new Competition
        {
            Id = competitionId,
            LeagueId = Guid.NewGuid(),
            CompetitionNumber = 1,
            Name = "Concurso 1",
            Date = DateTime.UtcNow,
            Status = CompetitionStatus.ResultsValidated,
            BiggestCatchMinWeightInGrams = 1500 // Min weight is 1.5kg
        };

        var f1 = new Fisherman { Id = 1, FirstName = "Toni", LastName = "Bello" };
        var f2 = new Fisherman { Id = 2, FirstName = "Carlos", LastName = "Algarra" };

        // Toni: catch 1400 (fails minimum limit)
        var res1 = new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = competitionId,
            FishermanId = 1,
            DidAttend = true,
            WeightInGrams = 2000,
            BiggestCatchWeight = 1400,
            Points = 15,
            Ranking = 2,
            RegistrationDate = DateTime.UtcNow.AddMinutes(-10)
        };

        // Carlos: catch 1600 (exceeds minimum limit)
        var res2 = new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = competitionId,
            FishermanId = 2,
            DidAttend = true,
            WeightInGrams = 3500,
            BiggestCatchWeight = 1600,
            Points = 20,
            Ranking = 1,
            RegistrationDate = DateTime.UtcNow.AddMinutes(-5)
        };

        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { comp }.AsQueryable());

        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { res1, res2 }.AsQueryable());

        _mockFishermanRepo.Setup(r => r.GetAll())
            .Returns(new List<Fisherman> { f1, f2 }.AsQueryable());

        var query = new GetCompetitionBiggestCatchQuery(competitionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        var dto = result.Value;
        dto.CompetitionId.Should().Be(competitionId);
        dto.FishermanId.Should().Be(2); // Carlos algarra won
        dto.FishermanName.Should().Be("Carlos Algarra");
        dto.WeightInGrams.Should().Be(1600);
    }
}
