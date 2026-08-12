using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class GetCompetitionByIdQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockRepo;
    private readonly GetCompetitionByIdQueryHandler _handler;

    public GetCompetitionByIdQueryHandlerTests()
    {
        _mockRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _handler = new GetCompetitionByIdQueryHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_WhenCompetitionExists_ShouldReturnCompetitionDto()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var competition = Competition.Create(
            leagueId: leagueId,
            competitionNumber: 1,
            name: "Concurso Anual 1",
            date: DateTime.Today.AddDays(7),
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Puerto de Alginet",
            zone: "Zona A",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,
            maxSpots: 20,
            biggestCatchMinWeightInGrams: 500,
            id: competitionId
        );

        _mockRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { competition }.AsQueryable());

        var query = new GetCompetitionByIdQuery(competitionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(competitionId);
        result.Value.LeagueId.Should().Be(leagueId);
        result.Value.Name.Should().Be("Concurso Anual 1");
        result.Value.MaxSpots.Should().Be(20);
        result.Value.BiggestCatchMinWeightInGrams.Should().Be(500);
    }

    [Fact]
    public async Task Handle_WhenCompetitionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition>().AsQueryable());

        var query = new GetCompetitionByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Competition.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCompetitionIsDeleted_ShouldReturnNotFoundError()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var competition = Competition.Create(
            leagueId: Guid.NewGuid(),
            competitionNumber: 1,
            name: "Concurso Borrado",
            date: DateTime.Today,
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Puerto",
            zone: "Zona A",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,

            maxSpots: 10,
            id: competitionId
        );
        competition.IsDeleted = true;

        _mockRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { competition }.AsQueryable());

        var query = new GetCompetitionByIdQuery(competitionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Competition.NotFound);
    }
}
