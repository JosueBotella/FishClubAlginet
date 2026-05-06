using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class ActivateLeagueCommandHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<ActivateLeagueCommandHandler>> _mockLogger;
    private readonly ActivateLeagueCommandHandler _handler;

    public ActivateLeagueCommandHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<League, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ActivateLeagueCommandHandler>>();
        _handler = new ActivateLeagueCommandHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ActivateInactiveLeague_ShouldDeactivateOthersAndActivateTarget()
    {
        // Arrange: 2 ligas previas activas (no debería pasar pero el handler lo limpia) + la objetivo
        var prevActive1 = League.Create("Liga 2024", 2024);
        prevActive1.Activate();
        var prevActive2 = League.Create("Liga 2023", 2023);
        prevActive2.Activate();
        var target = League.Create("Liga 2026", 2026);

        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { prevActive1, prevActive2, target }.AsQueryable());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        var command = new ActivateLeagueCommand(target.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsActive.Should().BeTrue();
        target.IsActive.Should().BeTrue();
        prevActive1.IsActive.Should().BeFalse();
        prevActive2.IsActive.Should().BeFalse();

        // Verificamos persistencia única (Save) y Update por cada liga modificada
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.Update(prevActive1), Times.Once);
        _mockRepository.Verify(r => r.Update(prevActive2), Times.Once);
        _mockRepository.Verify(r => r.Update(target), Times.Once);
    }

    [Fact]
    public async Task Handle_LeagueNotFound_ShouldReturnNotFoundAndNotPersist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());

        var command = new ActivateLeagueCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LeagueAlreadyActive_ShouldReturnAlreadyActive()
    {
        // Arrange
        var league = League.Create("Liga 2026", 2026);
        league.Activate();
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        var command = new ActivateLeagueCommand(league.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.AlreadyActive");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LeagueArchived_ShouldReturnCannotModifyArchived()
    {
        // Arrange
        var league = League.Create("Liga 2024", 2024);
        league.Archive();
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        var command = new ActivateLeagueCommand(league.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.CannotModifyArchived");
    }
}
