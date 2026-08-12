using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class GetCompetitionResultsQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompRepo;
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly GetCompetitionResultsQueryHandler _handler;

    public GetCompetitionResultsQueryHandlerTests()
    {
        _mockCompRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockResultRepo = new Mock<IGenericRepository<CompetitionResult, Guid>>();

        _handler = new GetCompetitionResultsQueryHandler(
            _mockCompRepo.Object,
            _mockResultRepo.Object);
    }

    [Fact]
    public async Task Handle_WhenCompetitionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        _mockCompRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition>().AsQueryable());

        var query = new GetCompetitionResultsQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Competition.NotFound);
    }

    [Fact]
    public async Task Handle_WhenResultsExist_ShouldCalculateRankingsAndIdentifyBiggestCatchCorrectly()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var competition = Competition.Create(
            leagueId: Guid.NewGuid(),
            competitionNumber: 1,
            name: "Concurso Anual",
            date: DateTime.Today,
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Puerto",
            zone: "Zona A",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,
            maxSpots: 20,
            biggestCatchMinWeightInGrams: 300,
            id: competitionId
        );

        var r1 = CompetitionResult.Register(competitionId, 1);
        r1.RecordResult(didAttend: true, weightInGrams: 4500, biggestCatchWeight: 1200);
        r1.Points = 100;

        var r2 = CompetitionResult.Register(competitionId, 2);
        r2.RecordResult(didAttend: true, weightInGrams: 3000, biggestCatchWeight: 600);
        r2.Points = 80;

        var r3 = CompetitionResult.Register(competitionId, 3);
        r3.RecordResult(didAttend: false, weightInGrams: 0, biggestCatchWeight: null);
        r3.Points = 0;

        _mockCompRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { competition }.AsQueryable());

        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { r1, r2, r3 }.AsQueryable());

        var query = new GetCompetitionResultsQuery(competitionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);

        // r1 debería ser puesto 1 con Pieza Mayor
        var res1 = result.Value.First(x => x.FishermanId == 1);
        res1.Ranking.Should().Be(1);
        res1.IsBiggestCatch.Should().BeTrue();

        // r2 debería ser puesto 2
        var res2 = result.Value.First(x => x.FishermanId == 2);
        res2.Ranking.Should().Be(2);
        res2.IsBiggestCatch.Should().BeFalse();

        // r3 debería ser puesto 3 (no asistió)
        var res3 = result.Value.First(x => x.FishermanId == 3);
        res3.Ranking.Should().Be(3);
        res3.DidAttend.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenTiedPointsAndBiggestCatch_ShouldAssignSameRankToBothFishermen()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var competition = Competition.Create(
            leagueId: Guid.NewGuid(),
            competitionNumber: 1,
            name: "Concurso Empate",
            date: DateTime.Today,
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Puerto",
            zone: "Zona A",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,
            maxSpots: 10,
            biggestCatchMinWeightInGrams: 100,
            id: competitionId
        );

        var r1 = CompetitionResult.Register(competitionId, 1);
        r1.RecordResult(didAttend: true, weightInGrams: 2000, biggestCatchWeight: 500);
        r1.Points = 90;

        var r2 = CompetitionResult.Register(competitionId, 2);
        r2.RecordResult(didAttend: true, weightInGrams: 2000, biggestCatchWeight: 500);
        r2.Points = 90;

        _mockCompRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { competition }.AsQueryable());

        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { r1, r2 }.AsQueryable());

        var query = new GetCompetitionResultsQuery(competitionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);

        // Empate perfecto en puntos y pieza mayor -> mismo rango 1
        result.Value[0].Ranking.Should().Be(1);
        result.Value[1].Ranking.Should().Be(1);
    }

}
