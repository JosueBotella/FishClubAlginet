using FishClubAlginet.Core.Domain.ValueObjects;
using FishClubAlginet.Application.Features.Fishermen;

namespace FishClubAlginet.Tests.Handlers;

public class FisherManGetAllQueriesHandlerTests
{
    private readonly Mock<IGenericRepository<Fisherman, int>> _mockRepository;

    public FisherManGetAllQueriesHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<Fisherman, int>>();
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnAllFishermen()
    {
        // Arrange
        var fishermen = new List<Fisherman>
        {
            new Fisherman
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = TypeNationalIdentifier.Dni,
                DocumentNumber = "12345678A",
                FederationLicense = "FED001",
                Address = new Address
                {
                    City = "Madrid",
                    Province = "Madrid",
                    Street = "Calle Principal",
                    ZipCode = "28001"
                }
            },
            new Fisherman
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = new(1995, 8, 22, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = TypeNationalIdentifier.Nie,
                DocumentNumber = "87654321B",
                FederationLicense = "FED002",
                Address = new Address
                {
                    City = "Barcelona",
                    Province = "Barcelona",
                    Street = "Avenida Secundaria",
                    ZipCode = "08001"
                }
            }
        };

        var query = new FisherManGetAllQuery();
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Returns(fishermen.AsQueryable());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("John", result.Value[0].FirstName);
        Assert.Equal("Doe", result.Value[0].LastName);
        Assert.Equal("Jane", result.Value[1].FirstName);
        Assert.Equal("Smith", result.Value[1].LastName);
        Assert.Equal("Madrid", result.Value[0].AddressCity);
        Assert.Equal("Barcelona", result.Value[1].AddressCity);
    }

    [Fact]
    public async Task Handle_EmptyRepository_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new FisherManGetAllQuery();
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Returns(new List<Fisherman>().AsQueryable());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_QueryThrowsException_ShouldReturnError()
    {
        // Arrange
        var query = new FisherManGetAllQuery();
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Throws(new Exception(ValidatorsConstants.UnexpectedErrorMessage));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(ValidatorsConstants.UnexpectedErrorCode, result.Errors.Select(e => e.Code));
        Assert.Contains(ValidatorsConstants.UnexpectedErrorMessage, result.Errors.Select(e => e.Description));
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnFishermanWithCorrectAddressData()
    {
        // Arrange
        var fishermen = new List<Fisherman>
        {
            new Fisherman
            {
                Id = 1,
                FirstName = "Carlos",
                LastName = "García",
                DateOfBirth = new(1988, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = TypeNationalIdentifier.Dni,
                DocumentNumber = "11223344C",
                FederationLicense = "FED003",
                Address = new Address
                {
                    City = "Valencia",
                    Province = "Valencia",
                    Street = "Calle del Mar",
                    ZipCode = "46001"
                }
            }
        };

        var query = new FisherManGetAllQuery();
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Returns(fishermen.AsQueryable());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        var fisherman = result.Value[0];
        Assert.Equal("Carlos", fisherman.FirstName);
        Assert.Equal("García", fisherman.LastName);
        Assert.Equal("Valencia", fisherman.AddressCity);
        Assert.Equal("Valencia", fisherman.AddressProvince);
        Assert.Equal(TypeNationalIdentifier.Dni, fisherman.DocumentType);
        Assert.Equal("11223344C", fisherman.DocumentNumber);
    }
}
