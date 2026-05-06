using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class GetActiveLeagueQueryHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockRepository;
    private readonly GetActiveLeagueQueryHandler _handler;

    public GetActiveLeagueQueryHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<League, Guid>>();
        _handler = new GetActiveLeagueQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_HasActiveLeague_ShouldReturnIt()
    {
        // Arrange: dos ligas, una activa
        var inactive = League.Create("Liga 2024", 2024);
        var active = League.Create("Liga 2026", 2026);
        active.Activate();

        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { inactive, active }.AsQueryable());

        // Act
        var result = await _handler.Handle(new GetActiveLeagueQuery(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(active.Id);
        result.Value.Year.Should().Be(2026);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NoActiveLeague_ShouldReturnNotFound()
    {
        // Arrange: ninguna liga activa
        var league1 = League.Create("Liga 2024", 2024);
        var league2 = League.Create("Liga 2025", 2025);
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league1, league2 }.AsQueryable());

        // Act
        var result = await _handler.Handle(new GetActiveLeagueQuery(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
    }

    [Fact]
    public async Task Handle_ActiveButSoftDeleted_ShouldReturnNotFound()
    {
        // Arrange: anomalía — alguien marcó IsActive y luego IsDeleted; el query las debe ignorar
        var ghost = League.Create("Liga fantasma", 2026);
        ghost.Activate();
        ghost.IsDeleted = true;

        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { ghost }.AsQueryable());

        // Act
        var result = await _handler.Handle(new GetActiveLeagueQuery(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
    }
}
