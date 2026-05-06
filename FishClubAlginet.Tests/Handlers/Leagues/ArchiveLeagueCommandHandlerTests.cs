using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class ArchiveLeagueCommandHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<ArchiveLeagueCommandHandler>> _mockLogger;
    private readonly ArchiveLeagueCommandHandler _handler;

    public ArchiveLeagueCommandHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<League, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ArchiveLeagueCommandHandler>>();
        _handler = new ArchiveLeagueCommandHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ArchiveActiveLeague_ShouldArchiveAndPersist()
    {
        // Arrange
        var league = League.Create("Liga 2024", 2024);
        league.Activate();
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        var command = new ArchiveLeagueCommand(league.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsArchived.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
        league.IsArchived.Should().BeTrue();
        league.IsActive.Should().BeFalse();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LeagueNotFound_ShouldReturnNotFoundAndNotPersist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());

        var command = new ArchiveLeagueCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LeagueAlreadyArchived_ShouldReturnAlreadyArchived()
    {
        // Arrange
        var league = League.Create("Liga 2023", 2023);
        league.Archive();
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        var command = new ArchiveLeagueCommand(league.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.AlreadyArchived");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
