using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class UpdateLeagueCommandHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<UpdateLeagueCommandHandler>> _mockLogger;
    private readonly UpdateLeagueCommandHandler _handler;

    public UpdateLeagueCommandHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<League, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateLeagueCommandHandler>>();
        _handler = new UpdateLeagueCommandHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldUpdateAndPersist()
    {
        // Arrange
        var league = League.Create("Liga 2025", 2025, 5, 0);
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        var command = new UpdateLeagueCommand(league.Id, "Liga 2025 Actualizada", 7, 3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Liga 2025 Actualizada");
        result.Value.MinPoints.Should().Be(7);
        result.Value.WorstResultsToDiscard.Should().Be(3);
        // El año NO se cambia desde Update — comprobamos que la entidad mantiene el original.
        result.Value.Year.Should().Be(2025);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LeagueNotFound_ShouldReturnNotFoundAndNotPersist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());

        var command = new UpdateLeagueCommand(Guid.NewGuid(), "X", 5, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
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

        var command = new UpdateLeagueCommand(league.Id, "Cambio", 5, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.CannotModifyArchived");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LeagueSoftDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var league = League.Create("Liga borrada", 2023);
        league.IsDeleted = true;
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        var command = new UpdateLeagueCommand(league.Id, "X", 5, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
    }
}
