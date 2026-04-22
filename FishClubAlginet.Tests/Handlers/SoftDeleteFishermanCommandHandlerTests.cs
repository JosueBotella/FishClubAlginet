using FishClubAlginet.Application.Features.Fishermen;

namespace FishClubAlginet.Tests.Handlers;

public class SoftDeleteFishermanCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockRepository;
    private readonly Mock<ILogger<SoftDeleteFishermanCommandHandler>> _mockLogger;
    private readonly SoftDeleteFishermanCommandHandler _handler;

    public SoftDeleteFishermanCommandHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<Fisherman, int>>();
        _mockLogger = new Mock<ILogger<SoftDeleteFishermanCommandHandler>>();
        _handler = new SoftDeleteFishermanCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenFishermanExists_ShouldSoftDeleteSuccessfully()
    {
        // Arrange
        const int fishermanId = 1;
        _mockRepository.Setup(repo => repo.SoftDelete(fishermanId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(
            new SoftDeleteFishermanCommand(fishermanId),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
        _mockRepository.Verify(repo => repo.SoftDelete(fishermanId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFishermanNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        const int fishermanId = 999;
        _mockRepository.Setup(repo => repo.SoftDelete(fishermanId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(
            new SoftDeleteFishermanCommand(fishermanId),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Single(result.Errors);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("Fisherman.NotFound", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WhenFishermanNotFound_ShouldLogWarning()
    {
        // Arrange
        const int fishermanId = 999;
        _mockRepository.Setup(repo => repo.SoftDelete(fishermanId))
            .ReturnsAsync(false);

        // Act
        await _handler.Handle(
            new SoftDeleteFishermanCommand(fishermanId),
            CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found for soft delete")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldLogInformation()
    {
        // Arrange
        const int fishermanId = 1;
        _mockRepository.Setup(repo => repo.SoftDelete(fishermanId))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(
            new SoftDeleteFishermanCommand(fishermanId),
            CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("soft deleted")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
