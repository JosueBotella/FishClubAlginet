

namespace FishClubAlginet.Tests.Handlers;

public class FisherManAddCommanHandlerTests
{
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockRepository;
    private readonly Mock<ILogger<FisherManAddCommandHandler>> _mockLogger;

    public FisherManAddCommanHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<Fisherman, int>>();
        _mockLogger = new Mock<ILogger<FisherManAddCommandHandler>>();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnFishermanId()
    {
        // Arrange
        var handler = new FisherManAddCommandHandler(_mockRepository.Object, _mockLogger.Object);
        var command = new FisherManCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: new(1994, 7, 5, 16, 23, 42, DateTimeKind.Utc),
            DocumentType: TypeNationalIdentifier.Dni,
            DocumentNumber: "12345678A",
            FederationLicense: "FED12345"
        );

        var expectedFisherman = new Fisherman
        {
            Id = 1,
            FirstName = command.FirstName,
            LastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            DocumentType = command.DocumentType,
            DocumentNumber = command.DocumentNumber,
            FederationLicense = command.FederationLicense
        };

        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Fisherman>()))
                 .ReturnsAsync(expectedFisherman);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError); 
        Assert.Equal(1, result.Value);

        // Verify the fisherman was added to the repository
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Fisherman>()), Times.Once);

        // Verify logging occurred
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fisherman created successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ShouldReturnError()
    {
        // Arrange
        var mockRepository = new Mock<IGenericRepository<Fisherman, int>>();
        var mockLogger = new Mock<ILogger<FisherManAddCommandHandler>>();

        var handler = new FisherManAddCommandHandler(mockRepository.Object, mockLogger.Object);
        var command = new FisherManCommand(
            FirstName: "", 
            LastName: "Doe",
            DateOfBirth: new(1994, 7, 5, 16, 23, 42, DateTimeKind.Utc),
            DocumentType: TypeNationalIdentifier.Dni,
            DocumentNumber: "12345678A",
            FederationLicense: "FED12345"
        );

        mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Fisherman>()))
                      .ReturnsAsync(ErrorOr<Fisherman>.From(new List<Error>
                      {
                          Error.Validation(ValidatorsConstants.FisherManValidationConstants.FirstNameRequiredErrorCode, ValidatorsConstants.FisherManValidationConstants.FirstNameRequiredErrorMessage)
                      }));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Single(result.Errors);

        // Verify logging of error occurred
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating Fisherman")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldRaiseDomainEvent()
    {
        // Arrange
        var mockRepository = new Mock<IGenericRepository<Fisherman, int>>();
        var mockLogger = new Mock<ILogger<FisherManAddCommandHandler>>();

        var handler = new FisherManAddCommandHandler(mockRepository.Object, mockLogger.Object);
        var command = new FisherManCommand(
            FirstName: "Jane",
            LastName: "Smith",
            DateOfBirth: new(1995, 3, 15, 10, 30, 0, DateTimeKind.Utc),
            DocumentType: TypeNationalIdentifier.Dni,
            DocumentNumber: "87654321B",
            FederationLicense: "FED54321"
        );

        var expectedFisherman = new Fisherman
        {
            Id = 2,
            FirstName = command.FirstName,
            LastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            DocumentType = command.DocumentType,
            DocumentNumber = command.DocumentNumber,
            FederationLicense = command.FederationLicense
        };

        // Capture the fisherman object passed to AddAsync to verify the domain event
        Fisherman capturedFisherman = null;
        mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Fisherman>()))
                 .Callback<Fisherman>(f => capturedFisherman = f)
                 .ReturnsAsync(expectedFisherman);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value);

        // Verify the domain event was raised (it should be in the captured fisherman)
        Assert.NotNull(capturedFisherman);
        var domainEvents = capturedFisherman.GetDomainEvents();
        Assert.NotEmpty(domainEvents);
        Assert.Single(domainEvents);

        var fishermanAddedEvent = domainEvents.FirstOrDefault() as FishermanAddedDomainEvent;
        Assert.NotNull(fishermanAddedEvent);
        Assert.Equal(command.FirstName, fishermanAddedEvent.FirstName);
        Assert.Equal(command.LastName, fishermanAddedEvent.LastName);
        Assert.Equal(command.DocumentNumber, fishermanAddedEvent.DocumentNumber);
    }
}
