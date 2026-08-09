using FishClubAlginet.Application.Features.Events.Commands.Fishermen;
using FishClubAlginet.Application.Features.Fishermen;

namespace FishClubAlginet.Tests.Handlers;

public class RestoreFishermanCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<RestoreFishermanCommandHandler>> _mockLogger;
    private readonly RestoreFishermanCommandHandler _handler;

    public RestoreFishermanCommandHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<Fisherman, int>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<RestoreFishermanCommandHandler>>();
        _handler = new RestoreFishermanCommandHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    private static Fisherman BuildDeletedFisherman(int id = 1) =>
        new Fisherman
        {
            Id = id,
            FirstName = "Juan",
            LastName = "García",
            DateOfBirth = new DateTime(1985, 6, 15),
            DocumentType = TypeNationalIdentifier.Dni,
            DocumentNumber = "12345678A",
            IsDeleted = true,
            DeletedTimeUtc = DateTime.UtcNow.AddDays(-1),
            Address = new Address { Street = "Calle Mayor 1", City = "Alginet", ZipCode = "46250", Province = "Valencia" }
        };

    [Fact]
    public async Task Handle_WhenFishermanIsDeleted_ShouldRestoreAndPersist()
    {
        // Arrange
        var fisherman = BuildDeletedFisherman();
        _mockRepository.Setup(r => r.GetById(fisherman.Id)).ReturnsAsync(fisherman);
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await _handler.Handle(
            new RestoreFishermanCommand(fisherman.Id),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
        Assert.False(fisherman.IsDeleted);
        Assert.Null(fisherman.DeletedTimeUtc);
        _mockRepository.Verify(r => r.Update(fisherman), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFishermanIsDeleted_ShouldRaiseFishermanRestoredDomainEvent()
    {
        // Arrange
        var fisherman = BuildDeletedFisherman();
        _mockRepository.Setup(r => r.GetById(fisherman.Id)).ReturnsAsync(fisherman);
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        await _handler.Handle(new RestoreFishermanCommand(fisherman.Id), CancellationToken.None);

        // Assert
        var domainEvents = fisherman.GetDomainEvents();
        Assert.Single(domainEvents);
        var restoredEvent = domainEvents.First() as FishermanRestoredDomainEvent;
        Assert.NotNull(restoredEvent);
        Assert.Equal(fisherman.Id, restoredEvent.Id);
    }

    [Fact]
    public async Task Handle_WhenFishermanNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        const int fishermanId = 999;
        _mockRepository.Setup(r => r.GetById(fishermanId)).ReturnsAsync((Fisherman?)null);

        // Act
        var result = await _handler.Handle(
            new RestoreFishermanCommand(fishermanId),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("Fisherman.NotFound", result.FirstError.Code);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenFishermanIsNotDeleted_ShouldReturnValidationError()
    {
        // Arrange
        var fisherman = BuildDeletedFisherman();
        fisherman.IsDeleted = false; // Active fisherman
        _mockRepository.Setup(r => r.GetById(fisherman.Id)).ReturnsAsync(fisherman);

        // Act
        var result = await _handler.Handle(
            new RestoreFishermanCommand(fisherman.Id),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
        Assert.Equal("Fisherman.NotDeleted", result.FirstError.Code);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPersistenceFails_ShouldPropagateUnitOfWorkError()
    {
        // Arrange
        var fisherman = BuildDeletedFisherman();
        _mockRepository.Setup(r => r.GetById(fisherman.Id)).ReturnsAsync(fisherman);
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(
                code: "Database.SaveFailure",
                description: "Failed to save the record. Please try again."));

        // Act
        var result = await _handler.Handle(
            new RestoreFishermanCommand(fisherman.Id),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Database.SaveFailure", result.FirstError.Code);
    }
}
