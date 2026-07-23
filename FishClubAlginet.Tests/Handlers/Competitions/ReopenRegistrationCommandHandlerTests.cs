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
        Competition.Create(
            Guid.NewGuid(), 1, "Comp 1", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(14), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, 10, status: CompetitionStatus.Closed, lastUpdateUtc: lastUpdate ?? DateTime.UtcNow.AddDays(-5));

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
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Comp 1", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(14), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, 10, status: status);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        // Act
        var result = await _handler.Handle(
            new ReopenRegistrationCommand(competition.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.InvalidStatusTransition");
    }
}
