

namespace FishClubAlginet.Tests.Handlers;

public class FisherManAddCommanHandlerTests
{
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<FisherManAddCommandHandler>> _mockLogger;

    public FisherManAddCommanHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<Fisherman, int>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<FisherManAddCommandHandler>>();
    }

    private FisherManAddCommandHandler CreateHandler() =>
        new FisherManAddCommandHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);

    private static FisherManCommand BuildCommand(string firstName = "John") =>
        new FisherManCommand(
            FirstName: firstName,
            LastName: "Doe",
            DateOfBirth: new(1994, 7, 5, 16, 23, 42, DateTimeKind.Utc),
            DocumentType: TypeNationalIdentifier.Dni,
            DocumentNumber: "12345678A",
            FederationLicense: "FED12345"
        );

    [Fact]
    public async Task Handle_ValidCommand_ShouldStageAndPersistAndReturnId()
    {
        // Arrange
        var command = BuildCommand();
        Fisherman? captured = null;

        // AddAsync ahora sólo "stagea" la entidad y la devuelve.
        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Fisherman>()))
            .Callback<Fisherman>(f =>
            {
                f.Id = 1; // Simula que la BBDD asigna el Id al persistir.
                captured = f;
            })
            .ReturnsAsync((Fisherman f) => f);

        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(1, result.Value);

        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Fisherman>()), Times.Once);
        // Critical: el UoW debe persistir tras el staging del repo.
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify logging occurred
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fisherman created successfully")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFailureErrorAndLog()
    {
        // Arrange
        var command = BuildCommand();

        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Fisherman>()))
            .ReturnsAsync((Fisherman f) => f);

        // El UoW devuelve un Error.Failure genérico (cualquier fallo de DB
        // no clasificado). El handler lo mapea a FISHERMAN_SAVE_FAILED.
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(
                code: "Database.SaveFailure",
                description: "Failed to save the record. Please try again."));

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Single(result.Errors);
        Assert.Equal("FISHERMAN_SAVE_FAILED", result.FirstError.Code);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating Fisherman")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDuplicate_ShouldReturnConflictError()
    {
        // Arrange
        var command = BuildCommand();

        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Fisherman>()))
            .ReturnsAsync((Fisherman f) => f);

        // El UoW traduce una violación de unique constraint (SqlException 2627/2601)
        // a este Error.Conflict.
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict(
                code: "Database.UniqueConstraintViolation",
                description: "A record with these unique values already exists."));

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Single(result.Errors);
        Assert.Equal($"{nameof(Fisherman)}.Duplicate", result.FirstError.Code);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldRaiseDomainEvent()
    {
        // Arrange
        var command = new FisherManCommand(
            FirstName: "Jane",
            LastName: "Smith",
            DateOfBirth: new(1995, 3, 15, 10, 30, 0, DateTimeKind.Utc),
            DocumentType: TypeNationalIdentifier.Dni,
            DocumentNumber: "87654321B",
            FederationLicense: "FED54321"
        );

        // Capture the fisherman object passed to AddAsync to verify the domain event
        Fisherman? capturedFisherman = null;
        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Fisherman>()))
            .Callback<Fisherman>(f =>
            {
                f.Id = 2;
                capturedFisherman = f;
            })
            .ReturnsAsync((Fisherman f) => f);

        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value);

        // Verify the domain event was raised (it should be in the captured fisherman)
        Assert.NotNull(capturedFisherman);
        var domainEvents = capturedFisherman!.GetDomainEvents();
        Assert.NotEmpty(domainEvents);
        Assert.Single(domainEvents);

        var fishermanAddedEvent = domainEvents.FirstOrDefault() as FishermanAddedDomainEvent;
        Assert.NotNull(fishermanAddedEvent);
        Assert.Equal(command.FirstName, fishermanAddedEvent!.FirstName);
        Assert.Equal(command.LastName, fishermanAddedEvent.LastName);
        Assert.Equal(command.DocumentNumber, fishermanAddedEvent.DocumentNumber);
    }
}
