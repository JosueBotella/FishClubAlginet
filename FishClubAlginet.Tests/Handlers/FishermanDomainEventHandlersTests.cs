using FishClubAlginet.Application.Features.Events.Handlers;

namespace FishClubAlginet.Tests.Handlers;

public class FishermanDomainEventHandlersTests
{
    [Fact]
    public async Task FishermanAddedDomainEventHandler_ShouldLogInformation_WhenHandled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FishermanAddedDomainEventHandler>>();
        var handler = new FishermanAddedDomainEventHandler(mockLogger.Object);
        var domainEvent = new FishermanAddedDomainEvent { Id = 10, FirstName = "Carlos", LastName = "Sanz" };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fisherman creado") && v.ToString()!.Contains("Carlos")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task FishermanUpdatedDomainEventHandler_ShouldLogInformation_WhenHandled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FishermanUpdatedDomainEventHandler>>();
        var handler = new FishermanUpdatedDomainEventHandler(mockLogger.Object);
        var domainEvent = new FishermanUpdatedDomainEvent { Id = 10, FirstName = "Carlos", LastName = "Sanz Rius" };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fisherman actualizado") && v.ToString()!.Contains("Carlos")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task FishermanDeletedDomainEventHandler_ShouldLogInformation_WhenHandled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FishermanDeletedDomainEventHandler>>();
        var handler = new FishermanDeletedDomainEventHandler(mockLogger.Object);
        var domainEvent = new FishermanDeletedDomainEvent { Id = 15 };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fisherman eliminado") && v.ToString()!.Contains("15")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task FishermanRestoredDomainEventHandler_ShouldLogInformation_WhenHandled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FishermanRestoredDomainEventHandler>>();
        var handler = new FishermanRestoredDomainEventHandler(mockLogger.Object);
        var domainEvent = new FishermanRestoredDomainEvent { Id = 20 };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fisherman restaurado") && v.ToString()!.Contains("20")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
