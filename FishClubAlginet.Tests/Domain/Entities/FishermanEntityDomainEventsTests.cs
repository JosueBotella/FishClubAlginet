namespace FishClubAlginet.Tests.Domain.Entities;

public class FishermanEntityDomainEventsTests
{
    private static Address BuildAddress() => new Address
    {
        Street = "Calle Mayor 1",
        City = "Alginet",
        ZipCode = "46230",
        Province = "Valencia"
    };

    [Fact]
    public void Create_ShouldInitializePropertiesAndTimestamp()
    {
        // Act
        var fisherman = Fisherman.Create(
            firstName: "Paco",
            lastName: "García",
            dateOfBirth: new DateTime(1990, 5, 20),
            documentType: TypeNationalIdentifier.Dni,
            documentNumber: "12345678Z",
            federationLicense: "FED-100",
            address: BuildAddress());

        // Assert
        fisherman.FirstName.Should().Be("Paco");
        fisherman.LastName.Should().Be("García");
        fisherman.DocumentNumber.Should().Be("12345678Z");
        fisherman.FederationLicense.Should().Be("FED-100");
        fisherman.IsDeleted.Should().BeFalse();
        fisherman.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        fisherman.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void Update_ShouldMutateStateAndUpdateTimestamp()
    {
        // Arrange
        var fisherman = Fisherman.Create(
            firstName: "Paco",
            lastName: "García",
            dateOfBirth: new DateTime(1990, 5, 20),
            documentType: TypeNationalIdentifier.Dni,
            documentNumber: "12345678Z",
            federationLicense: "FED-100",
            address: BuildAddress());

        var newAddress = new Address { Street = "Avenida Nueva 10", City = "Valencia", ZipCode = "46001", Province = "Valencia" };

        // Act
        fisherman.Update("Francisco", "García López", "FED-200", newAddress);

        // Assert
        fisherman.FirstName.Should().Be("Francisco");
        fisherman.LastName.Should().Be("García López");
        fisherman.FederationLicense.Should().Be("FED-200");
        fisherman.Address.Street.Should().Be("Avenida Nueva 10");
        fisherman.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Delete_ShouldSetIsDeletedAndDeletedTimeUtc()
    {
        // Arrange
        var fisherman = Fisherman.Create(
            firstName: "Paco",
            lastName: "García",
            dateOfBirth: new DateTime(1990, 5, 20),
            documentType: TypeNationalIdentifier.Dni,
            documentNumber: "12345678Z",
            federationLicense: "FED-100",
            address: BuildAddress());

        // Act
        fisherman.Delete();

        // Assert
        fisherman.IsDeleted.Should().BeTrue();
        fisherman.DeletedTimeUtc.Should().NotBeNull();
        fisherman.DeletedTimeUtc!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Restore_ShouldRevertDeletedStateAndUpdateTimestamp()
    {
        // Arrange
        var fisherman = Fisherman.Create(
            firstName: "Paco",
            lastName: "García",
            dateOfBirth: new DateTime(1990, 5, 20),
            documentType: TypeNationalIdentifier.Dni,
            documentNumber: "12345678Z",
            federationLicense: "FED-100",
            address: BuildAddress());

        fisherman.Delete();
        fisherman.IsDeleted.Should().BeTrue();

        // Act
        fisherman.Restore();

        // Assert
        fisherman.IsDeleted.Should().BeFalse();
        fisherman.DeletedTimeUtc.Should().BeNull();
        fisherman.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RaiseDomainEvent_And_ClearDomainEvents_ShouldManageEventQueue()
    {
        // Arrange
        var fisherman = Fisherman.Create(
            firstName: "Paco",
            lastName: "García",
            dateOfBirth: new DateTime(1990, 5, 20),
            documentType: TypeNationalIdentifier.Dni,
            documentNumber: "12345678Z",
            federationLicense: "FED-100",
            address: BuildAddress());

        var addedEvent = new FishermanAddedDomainEvent { Id = 1, FirstName = "Paco", LastName = "García" };
        var updatedEvent = new FishermanUpdatedDomainEvent { Id = 1, FirstName = "Paco", LastName = "García" };

        // Act
        fisherman.RaiseDomainEvent(addedEvent);
        fisherman.RaiseDomainEvent(updatedEvent);

        // Assert
        var events = fisherman.GetDomainEvents();
        events.Should().HaveCount(2);
        events.Should().Contain(addedEvent);
        events.Should().Contain(updatedEvent);

        fisherman.ClearDomainEvents();
        fisherman.GetDomainEvents().Should().BeEmpty();
    }
}
