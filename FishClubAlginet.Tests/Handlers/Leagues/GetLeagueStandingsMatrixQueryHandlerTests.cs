using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;
using FishClubAlginet.Application.Abstractions;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class GetLeagueStandingsMatrixQueryHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockLeagueRepo;
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompetitionRepo;
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockFishermanRepo;
    private readonly GetLeagueStandingsMatrixQueryHandler _handler;

    public GetLeagueStandingsMatrixQueryHandlerTests()
    {
        _mockLeagueRepo = new Mock<IGenericRepository<League, Guid>>();
        _mockCompetitionRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockResultRepo = new Mock<IGenericRepository<CompetitionResult, Guid>>();
        _mockFishermanRepo = new Mock<IGenericRepository<Fisherman, int>>();

        _handler = new GetLeagueStandingsMatrixQueryHandler(
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

        var query = new GetLeagueStandingsMatrixQuery(leagueId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
    }

    [Fact]
    public async Task Handle_CalculatesCorrectMatrixWithWorstResultsDiscarded()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var league = League.Create("Liga 2026", 2026, minPoints: 5, worstResultsToDiscard: 1);
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

        var comp2 = new Competition
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            CompetitionNumber = 2,
            Name = "Concurso 2",
            Date = DateTime.UtcNow.AddDays(-5),
            Status = CompetitionStatus.ResultsValidated
        };

        var fishermanId = 12;
        var fisherman = new Fisherman
        {
            Id = fishermanId,
            FirstName = "Juan",
            LastName = "Alcaraz"
        };

        var res1 = new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = comp1.Id,
            FishermanId = fishermanId,
            DidAttend = true,
            WeightInGrams = 1500,
            Points = 20m,
            Ranking = 1
        };

        var res2 = new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = comp2.Id,
            FishermanId = fishermanId,
            DidAttend = true,
            WeightInGrams = 2200,
            Points = 15m, // lowest score, should be discarded
            Ranking = 2
        };

        _mockLeagueRepo.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { comp1, comp2 }.AsQueryable());

        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { res1, res2 }.AsQueryable());

        _mockFishermanRepo.Setup(r => r.GetAll())
            .Returns(new List<Fisherman> { fisherman }.AsQueryable());

        var query = new GetLeagueStandingsMatrixQuery(leagueId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        var dto = result.Value;
        dto.LeagueId.Should().Be(leagueId);
        dto.WorstResultsToDiscard.Should().Be(1);
        dto.Competitions.Should().HaveCount(2);
        dto.Competitions[0].Id.Should().Be(comp1.Id);
        dto.Competitions[1].Id.Should().Be(comp2.Id);

        dto.ByPoints.Should().HaveCount(1);
        var row = dto.ByPoints[0];
        row.FishermanId.Should().Be(fishermanId);
        row.FullName.Should().Be("Juan Alcaraz");
        row.TotalWeightGrams.Should().Be(3700); // 1500 + 2200
        row.TotalPoints.Should().Be(35m); // 20 + 15
        row.PointsAfterDiscard.Should().Be(20m); // 15 points should be discarded
        row.CompetitionsAttended.Should().Be(2);

        row.ResultsPerCompetition.Should().ContainKey(comp1.Id);
        row.ResultsPerCompetition.Should().ContainKey(comp2.Id);

        // Cell 1 (Concurso 1)
        var cell1 = row.ResultsPerCompetition[comp1.Id];
        cell1.WeightInGrams.Should().Be(1500);
        cell1.Points.Should().Be(20m);
        cell1.Ranking.Should().Be(1);
        cell1.DidAttend.Should().BeTrue();
        cell1.IsDiscarded.Should().BeFalse();

        // Cell 2 (Concurso 2)
        var cell2 = row.ResultsPerCompetition[comp2.Id];
        cell2.WeightInGrams.Should().Be(2200);
        cell2.Points.Should().Be(15m);
        cell2.Ranking.Should().Be(2);
        cell2.DidAttend.Should().BeTrue();
        cell2.IsDiscarded.Should().BeTrue(); // Confirmed discarded
    }

    [Fact]
    public async Task Handle_AbsenceAndNoRegistration_ShouldRenderCorrectAbsentCells()
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

        var fishermanId = 5;
        var fisherman = new Fisherman
        {
            Id = fishermanId,
            FirstName = "Pedro",
            LastName = "Gomez"
        };

        // Pedro did not attend comp 1
        var res1 = new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = comp1.Id,
            FishermanId = fishermanId,
            DidAttend = false,
            WeightInGrams = 0,
            Points = 0,
            Ranking = 0
        };

        _mockLeagueRepo.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { comp1 }.AsQueryable());

        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { res1 }.AsQueryable());

        _mockFishermanRepo.Setup(r => r.GetAll())
            .Returns(new List<Fisherman> { fisherman }.AsQueryable());

        var query = new GetLeagueStandingsMatrixQuery(leagueId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        var row = result.Value.ByPoints[0];
        row.CompetitionsAttended.Should().Be(0);
        row.TotalWeightGrams.Should().Be(0);
        row.TotalPoints.Should().Be(0);

        row.ResultsPerCompetition.Should().ContainKey(comp1.Id);
        var cell = row.ResultsPerCompetition[comp1.Id];
        cell.DidAttend.Should().BeFalse();
        cell.Points.Should().Be(0);
        cell.WeightInGrams.Should().Be(0);
        cell.IsDiscarded.Should().BeFalse();
    }
}
