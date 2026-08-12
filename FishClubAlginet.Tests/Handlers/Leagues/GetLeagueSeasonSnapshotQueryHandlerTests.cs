using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class GetLeagueSeasonSnapshotQueryHandlerTests
{
    private readonly Mock<IGenericRepository<LeagueSeasonSnapshot, Guid>> _mockRepository;
    private readonly GetLeagueSeasonSnapshotQueryHandler _handler;

    public GetLeagueSeasonSnapshotQueryHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<LeagueSeasonSnapshot, Guid>>();
        _handler = new GetLeagueSeasonSnapshotQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ExistingSnapshot_ShouldReturnDto()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var snapshot = LeagueSeasonSnapshot.Create(leagueId, 2025, "{\"test\":true}");
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<LeagueSeasonSnapshot> { snapshot }.AsQueryable());

        var query = new GetLeagueSeasonSnapshotQuery(leagueId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.LeagueId.Should().Be(leagueId);
        result.Value.Year.Should().Be(2025);
        result.Value.SnapshotDataJson.Should().Be("{\"test\":true}");
    }

    [Fact]
    public async Task Handle_SnapshotNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<LeagueSeasonSnapshot>().AsQueryable());

        var query = new GetLeagueSeasonSnapshotQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Snapshot.NotFound");
    }
}
