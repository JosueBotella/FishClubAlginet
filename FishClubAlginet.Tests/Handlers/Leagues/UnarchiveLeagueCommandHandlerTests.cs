using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class UnarchiveLeagueCommandHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<UnarchiveLeagueCommandHandler>> _mockLogger;
    private readonly UnarchiveLeagueCommandHandler _handler;

    public UnarchiveLeagueCommandHandlerTests()
    {
        _mockRepo = new Mock<IGenericRepository<League, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UnarchiveLeagueCommandHandler>>();
        _handler = new UnarchiveLeagueCommandHandler(
            _mockRepo.Object, _mockUnitOfWork.Object, _mockLogger.Object);
    }

    private static League BuildArchivedLeague() => new League
    {
        Id = Guid.NewGuid(),
        Name = "Liga 2023",
        Year = 2023,
        IsActive = false,
        IsArchived = true,
        MinPoints = 5,
        WorstResultsToDiscard = 0,
        LastUpdateUtc = DateTime.UtcNow.AddMonths(-6)
    };

    private void SetupGetAll(League? league)
    {
        var data = league is null ? new List<League>() : new List<League> { league };
        _mockRepo.Setup(r => r.GetAll()).Returns(data.AsQueryable());
    }

    [Fact]
    public async Task Handle_ArchivedLeague_ShouldUnarchiveAndPersist()
    {
        // Arrange
        var league = BuildArchivedLeague();
        SetupGetAll(league);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await _handler.Handle(new UnarchiveLeagueCommand(league.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsArchived.Should().BeFalse();
        league.IsArchived.Should().BeFalse();
        _mockRepo.Verify(r => r.Update(league), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NotArchivedLeague_ShouldReturnNotArchivedError()
    {
        // Arrange
        var league = new League
        {
            Id = Guid.NewGuid(),
            Name = "Liga 2024",
            Year = 2024,
            IsActive = true,
            IsArchived = false,
            LastUpdateUtc = DateTime.UtcNow
        };
        SetupGetAll(league);

        // Act
        var result = await _handler.Handle(new UnarchiveLeagueCommand(league.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotArchived");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LeagueNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        SetupGetAll(null);

        // Act
        var result = await _handler.Handle(new UnarchiveLeagueCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Unarchived_ShouldKeepIsActiveFalse()
    {
        // Arrange — liga archivada no debe reactivarse al desarchivar
        var league = BuildArchivedLeague();
        SetupGetAll(league);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        await _handler.Handle(new UnarchiveLeagueCommand(league.Id), CancellationToken.None);

        // Assert
        league.IsActive.Should().BeFalse("unarchiving should not re-activate the league");
    }
}
