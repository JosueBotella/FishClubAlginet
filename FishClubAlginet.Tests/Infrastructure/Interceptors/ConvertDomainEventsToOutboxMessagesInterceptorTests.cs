namespace FishClubAlginet.Tests.Infrastructure.Interceptors;

public class ConvertDomainEventsToOutboxMessagesInterceptorTests
{
    [Fact]
    public async Task SavingChangesAsync_ShouldConvertDomainEventsToOutboxMessages_ForBothIntAndGuidEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new ConvertDomainEventsToOutboxMessagesInterceptor())
            .Options;

        using var dbContext = new AppDbContext(options);

        // 1. Entity BaseEntity<int> (Fisherman)
        var fisherman = Fisherman.Create(
            firstName: "Paco",
            lastName: "García",
            dateOfBirth: new DateTime(1990, 1, 1),
            documentType: TypeNationalIdentifier.Dni,
            documentNumber: "12345678Z",
            federationLicense: "FED-001",
            address: new Address { Street = "Calle Mayor 1", City = "Alginet", ZipCode = "46230", Province = "Valencia" }
        );
        fisherman.RaiseDomainEvent(new FishermanAddedDomainEvent
        {
            Id = 1,
            FirstName = "Paco",
            LastName = "García"
        });

        // 2. Entity BaseEntity<Guid> (League)
        var league = League.Create("Liga Mar de Alginet", 2026);
        league.RaiseDomainEvent(new FishermanUpdatedDomainEvent
        {
            Id = 99,
            FirstName = "Liga",
            LastName = "Actualizada"
        });

        dbContext.Fishermen.Add(fisherman);
        dbContext.Leagues.Add(league);

        // Act
        await dbContext.SaveChangesAsync();

        // Assert
        var outboxMessages = await dbContext.OutboxMessages.ToListAsync();
        outboxMessages.Should().HaveCount(2);

        outboxMessages.Should().Contain(m => m.Type == nameof(FishermanAddedDomainEvent));
        outboxMessages.Should().Contain(m => m.Type == nameof(FishermanUpdatedDomainEvent));

        // Ensure domain events were cleared from entities after conversion
        fisherman.GetDomainEvents().Should().BeEmpty();
        league.GetDomainEvents().Should().BeEmpty();
    }
}
