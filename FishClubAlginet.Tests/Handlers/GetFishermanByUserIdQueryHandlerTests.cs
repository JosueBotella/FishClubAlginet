using FishClubAlginet.Application.Features.Fishermen;
using FishClubAlginet.Contracts.Dtos.Responses.Fishermen;
using FluentAssertions;

namespace FishClubAlginet.Tests.Handlers;

public class GetFishermanByUserIdQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Fisherman, int>> _repositoryMock;
    private readonly Mock<ILogger<GetFishermanByUserIdQueryHandler>> _loggerMock;
    private readonly GetFishermanByUserIdQueryHandler _handler;

    private const string UserId = "user-123";

    public GetFishermanByUserIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Fisherman, int>>();
        _loggerMock = new Mock<ILogger<GetFishermanByUserIdQueryHandler>>();
        _handler = new GetFishermanByUserIdQueryHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_FishermanExists_ShouldReturnProfileDto()
    {
        // Arrange
        var fisherman = new Fisherman
        {
            Id = 1,
            UserId = UserId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            DocumentType = TypeNationalIdentifier.Dni,
            DocumentNumber = "12345678A",
            FederationLicense = "FED001",
            RegionalLicense = "REG001",
            IsDeleted = false,
            Address = new Address
            {
                Street = "Calle Principal",
                Number = "10",
                FloorDoor = "2B",
                ZipCode = "46230",
                City = "Alginet",
                Province = "Valencia"
            }
        };

        _repositoryMock.Setup(r => r.GetAll())
            .Returns(new List<Fisherman> { fisherman }.AsQueryable());

        var query = new GetFishermanByUserIdQuery(UserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.DocumentNumber.Should().Be("12345678A");
        result.Value.Street.Should().Be("Calle Principal");
        result.Value.City.Should().Be("Alginet");
        result.Value.Province.Should().Be("Valencia");
        result.Value.FederationLicense.Should().Be("FED001");
        result.Value.RegionalLicense.Should().Be("REG001");
    }

    [Fact]
    public async Task Handle_FishermanNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAll())
            .Returns(new List<Fisherman>().AsQueryable());

        var query = new GetFishermanByUserIdQuery("non-existent-user");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be("Fisherman.NotFound");
    }

    [Fact]
    public async Task Handle_FishermanIsDeleted_ShouldReturnNotFoundError()
    {
        // Arrange
        var deletedFisherman = new Fisherman
        {
            Id = 1,
            UserId = UserId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            DocumentType = TypeNationalIdentifier.Dni,
            DocumentNumber = "12345678A",
            IsDeleted = true,
            Address = new Address
            {
                Street = "Calle Principal",
                Number = "10",
                FloorDoor = "2B",
                ZipCode = "46230",
                City = "Alginet",
                Province = "Valencia"
            }
        };

        _repositoryMock.Setup(r => r.GetAll())
            .Returns(new List<Fisherman> { deletedFisherman }.AsQueryable());

        var query = new GetFishermanByUserIdQuery(UserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be("Fisherman.NotFound");
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAll())
            .Throws(new Exception(ValidatorsConstants.UnexpectedErrorMessage));

        var query = new GetFishermanByUserIdQuery(UserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be(ValidatorsConstants.UnexpectedErrorCode);
        result.Errors[0].Description.Should().Be(ValidatorsConstants.UnexpectedErrorMessage);
    }
}
