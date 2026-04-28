using FluentAssertions;

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

        var query = new FisherManGetAllQuery(0,10,null);
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Returns(fishermen.AsQueryable());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal("John", result.Value.Items[0].FirstName);
        Assert.Equal("Doe", result.Value.Items[0].LastName);
        Assert.Equal("Jane", result.Value.Items[1].FirstName);
        Assert.Equal("Smith", result.Value.Items[1].LastName);
        Assert.Equal("Madrid", result.Value.Items[0].AddressCity);
        Assert.Equal("Barcelona", result.Value.Items[1].AddressCity);
    }

    [Fact]
    public async Task Handle_EmptyRepository_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new FisherManGetAllQuery(0, 10, null);
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Returns(new List<Fisherman>().AsQueryable());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_QueryThrowsException_ShouldReturnError()
    {
        // Arrange
        var query = new FisherManGetAllQuery(0, 10, null);
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

        var query = new FisherManGetAllQuery(0, 10, null);
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Returns(fishermen.AsQueryable());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value.Items);
        var fisherman = result.Value.Items[0];
        Assert.Equal("Carlos", fisherman.FirstName);
        Assert.Equal("García", fisherman.LastName);
        Assert.Equal("Valencia", fisherman.AddressCity);
        Assert.Equal("Valencia", fisherman.AddressProvince);
        Assert.Equal(TypeNationalIdentifier.Dni, fisherman.DocumentType);
        Assert.Equal("11223344C", fisherman.DocumentNumber);
    }

    [Fact]
    public async Task Handle_WhenShowDeletedFalse_ShouldExcludeDeletedFishermen()
    {
        // Arrange
        var fishermen = new List<Fisherman>
        {
            new Fisherman
            {
                Id = 1,
                FirstName = "Active",
                LastName = "Fisher",
                DateOfBirth = new(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = TypeNationalIdentifier.Dni,
                DocumentNumber = "11111111A",
                Address = new Address { City = "Valencia", Province = "Valencia", Street = "Calle A", ZipCode = "46001" },
                IsDeleted = false
            },
            new Fisherman
            {
                Id = 2,
                FirstName = "Deleted",
                LastName = "Fisher",
                DateOfBirth = new(1985, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = TypeNationalIdentifier.Dni,
                DocumentNumber = "22222222B",
                Address = new Address { City = "Madrid", Province = "Madrid", Street = "Calle B", ZipCode = "28001" },
                IsDeleted = true,
                DeletedTimeUtc = DateTime.UtcNow
            }
        };

        var query = new FisherManGetAllQuery(0, 10, null, ShowDeleted: false);
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Returns(fishermen.AsQueryable());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].FirstName.Should().Be("Active");
        result.Value.Items[0].IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenShowDeletedTrue_ShouldReturnOnlyDeletedFishermen()
    {
        // Arrange
        var fishermen = new List<Fisherman>
        {
            new Fisherman
            {
                Id = 1,
                FirstName = "Active",
                LastName = "Fisher",
                DateOfBirth = new(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = TypeNationalIdentifier.Dni,
                DocumentNumber = "11111111A",
                Address = new Address { City = "Valencia", Province = "Valencia", Street = "Calle A", ZipCode = "46001" },
                IsDeleted = false
            },
            new Fisherman
            {
                Id = 2,
                FirstName = "Deleted",
                LastName = "Fisher",
                DateOfBirth = new(1985, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = TypeNationalIdentifier.Dni,
                DocumentNumber = "22222222B",
                Address = new Address { City = "Madrid", Province = "Madrid", Street = "Calle B", ZipCode = "28001" },
                IsDeleted = true,
                DeletedTimeUtc = DateTime.UtcNow
            }
        };

        var query = new FisherManGetAllQuery(0, 10, null, ShowDeleted: true);
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Returns(fishermen.AsQueryable());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].FirstName.Should().Be("Deleted");
        result.Value.Items[0].IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenShowDeletedFalse_ShouldDefaultToActiveOnlyView()
    {
        // Arrange — ShowDeleted no especificado, debe ser false por defecto
        var fishermen = new List<Fisherman>
        {
            new Fisherman
            {
                Id = 1,
                FirstName = "Active",
                LastName = "Fisher",
                DateOfBirth = new(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = TypeNationalIdentifier.Dni,
                DocumentNumber = "33333333C",
                Address = new Address { City = "Alginet", Province = "Valencia", Street = "Calle C", ZipCode = "46250" },
                IsDeleted = false
            },
            new Fisherman
            {
                Id = 2,
                FirstName = "Hidden",
                LastName = "Deleted",
                DateOfBirth = new(1988, 3, 20, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = TypeNationalIdentifier.Dni,
                DocumentNumber = "44444444D",
                Address = new Address { City = "Alginet", Province = "Valencia", Street = "Calle D", ZipCode = "46250" },
                IsDeleted = true,
                DeletedTimeUtc = DateTime.UtcNow
            }
        };

        var query = new FisherManGetAllQuery(0, 10, null); // ShowDeleted = false por defecto
        var handler = new FisherManGetAllQueryHandler(_mockRepository.Object);

        _mockRepository
            .Setup(repo => repo.GetAll())
            .Returns(fishermen.AsQueryable());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.Should().NotContain(f => f.IsDeleted);
    }

}
