using FluentAssertions;
using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class AssignSpotsCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompetitionRepo;
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly AssignSpotsCommandHandler _handler;

    public AssignSpotsCommandHandlerTests()
    {
        _mockCompetitionRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockResultRepo = new Mock<IGenericRepository<CompetitionResult, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new AssignSpotsCommandHandler(
            _mockCompetitionRepo.Object,
            _mockResultRepo.Object,
            _mockUnitOfWork.Object);
    }

    private static Competition BuildCompetition(CompetitionStatus status) =>
        new Competition
        {
            Id = Guid.NewGuid(),
            LeagueId = Guid.NewGuid(),
            CompetitionNumber = 1,
            Status = status,
            LastUpdateUtc = DateTime.UtcNow
        };

    private static CompetitionResult BuildResult(Guid competitionId, DateTime? registrationDate = null) =>
        new CompetitionResult
        {
            Id = Guid.NewGuid(),
            CompetitionId = competitionId,
            FishermanId = 1,
            RegistrationDate = registrationDate ?? DateTime.UtcNow,
            LastUpdateUtc = DateTime.UtcNow
        };

    private void SetupResults(Guid competitionId, List<CompetitionResult> results)
    {
        _mockResultRepo.Setup(r => r.GetAll()).Returns(results.AsQueryable());
    }

    [Fact]
    public async Task Handle_WithRegisteredFishermen_ShouldAssignSequentialSpots()
    {
        // Arrange
        var competition = BuildCompetition(CompetitionStatus.RegistrationOpen);
        var results = new List<CompetitionResult>
        {
            BuildResult(competition.Id, DateTime.UtcNow.AddMinutes(-30)),
            BuildResult(competition.Id, DateTime.UtcNow.AddMinutes(-20)),
            BuildResult(competition.Id, DateTime.UtcNow.AddMinutes(-10)),
        };
        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        SetupResults(competition.Id, results);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await _handler.Handle(new AssignSpotsCommand(competition.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        results[0].AssignedSpotNumber.Should().Be(1);
        results[1].AssignedSpotNumber.Should().Be(2);
        results[2].AssignedSpotNumber.Should().Be(3);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CompetitionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        _mockCompetitionRepo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Competition?)null);

        // Act
        var result = await _handler.Handle(new AssignSpotsCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.NotFound");
    }

    [Fact]
    public async Task Handle_NoRegisteredFishermen_ShouldReturnNoResultsToAssignError()
    {
        // Arrange
        var competition = BuildCompetition(CompetitionStatus.RegistrationOpen);
        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        SetupResults(competition.Id, new List<CompetitionResult>());

        // Act
        var result = await _handler.Handle(new AssignSpotsCommand(competition.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.NoResultsToAssign");
    }

    [Theory]
    [InlineData(CompetitionStatus.Planned)]
    [InlineData(CompetitionStatus.ResultsDraft)]
    [InlineData(CompetitionStatus.ResultsValidated)]
    public async Task Handle_InvalidStatus_ShouldReturnInvalidStatusTransitionError(CompetitionStatus status)
    {
        // Arrange
        var competition = BuildCompetition(status);
        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        // Act
        var result = await _handler.Handle(new AssignSpotsCommand(competition.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.InvalidStatusTransition");
    }
}
