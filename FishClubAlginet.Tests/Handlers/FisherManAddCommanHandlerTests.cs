using FishClubAlginet.Core.Domain.Common.Errors;

namespace FishClubAlginet.Tests.Handlers;

public class FisherManAddCommanHandlerTests
{
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockRepository;
    public FisherManAddCommanHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<Fisherman, int>>();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnFishermanId()
    {
        // Arrange
        var handler = new FisherManAddCommandHandler(_mockRepository.Object);
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
    }


    [Fact]
    public async Task Handle_InvalidCommand_ShouldReturnError()
    {
        // Arrange
        var mockRepository = new Mock<IGenericRepository<Fisherman, int>>();

        var handler = new FisherManAddCommandHandler(mockRepository.Object);
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
    }
}
