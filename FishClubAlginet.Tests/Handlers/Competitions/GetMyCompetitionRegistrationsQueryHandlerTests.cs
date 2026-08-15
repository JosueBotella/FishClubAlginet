using FishClubAlginet.Application.Features.Competitions;
using FishClubAlginet.Contracts.Dtos.Responses.Competition;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class GetMyCompetitionRegistrationsQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockFishermanRepo;
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompetitionRepo;
    private readonly Mock<IGenericRepository<League, Guid>> _mockLeagueRepo;
    private readonly Mock<ILogger<GetMyCompetitionRegistrationsQueryHandler>> _mockLogger;
    private readonly GetMyCompetitionRegistrationsQueryHandler _handler;

    public GetMyCompetitionRegistrationsQueryHandlerTests()
    {
        _mockFishermanRepo = new Mock<IGenericRepository<Fisherman, int>>();
        _mockResultRepo = new Mock<IGenericRepository<CompetitionResult, Guid>>();
        _mockCompetitionRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockLeagueRepo = new Mock<IGenericRepository<League, Guid>>();
        _mockLogger = new Mock<ILogger<GetMyCompetitionRegistrationsQueryHandler>>();

        _handler = new GetMyCompetitionRegistrationsQueryHandler(
            _mockFishermanRepo.Object,
            _mockResultRepo.Object,
            _mockCompetitionRepo.Object,
            _mockLeagueRepo.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_UserWithoutFishermanProfile_ShouldReturnNotFoundError()
    {
        // Arrange
        _mockFishermanRepo.Setup(r => r.GetAll())
            .Returns(new List<Fisherman>().AsQueryable());

        // Act
        var result = await _handler.Handle(new GetMyCompetitionRegistrationsQuery("user-123"), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Fisherman.NotFound");
    }

    [Fact]
    public async Task Handle_FishermanWithNoRegistrations_ShouldReturnEmptyList()
    {
        // Arrange
        var fisherman = Fisherman.Create("Jose", "Botella", new DateTime(1990, 1, 1), TypeNationalIdentifier.Dni, "12345678Z", "FED-1", new Address());
        fisherman.Id = 10;
        fisherman.UserId = "user-123";

        _mockFishermanRepo.Setup(r => r.GetAll())
            .Returns(new List<Fisherman> { fisherman }.AsQueryable());
        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult>().AsQueryable());

        // Act
        var result = await _handler.Handle(new GetMyCompetitionRegistrationsQuery("user-123"), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FishermanWithRegistrations_ShouldReturnMappedDtosOrderedByDate()
    {
        // Arrange
        var fisherman = Fisherman.Create("Jose", "Botella", new DateTime(1990, 1, 1), TypeNationalIdentifier.Dni, "12345678Z", "FED-1", new Address());
        fisherman.Id = 10;
        fisherman.UserId = "user-123";

        var league = League.Create("Liga 2026", 2026);
        league.Id = Guid.NewGuid();

        var comp1 = Competition.Create(league.Id, 1, "Manga 1", DateTime.UtcNow.AddDays(-10), new TimeSpan(8, 0, 0), new TimeSpan(14, 0, 0), "Río Júcar", null, Subspecialty.AguaDulce, Category.Seniors, 20);
        var comp2 = Competition.Create(league.Id, 2, "Manga 2", DateTime.UtcNow.AddDays(5), new TimeSpan(8, 0, 0), new TimeSpan(14, 0, 0), "Albufera", null, Subspecialty.AguaDulce, Category.Seniors, 20);

        var result1 = CompetitionResult.Register(comp1.Id, fisherman.Id);
        result1.AssignedSpotNumber = 5;
        result1.WeightInGrams = 2500;
        result1.Points = 15;
        result1.Ranking = 1;

        var result2 = CompetitionResult.Register(comp2.Id, fisherman.Id);
        result2.AssignedSpotNumber = 12;

        _mockFishermanRepo.Setup(r => r.GetAll())
            .Returns(new List<Fisherman> { fisherman }.AsQueryable());
        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { result1, result2 }.AsQueryable());
        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { comp1, comp2 }.AsQueryable());
        _mockLeagueRepo.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        // Act
        var result = await _handler.Handle(new GetMyCompetitionRegistrationsQuery("user-123"), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.First().CompetitionName.Should().Be("Manga 2"); // most recent first
        result.Value.First().LeagueName.Should().Be("Liga 2026");
        result.Value.First().AssignedSpotNumber.Should().Be(12);

        result.Value.Last().CompetitionName.Should().Be("Manga 1");
        result.Value.Last().WeightInGrams.Should().Be(2500);
        result.Value.Last().Ranking.Should().Be(1);
    }
}
