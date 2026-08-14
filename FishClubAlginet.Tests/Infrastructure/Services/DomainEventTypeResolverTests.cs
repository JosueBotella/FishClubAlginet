namespace FishClubAlginet.Tests.Infrastructure.Services;

public class DomainEventTypeResolverTests
{
    private readonly DomainEventTypeResolver _resolver = new();

    [Fact]
    public void Resolve_BySimpleName_ShouldReturnCorrectType()
    {
        // Act
        var type = _resolver.Resolve("FishermanAddedDomainEvent");

        // Assert
        type.Should().NotBeNull();
        type.Should().Be<FishermanAddedDomainEvent>();
    }

    [Fact]
    public void Resolve_ByFullName_ShouldReturnCorrectType()
    {
        // Act
        var typeName = typeof(FishermanAddedDomainEvent).FullName!;
        var type = _resolver.Resolve(typeName);

        // Assert
        type.Should().NotBeNull();
        type.Should().Be<FishermanAddedDomainEvent>();

    }

    [Fact]
    public void Resolve_NonExistentTypeName_ShouldReturnNull()
    {
        // Act
        var type = _resolver.Resolve("NonExistentDomainEvent");

        // Assert
        type.Should().BeNull();
    }

    [Fact]
    public void Resolve_NullOrEmptyTypeName_ShouldReturnNull()
    {
        // Act & Assert
        _resolver.Resolve("").Should().BeNull();
        _resolver.Resolve("   ").Should().BeNull();
    }
}
