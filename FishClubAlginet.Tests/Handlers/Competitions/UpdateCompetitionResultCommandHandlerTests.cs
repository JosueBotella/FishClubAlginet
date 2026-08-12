using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class UpdateCompetitionResultCommandHandlerTests
{
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UpdateCompetitionResultCommandHandler _handler;

    public UpdateCompetitionResultCommandHandlerTests()
    {
        _mockResultRepo = new Mock<IGenericRepository<CompetitionResult, Guid>>();
        _mockCompRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _handler = new UpdateCompetitionResultCommandHandler(
            _mockResultRepo.Object,
            _mockCompRepo.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenResultAndCompetitionExist_ShouldRecordResultAndSaveChanges()
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
            zone: "Zona A",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,

            maxSpots: 10,
            id: competitionId
        );

        var registration = CompetitionResult.Register(competitionId, fishermanId: 5);
        registration.Id = resultId;

        _mockResultRepo.Setup(r => r.GetById(resultId))
            .ReturnsAsync(registration);

        _mockCompRepo.Setup(r => r.GetById(competitionId))
            .ReturnsAsync(competition);

        var command = new UpdateCompetitionResultCommand(
            ResultId: resultId,
            DidAttend: true,
            WeightInGrams: 3500,
            BiggestCatchWeight: 1200
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        registration.DidAttend.Should().BeTrue();
        registration.WeightInGrams.Should().Be(3500);
        registration.BiggestCatchWeight.Should().Be(1200);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenResultNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var resultId = Guid.NewGuid();
        _mockResultRepo.Setup(r => r.GetById(resultId))
            .ReturnsAsync((CompetitionResult?)null);

        var command = new UpdateCompetitionResultCommand(
            ResultId: resultId,
            DidAttend: true,
            WeightInGrams: 1000,
            BiggestCatchWeight: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Competition.NotFound);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCompetitionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var resultId = Guid.NewGuid();

        var registration = CompetitionResult.Register(competitionId, fishermanId: 5);
        registration.Id = resultId;

        _mockResultRepo.Setup(r => r.GetById(resultId))
            .ReturnsAsync(registration);

        _mockCompRepo.Setup(r => r.GetById(competitionId))
            .ReturnsAsync((Competition?)null);

        var command = new UpdateCompetitionResultCommand(
            ResultId: resultId,
            DidAttend: true,
            WeightInGrams: 1000,
            BiggestCatchWeight: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Competition.NotFound);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
