using FluentAssertions;
using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class ReopenRegistrationCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ReopenRegistrationCommandHandler _handler;

    public ReopenRegistrationCommandHandlerTests()
    {
        _mockRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new ReopenRegistrationCommandHandler(_mockRepo.Object, _mockUnitOfWork.Object);
    }

    private static Competition BuildClosed(DateTime? lastUpdate = null) =>
        new Competition
        {
            Id = Guid.NewGuid(),
            LeagueId = Guid.NewGuid(),
            CompetitionNumber = 1,
            Status = CompetitionStatus.Closed,
            // ≤30 days ago by default (within window)
            LastUpdateUtc = lastUpdate ?? DateTime.UtcNow.AddDays(-5)
        };

    [Fact]
    public async Task Handle_ClosedWithinWindow_ShouldTransitionToRegistrationOpen()
    {
        // Arrange
        var competition = BuildClosed(DateTime.UtcNow.AddDays(-10));
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await _handler.Handle(new ReopenRegistrationCommand(competition.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        competition.Status.Should().Be(CompetitionStatus.RegistrationOpen);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClosedWindowExpired_ShouldReturnReopenWindowExpiredError()
    {
        // Arrange — closed 31 days ago (outside 30-day window)
        var competition = BuildClosed(DateTime.UtcNow.AddDays(-31));
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        // Act
        var result = await _handler.Handle(new ReopenRegistrationCommand(competition.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.ReopenWindowExpired");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CompetitionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Competition?)null);

        // Act
        var result = await _handler.Handle(
            new ReopenRegistrationCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.NotFound");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(CompetitionStatus.Planned)]
    [InlineData(CompetitionStatus.RegistrationOpen)]
    [InlineData(CompetitionStatus.ResultsDraft)]
    [InlineData(CompetitionStatus.ResultsValidated)]
    public async Task Handle_WrongStatus_ShouldReturnInvalidStatusTransitionError(CompetitionStatus status)
    {
        // Arrange
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            LeagueId = Guid.NewGuid(),
            CompetitionNumber = 1,
            Status = status,
            LastUpdateUtc = DateTime.UtcNow
        };
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        // Act
        var result = await _handler.Handle(
            new ReopenRegistrationCommand(competition.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.InvalidStatusTransition");
    }
}
