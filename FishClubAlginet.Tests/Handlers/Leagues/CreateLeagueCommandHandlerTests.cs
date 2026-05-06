using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class CreateLeagueCommandHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<CreateLeagueCommandHandler>> _mockLogger;
    private readonly CreateLeagueCommandHandler _handler;

    public CreateLeagueCommandHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<League, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<CreateLeagueCommandHandler>>();
        _handler = new CreateLeagueCommandHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateLeagueAndPersist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<League>()))
            .ReturnsAsync((League l) => l);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        var command = new CreateLeagueCommand("Liga 2026", 2026, 5, 2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeEmpty();
        _mockRepository.Verify(r => r.AddAsync(It.Is<League>(l =>
            l.Name == "Liga 2026" && l.Year == 2026 && l.MinPoints == 5 && l.WorstResultsToDiscard == 2)),
            Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateYear_ShouldReturnDuplicateYearErrorAndNotPersist()
    {
        // Arrange: ya existe una liga con Year=2026
        var existing = League.Create("Liga existente", 2026);
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { existing }.AsQueryable());

        var command = new CreateLeagueCommand("Otra Liga 2026", 2026, 5, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.DuplicateYear");
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<League>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateYearOnDeletedLeague_ShouldAllowCreation()
    {
        // Arrange: existe una liga 2026 pero soft-deleted → debe permitir crear otra
        var deleted = League.Create("Liga vieja", 2026);
        deleted.IsDeleted = true;
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { deleted }.AsQueryable());
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<League>()))
            .ReturnsAsync((League l) => l);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        var command = new CreateLeagueCommand("Liga nueva 2026", 2026, 5, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<League>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PersistFailsWithUniqueConstraint_ShouldMapToDuplicateYear()
    {
        // Arrange: race condition — el GetAll no detectó duplicado pero la BBDD sí
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<League>()))
            .ReturnsAsync((League l) => l);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict(
                code: "Database.UniqueConstraintViolation",
                description: "..."));

        var command = new CreateLeagueCommand("Liga 2026", 2026, 5, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.DuplicateYear");
    }

    [Fact]
    public async Task Handle_PersistFailsGenerically_ShouldReturnLeagueSaveFailed()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<League>()))
            .ReturnsAsync((League l) => l);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(
                code: "Database.SaveFailure",
                description: "..."));

        var command = new CreateLeagueCommand("Liga 2026", 2026, 5, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("LEAGUE_SAVE_FAILED");
    }
}
