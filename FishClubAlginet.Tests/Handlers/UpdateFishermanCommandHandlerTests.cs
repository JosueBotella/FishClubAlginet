using FishClubAlginet.Application.Features.Fishermen;

namespace FishClubAlginet.Tests.Handlers;

public class UpdateFishermanCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<UpdateFishermanCommandHandler>> _mockLogger;
    private readonly UpdateFishermanCommandHandler _handler;

    public UpdateFishermanCommandHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<Fisherman, int>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateFishermanCommandHandler>>();
        _handler = new UpdateFishermanCommandHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    private static Fisherman BuildFisherman(int id = 1) =>
        new Fisherman
        {
            Id = id,
            FirstName = "Juan",
            LastName = "García",
            DateOfBirth = new DateTime(1985, 6, 15),
            DocumentType = TypeNationalIdentifier.Dni,
            DocumentNumber = "12345678A",
            Address = new Address { Street = "Calle Mayor 1", City = "Alginet", ZipCode = "46250", Province = "Valencia" }
        };

    private static UpdateFishermanCommand BuildCommand(int id = 1) =>
        new UpdateFishermanCommand(
            Id: id,
            FirstName: "Pedro",
            LastName: "López",
            AddressStreet: "Calle Nueva 5",
            AddressCity: "Algemesí",
            AddressZipCode: "46260",
            AddressProvince: "Valencia");

    [Fact]
    public async Task Handle_WhenFishermanExists_ShouldUpdateFieldsAndPersist()
    {
        // Arrange
        var fisherman = BuildFisherman();
        var command = BuildCommand();
        _mockRepository.Setup(r => r.GetById(command.Id)).ReturnsAsync(fisherman);
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
        Assert.Equal("Pedro", fisherman.FirstName);
        Assert.Equal("López", fisherman.LastName);
        Assert.Equal("Calle Nueva 5", fisherman.Address.Street);
        _mockRepository.Verify(r => r.Update(fisherman), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFishermanExists_ShouldRaiseFishermanUpdatedDomainEvent()
    {
        // Arrange
        var fisherman = BuildFisherman();
        var command = BuildCommand();
        _mockRepository.Setup(r => r.GetById(command.Id)).ReturnsAsync(fisherman);
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: el interceptor capturará este evento al hacer SaveChanges
        var domainEvents = fisherman.GetDomainEvents();
        Assert.Single(domainEvents);
        var updatedEvent = domainEvents.First() as FishermanUpdatedDomainEvent;
        Assert.NotNull(updatedEvent);
        Assert.Equal(fisherman.Id, updatedEvent.Id);
        Assert.Equal("Pedro", updatedEvent.FirstName);
        Assert.Equal("López", updatedEvent.LastName);
    }

    [Fact]
    public async Task Handle_WhenFishermanNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        const int fishermanId = 999;
        _mockRepository.Setup(r => r.GetById(fishermanId)).ReturnsAsync((Fisherman?)null);

        // Act
        var result = await _handler.Handle(
            BuildCommand(fishermanId),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("Fisherman.NotFound", result.FirstError.Code);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenFishermanNotFound_ShouldLogWarning()
    {
        // Arrange
        const int fishermanId = 999;
        _mockRepository.Setup(r => r.GetById(fishermanId)).ReturnsAsync((Fisherman?)null);

        // Act
        await _handler.Handle(BuildCommand(fishermanId), CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found for update")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPersistenceFails_ShouldPropagateUnitOfWorkError()
    {
        // Arrange
        var fisherman = BuildFisherman();
        var command = BuildCommand();
        _mockRepository.Setup(r => r.GetById(command.Id)).ReturnsAsync(fisherman);
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(
                code: "Database.SaveFailure",
                description: "Failed to save the record. Please try again."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Database.SaveFailure", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldLogInformation()
    {
        // Arrange
        var fisherman = BuildFisherman();
        var command = BuildCommand();
        _mockRepository.Setup(r => r.GetById(command.Id)).ReturnsAsync(fisherman);
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("updated successfully")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
