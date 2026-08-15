namespace FishClubAlginet.Tests.Handlers;

[SuppressMessage("Performance", "CA1873:Avoid potentially expensive evaluations on logging calls", Justification = "Test assertions on logger invocations")]
public class CompetitionDomainEventHandlersTests
{
    [Fact]
    public async Task CompetitionCreatedDomainEventHandler_ShouldLogInformation_WhenHandled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CompetitionCreatedDomainEventHandler>>();
        var handler = new CompetitionCreatedDomainEventHandler(mockLogger.Object);
        var domainEvent = new CompetitionCreatedDomainEvent
        {
            CompetitionId = Guid.NewGuid(),
            LeagueId = Guid.NewGuid(),
            CompetitionNumber = 3,
            Name = "Manga de Apertura",
            Date = DateTime.UtcNow.AddDays(10),
            MaxSpots = 25
        };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Competition creada") && v.ToString()!.Contains("Manga de Apertura")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CompetitionRegistrationOpenedDomainEventHandler_ShouldLogInformation_WhenHandled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CompetitionRegistrationOpenedDomainEventHandler>>();
        var handler = new CompetitionRegistrationOpenedDomainEventHandler(mockLogger.Object);
        var domainEvent = new CompetitionRegistrationOpenedDomainEvent
        {
            CompetitionId = Guid.NewGuid(),
            LeagueId = Guid.NewGuid(),
            CompetitionNumber = 1
        };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Inscripciones abiertas")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CompetitionRegistrationClosedDomainEventHandler_ShouldLogInformation_WhenHandled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CompetitionRegistrationClosedDomainEventHandler>>();
        var handler = new CompetitionRegistrationClosedDomainEventHandler(mockLogger.Object);
        var domainEvent = new CompetitionRegistrationClosedDomainEvent
        {
            CompetitionId = Guid.NewGuid(),
            LeagueId = Guid.NewGuid(),
            CompetitionNumber = 1,
            ParticipantCount = 18
        };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Inscripciones cerradas") && v.ToString()!.Contains("18")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CompetitionMovedToResultsDraftDomainEventHandler_ShouldLogInformation_WhenHandled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CompetitionMovedToResultsDraftDomainEventHandler>>();
        var handler = new CompetitionMovedToResultsDraftDomainEventHandler(mockLogger.Object);
        var domainEvent = new CompetitionMovedToResultsDraftDomainEvent
        {
            CompetitionId = Guid.NewGuid(),
            LeagueId = Guid.NewGuid()
        };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("borrador de resultados")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CompetitionResultsValidatedDomainEventHandler_ShouldLogInformation_WhenHandled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CompetitionResultsValidatedDomainEventHandler>>();
        var handler = new CompetitionResultsValidatedDomainEventHandler(mockLogger.Object);
        var domainEvent = new CompetitionResultsValidatedDomainEvent
        {
            CompetitionId = Guid.NewGuid(),
            LeagueId = Guid.NewGuid(),
            CompetitionNumber = 2
        };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resultados validados")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
