using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class RemoveRegistrationCommandHandlerTests
{
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly RemoveRegistrationCommandHandler _handler;

    public RemoveRegistrationCommandHandlerTests()
    {
        _mockResultRepo = new Mock<IGenericRepository<CompetitionResult, Guid>>();
        _mockCompRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _handler = new RemoveRegistrationCommandHandler(
            _mockResultRepo.Object,
            _mockCompRepo.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenRegistrationExists_ShouldSoftDeleteAndDecrementParticipantCount()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var resultId = Guid.NewGuid();

        var competition = Competition.Create(
            leagueId: Guid.NewGuid(),
            competitionNumber: 1,
            name: "Concurso Test",
            date: DateTime.Today,
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Playa",
            zone: "A",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,

            maxSpots: 10,
            id: competitionId
        );
        competition.IncrementParticipantCount(); // Count = 1

        var registration = CompetitionResult.Register(competitionId, fishermanId: 10);
        registration.Id = resultId;

        _mockResultRepo.Setup(r => r.GetById(resultId))
            .ReturnsAsync(registration);

        _mockCompRepo.Setup(r => r.GetById(competitionId))
            .ReturnsAsync(competition);

        var command = new RemoveRegistrationCommand(resultId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        registration.IsDeleted.Should().BeTrue();
        competition.ParticipantCount.Should().Be(0);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRegistrationNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var resultId = Guid.NewGuid();
        _mockResultRepo.Setup(r => r.GetById(resultId))
            .ReturnsAsync((CompetitionResult?)null);

        var command = new RemoveRegistrationCommand(resultId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Competition.NotFound);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
