namespace FishClubAlginet.Tests.Domain.Entities;

public class LeagueSeasonSnapshotTests
{
    [Fact]
    public void Create_ValidParameters_ShouldInstantiateLeagueSeasonSnapshot()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var year = 2025;
        var json = "{\"LeagueId\":\"" + leagueId + "\"}";

        // Act
        var snapshot = LeagueSeasonSnapshot.Create(leagueId, year, json);

        // Assert
        snapshot.Should().NotBeNull();
        snapshot.Id.Should().NotBeEmpty();
        snapshot.LeagueId.Should().Be(leagueId);
        snapshot.Year.Should().Be(year);
        snapshot.SnapshotDataJson.Should().Be(json);
        snapshot.ArchivedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
