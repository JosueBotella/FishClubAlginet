using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class GetCompetitionsByLeagueQueryHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockLeagueRepo;
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompetitionRepo;
    private readonly GetCompetitionsByLeagueQueryHandler _handler;

    public GetCompetitionsByLeagueQueryHandlerTests()
    {
        _mockLeagueRepo = new Mock<IGenericRepository<League, Guid>>();
        _mockCompetitionRepo = new Mock<IGenericRepository<Competition, Guid>>();

        _handler = new GetCompetitionsByLeagueQueryHandler(
            _mockLeagueRepo.Object,
            _mockCompetitionRepo.Object);
    }

    [Fact]
    public async Task Handle_WhenLeagueNotFound_ShouldReturnLeagueNotFoundError()
    {
        // Arrange
        _mockLeagueRepo.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());

        var query = new GetCompetitionsByLeagueQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.League.NotFound);
    }

    [Fact]
    public async Task Handle_WhenLeagueExists_ShouldReturnOrderedCompetitionDtosByNumber()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var league = League.Create("Liga Alginet 2026", 2026);
        league.Id = leagueId;

        var comp1 = Competition.Create(
            leagueId: leagueId,
            competitionNumber: 2,
            name: "Concurso 2",
            date: DateTime.Today.AddDays(14),
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Playa 2",
            zone: "Zona B",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,
            maxSpots: 15
        );

        var comp2 = Competition.Create(
            leagueId: leagueId,
            competitionNumber: 1,
            name: "Concurso 1",
            date: DateTime.Today.AddDays(7),
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Playa 1",
            zone: "Zona A",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,
            maxSpots: 15
        );

        _mockLeagueRepo.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { comp1, comp2 }.AsQueryable());

        var query = new GetCompetitionsByLeagueQuery(leagueId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        // Deberían devolverse ordenados por CompetitionNumber ascendente
        result.Value[0].CompetitionNumber.Should().Be(1);
        result.Value[0].Name.Should().Be("Concurso 1");
        result.Value[1].CompetitionNumber.Should().Be(2);
        result.Value[1].Name.Should().Be("Concurso 2");
    }

    [Fact]
    public async Task Handle_WhenCompetitionIsDeleted_ShouldExcludeDeletedCompetitions()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var league = League.Create("Liga Alginet 2026", 2026);
        league.Id = leagueId;

        var comp1 = Competition.Create(
            leagueId: leagueId,
            competitionNumber: 1,
            name: "Concurso Activo",
            date: DateTime.Today.AddDays(7),
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Playa 1",
            zone: "Zona A",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,
            maxSpots: 15
        );

        var comp2 = Competition.Create(
            leagueId: leagueId,
            competitionNumber: 2,
            name: "Concurso Borrado",
            date: DateTime.Today.AddDays(14),
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Playa 2",
            zone: "Zona B",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,

            maxSpots: 15
        );
        comp2.IsDeleted = true;

        _mockLeagueRepo.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { comp1, comp2 }.AsQueryable());

        var query = new GetCompetitionsByLeagueQuery(leagueId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Concurso Activo");
    }
}
