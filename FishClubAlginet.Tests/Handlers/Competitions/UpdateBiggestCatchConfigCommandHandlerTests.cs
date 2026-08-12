using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class UpdateBiggestCatchConfigCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UpdateBiggestCatchConfigCommandHandler _handler;

    public UpdateBiggestCatchConfigCommandHandlerTests()
    {
        _mockRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _handler = new UpdateBiggestCatchConfigCommandHandler(
            _mockRepo.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenCompetitionExists_ShouldSetMinWeightAndSaveChanges()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var competition = Competition.Create(
            leagueId: Guid.NewGuid(),
            competitionNumber: 1,
            name: "Concurso Test",
            date: DateTime.Today,
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Puerto",
            zone: "Zona A",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,

            maxSpots: 20,
            biggestCatchMinWeightInGrams: null,
            id: competitionId
        );

        _mockRepo.Setup(r => r.GetById(competitionId))
            .ReturnsAsync(competition);

        var command = new UpdateBiggestCatchConfigCommand(competitionId, MinWeightInGrams: 400);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        competition.BiggestCatchMinWeightInGrams.Should().Be(400);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCompetitionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetById(competitionId))
            .ReturnsAsync((Competition?)null);

        var command = new UpdateBiggestCatchConfigCommand(competitionId, MinWeightInGrams: 500);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Competition.NotFound);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
